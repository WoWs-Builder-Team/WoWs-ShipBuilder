using System.Reflection;
using NLog;
using WoWsShipBuilder.Core;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.DataProvider.Updater;
using WoWsShipBuilder.Core.HttpClients;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.Core.Settings;
using WoWsShipBuilder.Core.Translations;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Web.Services;

public class WebDataUpdate : ILocalDataUpdater
{
    private readonly IAppDataService appDataService;
    private readonly IAwsClient awsClient;
    private readonly IDataService dataService;
    private readonly Logger logger;
    private readonly AppSettings appSettings;

    /// <summary>
    /// Creates a new instance of the <see cref="LocalDataUpdater"/> class.
    /// </summary>
    /// <param name="dataService">The <see cref="IDataService"/> used to access the data in the indexedDB of the browser.</param>
    /// <param name="awsClient">The <see cref="IAwsClient"/> used to access data.</param>
    /// <param name="appDataService">The AppDataHelper used to access local application data.</param>
    public WebDataUpdate(IDataService dataService, IAwsClient awsClient, IAppDataService appDataService, AppSettings appSettings)
    {
        this.dataService = dataService;
        this.awsClient = awsClient;
        this.appDataService = appDataService;
        this.appSettings = appSettings;
        logger = Logging.GetLogger("DataUpdater");
    }

    public async Task RunDataUpdateCheck(ServerType serverType, IProgress<(int, string)> progressTracker, bool overrideDateCheck = false)
    {
        logger.Info("UpdateCheck triggered. Checking whether update should execute...");
        if (!overrideDateCheck && !await ShouldUpdaterRun(serverType))
        {
            logger.Info("Skipping update check.");
        }
        else
        {
            logger.Info("Starting update check...");
            await CheckFilesAndDownloadUpdates(serverType, progressTracker);
            logger.Info("Completed update check.");
        }

        // TODO Data validation
        // logger.Info("Checking installed localization files...");
        // await CheckInstalledLocalizations(serverType);
        // logger.Info("Starting data validation...");
        // string dataBasePath = appDataService.GetDataPath(serverType);
        // if (!await ValidateData(serverType, dataBasePath))
        // {
        //     logger.Info("Data validation failed. Clearing existing app data...");
        //     fileSystem.Directory.Delete(dataBasePath, true);
        //     await CheckFilesAndDownloadUpdates(serverType, progressTracker);
        //     if (!await ValidateData(serverType, dataBasePath))
        //     {
        //         logger.Error("Invalid application data after full reload detected.");
        //     }
        //     else
        //     {
        //         logger.Info("Application data has been repaired and validated.");
        //     }
        // }
        // else
        // {
        //     logger.Info("Data validation successful.");
        // }
        logger.Info("Completed update check run.");
    }

    private async Task CheckFilesAndDownloadUpdates(ServerType serverType, IProgress<(int, string)> progressTracker)
    {
        UpdateCheckResult checkResult = await CheckJsonFileVersions(serverType);
        if (checkResult.AvailableFileUpdates.Any())
        {
            logger.Info("Updating {0} files...", checkResult.AvailableFileUpdates.Count);
            progressTracker.Report((1, Translation.SplashScreen_Json));
            await awsClient.DownloadFiles(serverType, checkResult.AvailableFileUpdates);
        }

        if (checkResult.ShouldLocalizationUpdate)
        {
            logger.Info("Updating installed localizations...");
            await UpdateLocalization(serverType);
        }

        // TODO images are not downloaded for the web version
        // if (checkResult.ShouldImagesUpdate)
        // {
        //     logger.Info("Updating images. Can delta update: {0}", checkResult.CanImagesDeltaUpdate);
        //     await ImageUpdate(progressTracker, checkResult.CanImagesDeltaUpdate, checkResult.DataVersionName);
        // }
    }

    public async Task<UpdateCheckResult> CheckJsonFileVersions(ServerType serverType)
    {
        logger.Info("Checking json file versions for server type {0}...", serverType);
        VersionInfo onlineVersionInfo = await awsClient.DownloadVersionInfo(serverType);

        VersionInfo? localVersionInfo = await appDataService.GetLocalVersionInfo(serverType);

        List<(string, string)> filesToDownload;
        bool shouldImagesUpdate;
        bool canImagesDeltaUpdate;
        bool shouldLocalizationUpdate;

        if (localVersionInfo == null)
        {
            // Local version info file being null means it does not exist or could not be found. Always requires a full data download.
            logger.Info("No local version info found. Downloading full data and flagging images for full update.");
            filesToDownload = onlineVersionInfo.Categories
                .SelectMany(category => category.Value.Select(file => (category.Key, file.FileName)))
                .ToList();
            shouldImagesUpdate = true;
            canImagesDeltaUpdate = false;
            shouldLocalizationUpdate = true;
        }
        else if (localVersionInfo.CurrentVersionCode < onlineVersionInfo.CurrentVersionCode)
        {
            logger.Info(
                "Local data version ({0}) is older than online data version ({1}). Selecting files for update...",
                localVersionInfo.CurrentVersionCode,
                onlineVersionInfo.CurrentVersionCode);
            filesToDownload = new List<(string, string)>();

            foreach ((string category, var fileVersions) in onlineVersionInfo.Categories)
            {
                localVersionInfo.Categories.TryGetValue(category, out var localCategoryFiles);
                if (localCategoryFiles == null)
                {
                    logger.Info("Category {0} not found in local version info file. Adding category to download list.", category);
                    filesToDownload.AddRange(fileVersions.Select(file => (category, file.FileName)));
                    continue;
                }

                foreach (FileVersion onlineFile in fileVersions)
                {
                    FileVersion? localFile = localCategoryFiles.Find(file =>
                        file.FileName.Equals(onlineFile.FileName, StringComparison.InvariantCultureIgnoreCase));

                    if (localFile == null || localFile.Version < onlineFile.Version)
                    {
                        filesToDownload.Add((category, onlineFile.FileName));
                    }
                }
            }

            shouldImagesUpdate = true;
            shouldLocalizationUpdate = true;
            try
            {
                canImagesDeltaUpdate = onlineVersionInfo.LastVersion != null && onlineVersionInfo.LastVersion.MainVersion == localVersionInfo.CurrentVersion?.MainVersion;
            }
            catch (Exception)
            {
                logger.Error(
                    "Unable to strip suffix from a version name. Local version name: {LocalVersion}, Online version name: {OnlineVersion}.",
                    localVersionInfo.CurrentVersion,
                    onlineVersionInfo.CurrentVersion);
                canImagesDeltaUpdate = false;
            }
        }
        else
        {
            // Default case if there is no update available.
            filesToDownload = new();
            shouldImagesUpdate = false;
            canImagesDeltaUpdate = false;
            shouldLocalizationUpdate = false;
        }

        if (filesToDownload.Any())
        {
            filesToDownload.Add((string.Empty, "VersionInfo.json"));
        }

        string versionName;
        if (onlineVersionInfo.CurrentVersion != null)
        {
            versionName = onlineVersionInfo.CurrentVersion.MainVersion.ToString(3);
        }
        else
        {
            // TODO: remove legacy compatibility code with update 1.5.0
            try
            {
#pragma warning disable CS8602
#pragma warning disable CS0618
                versionName = onlineVersionInfo.VersionName![..onlineVersionInfo.VersionName.IndexOf('#')];
#pragma warning restore CS0618
#pragma warning restore CS8602
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to strip version name of unnecessary suffixes.");
                versionName = "0.11.0"; // Fallback value for now.
            }
        }

        var currentDataStructureVersion = Assembly.GetAssembly(typeof(Ship))!.GetName().Version!;
        if (onlineVersionInfo.DataStructuresVersion.Major > 0 && onlineVersionInfo.DataStructuresVersion > currentDataStructureVersion)
        {
            logger.Warn(
                "Data structures version of online data not supported yet. Highest supported version: {}, online version: {}",
                currentDataStructureVersion,
                onlineVersionInfo.DataStructuresVersion);
        }

        return new(filesToDownload, shouldImagesUpdate, canImagesDeltaUpdate, shouldLocalizationUpdate, versionName, serverType);
    }

    public async Task UpdateLocalization(ServerType serverType)
    {
        var installedLocales = await appDataService.GetInstalledLocales(serverType);

        if (!installedLocales.Contains(appSettings.SelectedLanguage.LocalizationFileName))
        {
            installedLocales.Add(appSettings.SelectedLanguage.LocalizationFileName);
        }

        List<(string, string)> downloadList = installedLocales.Select(locale => ("Localization", locale + ".json")).ToList();
        await awsClient.DownloadFiles(serverType, downloadList);
    }

    // TODO data validation
    public Task<bool> ValidateData(ServerType serverType, string dataBasePath)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> ShouldUpdaterRun(ServerType serverType)
    {
        var today = DateTime.Today;
        return appSettings.LastDataUpdateCheck == null || (today - appSettings.LastDataUpdateCheck).Value.TotalDays > 1 ||
               await appDataService.GetLocalVersionInfo(serverType) == null;
    }

    public async Task CheckInstalledLocalizations(ServerType serverType)
    {
        List<string> installedLocales = await appDataService.GetInstalledLocales(serverType, false);
        if (!installedLocales.Contains(appSettings.SelectedLanguage.LocalizationFileName))
        {
            logger.Info("Selected localization is not installed. Downloading file...");
            string localizationFile = appSettings.SelectedLanguage.LocalizationFileName + ".json";
            await awsClient.DownloadFiles(serverType, new() { ("Localization", localizationFile) });
            logger.Info("Downlaoded localization file for selected localization. Updating localizer data...");
        }
        else
        {
            logger.Info("Selected localization is installed.");
        }
    }
}
