using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.DataStructures.Versioning;
using WoWsShipBuilder.Desktop.Infrastructure.AwsClient;
using WoWsShipBuilder.Features.Settings;
using WoWsShipBuilder.Infrastructure.ApplicationData;
using WoWsShipBuilder.Infrastructure.GameData;
using WoWsShipBuilder.Infrastructure.HttpClients;
using WoWsShipBuilder.Infrastructure.Localization.Resources;

namespace WoWsShipBuilder.Desktop.Features.Updater;

/// <summary>
/// The default <see cref="ILocalDataUpdater"/> implementation used to update the local application data.
/// </summary>
public class LocalDataUpdater : ILocalDataUpdater
{
    private readonly IAppDataService appDataService;
    private readonly AppSettings appSettings;
    private readonly IDesktopAwsClient awsClient;
    private readonly IFileSystem fileSystem;
    private readonly ILogger<LocalDataUpdater> logger;

    /// <summary>
    /// Creates a new instance of the <see cref="LocalDataUpdater"/> class.
    /// </summary>
    /// <param name="fileSystem">The <see cref="IFileSystem"/> used to access the local file system.</param>
    /// <param name="awsClient">The <see cref="IAwsClient"/> used to access data.</param>
    /// <param name="appDataService">The AppDataHelper used to access local application data.</param>
    /// <param name="appSettings">The current app settings.</param>
    /// <param name="logger">The logger to use.</param>
    public LocalDataUpdater(IFileSystem fileSystem, IDesktopAwsClient awsClient, IAppDataService appDataService, AppSettings appSettings, ILogger<LocalDataUpdater> logger)
    {
        this.fileSystem = fileSystem;
        this.awsClient = awsClient;
        this.appDataService = appDataService;
        this.appSettings = appSettings;
        this.logger = logger;
    }

    public Version SupportedDataStructureVersion => Assembly.GetAssembly(typeof(Ship))!.GetName().Version!;

    /// <summary>
    /// A utility method that combines the update check and execution of necessary updates.
    /// </summary>
    /// <param name="serverType">The <see cref="ServerType"/> for the update check.</param>
    /// <param name="progressTracker">An <see cref="IProgress{T}"/> used to monitor the progress of the update.</param>
    /// <param name="overrideDateCheck">A bool that allows to override the result of the time-based update check evaluation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task RunDataUpdateCheck(ServerType serverType, IProgress<(int, string)> progressTracker, bool overrideDateCheck = false)
    {
        this.logger.LogInformation("UpdateCheck triggered. Checking whether update should execute...");
        if (!overrideDateCheck && !await this.ShouldUpdaterRun(serverType))
        {
            this.logger.LogInformation("Skipping update check");
        }
        else
        {
            this.logger.LogInformation("Starting update check...");
            await this.CheckFilesAndDownloadUpdates(serverType, progressTracker);
            this.logger.LogInformation("Completed update check");
        }

        this.logger.LogInformation("Checking installed localization files...");
        await this.CheckInstalledLocalizations(serverType);
        this.logger.LogInformation("Starting data validation...");
        string dataBasePath = this.appDataService.GetDataPath(serverType);
        var validation = await this.ValidateData(serverType, dataBasePath);
        if (!validation.ValidationStatus)
        {
            this.logger.LogInformation("Data validation failed. Selecting repair method...");
            if (validation.InvalidFiles != null)
            {
                this.logger.LogInformation("List of corrupted files found. Attempting partial repair...");
                await this.awsClient.DownloadFiles(serverType, validation.InvalidFiles.ToList());
            }
            else
            {
                this.logger.LogInformation("No list of corrupted files found. Attempting clean reload of gamedata...");
                this.fileSystem.Directory.Delete(dataBasePath, true);
                await this.CheckFilesAndDownloadUpdates(serverType, progressTracker);
            }

            validation = await this.ValidateData(serverType, dataBasePath);
            if (!validation.ValidationStatus)
            {
                this.logger.LogError("Invalid application data after data reload detected");
            }
            else
            {
                this.logger.LogInformation("Application data has been repaired and validated");
            }
        }
        else
        {
            this.logger.LogInformation("Data validation successful");
        }

        this.logger.LogInformation("Completed update check run");
    }

    /// <summary>
    /// Updates the localization files of the application.
    /// All available localization files will be updated. If the file for the currently selected language is missing, it will be downloaded as well.
    /// </summary>
    /// <param name="serverType">The currently selected <see cref="ServerType"/> of the application.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task UpdateLocalization(ServerType serverType)
    {
        var installedLocales = await this.appDataService.GetInstalledLocales(serverType);

        if (!installedLocales.Contains(this.appSettings.SelectedLanguage.LocalizationFileName + ".json"))
        {
            installedLocales.Add(this.appSettings.SelectedLanguage.LocalizationFileName + ".json");
        }

        var downloadList = installedLocales.Select(locale => ("Localization", locale)).ToList();
        await this.awsClient.DownloadFiles(serverType, downloadList);
    }

    /// <summary>
    /// Compares the versions of the local application data with the newest version available online.
    /// Flags outdated files for update and determines whether image data should be updated and whether it can just use differential updates.
    /// </summary>
    /// <param name="serverType">The currently selected <see cref="ServerType"/> of the application.</param>
    /// <returns>A <see cref="UpdateCheckResult"/> containing the results of the version check.</returns>
    public async Task<UpdateCheckResult> CheckJsonFileVersions(ServerType serverType)
    {
        this.logger.LogInformation("Checking json file versions for server type {ServerType}", serverType.DisplayName());
        VersionInfo onlineVersionInfo = await this.awsClient.DownloadVersionInfo(serverType);
        VersionInfo? localVersionInfo = await this.appDataService.GetCurrentVersionInfo(serverType);

        List<(string, string)> filesToDownload;
        bool shouldImagesUpdate;
        bool canImagesDeltaUpdate;
        bool shouldLocalizationUpdate;

        if (localVersionInfo == null)
        {
            // Local version info file being null means it does not exist or could not be found. Always requires a full data download.
            this.logger.LogInformation("No local version info found. Downloading full data and flagging images for full update");
            filesToDownload = onlineVersionInfo.Categories
                .SelectMany(category => category.Value.Select(file => (category.Key, file.FileName)))
                .ToList();
            shouldImagesUpdate = true;
            canImagesDeltaUpdate = false;
            shouldLocalizationUpdate = true;
        }
        else if (localVersionInfo.CurrentVersionCode < onlineVersionInfo.CurrentVersionCode)
        {
            this.logger.LogInformation(
                "Local data version ({CurrentVersionLocal}) is older than online data version ({CurrentVersionOnline}). Selecting files for update...",
                localVersionInfo.CurrentVersionCode,
                onlineVersionInfo.CurrentVersionCode);
            filesToDownload = new();

            foreach ((string category, var fileVersions) in onlineVersionInfo.Categories)
            {
                localVersionInfo.Categories.TryGetValue(category, out var localCategoryFiles);
                if (localCategoryFiles == null)
                {
                    this.logger.LogInformation("Category {Category} not found in local version info file. Adding category to download list", category);
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
                canImagesDeltaUpdate = onlineVersionInfo.LastVersion != null && onlineVersionInfo.LastVersion.MainVersion == localVersionInfo.CurrentVersion.MainVersion;
            }
            catch (Exception)
            {
                this.logger.LogError(
                    "Unable to strip suffix from a version name. Local version name: {LocalVersion}, Online version name: {OnlineVersion}",
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

        var versionName = onlineVersionInfo.CurrentVersion.MainVersion.ToString(3);

        if (this.SupportedDataStructureVersion.Major < onlineVersionInfo.DataStructuresVersion.Major || this.SupportedDataStructureVersion.Minor < onlineVersionInfo.DataStructuresVersion.Minor)
        {
            this.logger.LogWarning("Online data is incompatible with this application version. Online data version: {}, maximum supported version: {}", this.SupportedDataStructureVersion, onlineVersionInfo.DataStructuresVersion);
            return new(new(), false, false, false, versionName, serverType);
        }

        if (this.SupportedDataStructureVersion.Build != onlineVersionInfo.DataStructuresVersion.Build)
        {
            this.logger.LogWarning("The build version of the online data is different to the currently supported version. Some data may be unavailable");
        }
        else if (this.SupportedDataStructureVersion.Major > onlineVersionInfo.DataStructuresVersion.Major || this.SupportedDataStructureVersion.Minor > onlineVersionInfo.DataStructuresVersion.Minor)
        {
            this.logger.LogWarning("Online data version is behind the currently supported data version. Loading data may crash the application");
        }

        return new(filesToDownload, shouldImagesUpdate, canImagesDeltaUpdate, shouldLocalizationUpdate, versionName, serverType);
    }

    /// <summary>
    /// Validates local application data by comparing the version info file with the actual local files.
    /// The method does not modify any local file and does not validate the content of the files.
    /// If a file exists, the validation will consider it to be valid.
    /// </summary>
    /// <param name="serverType">The currently selected <see cref="ServerType"/> of the application.</param>
    /// <param name="dataBasePath">The base file system path to the directory of the local VersionInfo file.</param>
    /// <returns><see langword="true"/> if the local data matches the structure of the version info file, <see langword="false"/> otherwise.</returns>
    public async Task<ValidationResult> ValidateData(ServerType serverType, string dataBasePath)
    {
        var versionInfo = await this.appDataService.GetCurrentVersionInfo(serverType);
        if (versionInfo == null)
        {
            this.logger.LogError("VersionInfo does not exist. AppData validation failed");
            return new(false);
        }

        var missingFiles = new List<(string, string)>();
        var categoryFiles = versionInfo.Categories.SelectMany(category => category.Value.Select(file => (category.Key, file)));
        foreach ((string category, var file) in categoryFiles)
        {
            string path = this.fileSystem.Path.Combine(dataBasePath, category, file.FileName);
            if (!this.fileSystem.File.Exists(path))
            {
                missingFiles.Add((category, file.FileName));
                continue;
            }

            await using var fs = this.fileSystem.File.OpenRead(path);
            string hash = FileVersion.ComputeChecksum(fs);
            if (!hash.Equals(file.Checksum))
            {
                missingFiles.Add((category, file.FileName));
            }
        }

        if (!missingFiles.Any())
        {
            return new(true);
        }

        this.logger.LogWarning("Missing files during data validation. These files were missing: {MissingFiles}", string.Join(", ", missingFiles));
        return new(false) { InvalidFiles = missingFiles };
    }

    /// <summary>
    /// Checks whether the update should be executed or not.
    /// </summary>
    /// <param name="serverType">The currently selected <see cref="ServerType"/> of the application.</param>
    /// <returns><see langword="true"/> if the updater should run, <see langword="false"/> otherwise.</returns>
    public async Task<bool> ShouldUpdaterRun(ServerType serverType)
    {
        var today = DateTime.Today;
        return this.appSettings.LastDataUpdateCheck == null || (today - this.appSettings.LastDataUpdateCheck).Value.TotalDays > 1 ||
               await this.appDataService.GetCurrentVersionInfo(serverType) == null;
    }

    public async Task CheckInstalledLocalizations(ServerType serverType)
    {
        List<string> installedLocales = await this.appDataService.GetInstalledLocales(serverType, false);
        if (!installedLocales.Contains(this.appSettings.SelectedLanguage.LocalizationFileName))
        {
            this.logger.LogInformation("Selected localization is not installed. Downloading file...");
            string localizationFile = this.appSettings.SelectedLanguage.LocalizationFileName + ".json";
            await this.awsClient.DownloadFiles(serverType, new() { ("Localization", localizationFile) });
            this.logger.LogInformation("Downloaded localization file for selected localization. Updating localizer data...");
        }
        else
        {
            this.logger.LogInformation("Selected localization is installed");
        }
    }

    /// <summary>
    /// Helper method to manage the download of the results of a version check.
    /// </summary>
    /// <param name="serverType">The currently selected <see cref="ServerType"/> of the application.</param>
    /// <param name="progressTracker">An <see cref="IProgress{T}"/> used to monitor the progress of the update.</param>
    private async Task CheckFilesAndDownloadUpdates(ServerType serverType, IProgress<(int, string)> progressTracker)
    {
        UpdateCheckResult checkResult = await this.CheckJsonFileVersions(serverType);
        if (checkResult.AvailableFileUpdates.Any())
        {
            this.logger.LogInformation("Updating {AvailableUpdateCount} files...", checkResult.AvailableFileUpdates.Count);
            progressTracker.Report((1, nameof(Translation.SplashScreen_Json)));
            await this.awsClient.DownloadFiles(serverType, checkResult.AvailableFileUpdates);
        }

        if (checkResult.ShouldLocalizationUpdate)
        {
            this.logger.LogInformation("Updating installed localizations...");
            await this.UpdateLocalization(serverType);
        }

        if (checkResult.ShouldImagesUpdate)
        {
            this.logger.LogInformation("Updating images. Can delta update: {CanImagesDeltaUpdate}", checkResult.CanImagesDeltaUpdate);
            await this.ImageUpdate(progressTracker, checkResult.CanImagesDeltaUpdate, checkResult.DataVersionName);
        }
    }

    /// <summary>
    /// Updater method to update the local image data after an update check.
    /// </summary>
    /// <param name="progressTracker">An <see cref="IProgress{T}"/> used to report the progress of the operation.</param>
    /// <param name="canDeltaUpdate">A bool indicating whether images can be updated differentially.</param>
    /// <param name="versionName">The version name of the new image data, needs to be identical to the WG version name.</param>
    private async Task ImageUpdate(IProgress<(int, string)> progressTracker, bool canDeltaUpdate, string? versionName)
    {
        string imageBasePath = this.appDataService.AppDataImageDirectory;
        var shipImageDirectory = this.fileSystem.DirectoryInfo.New(this.fileSystem.Path.Combine(imageBasePath, "Ships"));
        if (!shipImageDirectory.Exists || !shipImageDirectory.GetFiles().Any() || !canDeltaUpdate)
        {
            progressTracker.Report((2, nameof(Translation.SplashScreen_ShipImages)));
            await this.awsClient.DownloadImages(this.fileSystem);
        }
        else
        {
            progressTracker.Report((2, nameof(Translation.SplashScreen_ShipImages)));
            await this.awsClient.DownloadImages(this.fileSystem, versionName);
        }
    }
}
