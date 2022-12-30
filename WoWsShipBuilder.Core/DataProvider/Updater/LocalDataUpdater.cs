using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NLog;
using WoWsShipBuilder.Core.HttpClients;
using WoWsShipBuilder.Core.Localization;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.Core.Settings;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.DataStructures.Versioning;

namespace WoWsShipBuilder.Core.DataProvider.Updater
{
    /// <summary>
    /// The default <see cref="ILocalDataUpdater"/> implementation used to update the local application data.
    /// </summary>
    public class LocalDataUpdater : ILocalDataUpdater
    {
        private readonly IAppDataService appDataService;
        private readonly AppSettings appSettings;
        private readonly IAwsClient awsClient;
        private readonly IFileSystem fileSystem;
        private readonly Logger logger;

        /// <summary>
        /// Creates a new instance of the <see cref="LocalDataUpdater"/> class.
        /// </summary>
        /// <param name="fileSystem">The <see cref="IFileSystem"/> used to access the local file system.</param>
        /// <param name="awsClient">The <see cref="IAwsClient"/> used to access data.</param>
        /// <param name="appDataService">The AppDataHelper used to access local application data.</param>
        public LocalDataUpdater(IFileSystem fileSystem, IAwsClient awsClient, IAppDataService appDataService, AppSettings appSettings)
        {
            this.fileSystem = fileSystem;
            this.awsClient = awsClient;
            this.appDataService = appDataService;
            this.appSettings = appSettings;
            logger = Logging.GetLogger("DataUpdater");
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

            logger.Info("Checking installed localization files...");
            await CheckInstalledLocalizations(serverType);
            logger.Info("Starting data validation...");
            string dataBasePath = appDataService.GetDataPath(serverType);
            var validation = await ValidateData(serverType, dataBasePath);
            if (!validation.ValidationStatus)
            {
                logger.Info("Data validation failed. Selecting repair method...");
                if (validation.InvalidFiles != null)
                {
                    logger.Info("List of corrupted files found. Attempting partial repair...");
                    await awsClient.DownloadFiles(serverType, validation.InvalidFiles.ToList());
                }
                else
                {
                    logger.Info("No list of corrupted files found. Attempting clean reload of gamedata...");
                    fileSystem.Directory.Delete(dataBasePath, true);
                    await CheckFilesAndDownloadUpdates(serverType, progressTracker);
                }

                validation = await ValidateData(serverType, dataBasePath);
                if (!validation.ValidationStatus)
                {
                    logger.Error("Invalid application data after data reload detected.");
                }
                else
                {
                    logger.Info("Application data has been repaired and validated.");
                }
            }
            else
            {
                logger.Info("Data validation successful.");
            }

            logger.Info("Completed update check run.");
        }

        /// <summary>
        /// Updates the localization files of the application.
        /// All available localization files will be updated. If the file for the currently selected language is missing, it will be downloaded as well.
        /// </summary>
        /// <param name="serverType">The currently selected <see cref="ServerType"/> of the application.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task UpdateLocalization(ServerType serverType)
        {
            var installedLocales = await appDataService.GetInstalledLocales(serverType);

            if (!installedLocales.Contains(appSettings.SelectedLanguage.LocalizationFileName + ".json"))
            {
                installedLocales.Add(appSettings.SelectedLanguage.LocalizationFileName + ".json");
            }

            var downloadList = installedLocales.Select(locale => ("Localization", locale)).ToList();
            await awsClient.DownloadFiles(serverType, downloadList);
        }

        /// <summary>
        /// Compares the versions of the local application data with the newest version available online.
        /// Flags outdated files for update and determines whether image data should be updated and whether it can just use differential updates.
        /// </summary>
        /// <param name="serverType">The currently selected <see cref="ServerType"/> of the application.</param>
        /// <returns>A <see cref="UpdateCheckResult"/> containing the results of the version check.</returns>
        public async Task<UpdateCheckResult> CheckJsonFileVersions(ServerType serverType)
        {
            logger.Info("Checking json file versions for server type {0}...", serverType);
            VersionInfo onlineVersionInfo = await awsClient.DownloadVersionInfo(serverType);
            VersionInfo? localVersionInfo = await appDataService.GetCurrentVersionInfo(serverType);

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
                filesToDownload = new();

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
                    canImagesDeltaUpdate = onlineVersionInfo.LastVersion != null && onlineVersionInfo.LastVersion.MainVersion == localVersionInfo.CurrentVersion.MainVersion;
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

            var versionName = onlineVersionInfo.CurrentVersion.MainVersion.ToString(3);

            if (SupportedDataStructureVersion.Major < onlineVersionInfo.DataStructuresVersion.Major || SupportedDataStructureVersion.Minor < onlineVersionInfo.DataStructuresVersion.Minor)
            {
                logger.Warn(
                    "Online data is incompatible with this application version. Online data version: {}, maximum supported version: {}",
                    SupportedDataStructureVersion,
                    onlineVersionInfo.DataStructuresVersion);
                return new(new(), false, false, false, versionName, serverType);
            }

            if (SupportedDataStructureVersion.Build != onlineVersionInfo.DataStructuresVersion.Build)
            {
                logger.Warn("The build version of the online data is different to the currently supported version. Some data may be unavailable.");
            }
            else if (SupportedDataStructureVersion.Major > onlineVersionInfo.DataStructuresVersion.Major || SupportedDataStructureVersion.Minor > onlineVersionInfo.DataStructuresVersion.Minor)
            {
                logger.Warn("Online data version is behind the currently supported data version. Loading data may crash the application.");
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
            var versionInfo = await appDataService.GetCurrentVersionInfo(serverType);
            if (versionInfo == null)
            {
                logger.Error("VersionInfo does not exist. AppData validation failed.");
                return new(false);
            }

            var missingFiles = new List<(string, string)>();
            var categoryFiles = versionInfo.Categories.SelectMany(category => category.Value.Select(file => (category.Key, file)));
            foreach ((string category, var file) in categoryFiles)
            {
                string? path = fileSystem.Path.Combine(dataBasePath, category, file.FileName);
                if (!fileSystem.File.Exists(path))
                {
                    missingFiles.Add((category, file.FileName));
                    continue;
                }

                await using var fs = fileSystem.File.OpenRead(path);
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

            logger.Warn("Missing files during data validation. These files were missing: {MissingFiles}", string.Join(", ", missingFiles));
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
            return appSettings.LastDataUpdateCheck == null || (today - appSettings.LastDataUpdateCheck).Value.TotalDays > 1 ||
                   await appDataService.GetCurrentVersionInfo(serverType) == null;
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

        /// <summary>
        /// Helper method to manage the download of the results of a version check.
        /// </summary>
        /// <param name="serverType">The currently selected <see cref="ServerType"/> of the application.</param>
        /// <param name="progressTracker">An <see cref="IProgress{T}"/> used to monitor the progress of the update.</param>
        private async Task CheckFilesAndDownloadUpdates(ServerType serverType, IProgress<(int, string)> progressTracker)
        {
            UpdateCheckResult checkResult = await CheckJsonFileVersions(serverType);
            if (checkResult.AvailableFileUpdates.Any())
            {
                logger.Info("Updating {0} files...", checkResult.AvailableFileUpdates.Count);
                progressTracker.Report((1, nameof(Translation.SplashScreen_Json)));
                await awsClient.DownloadFiles(serverType, checkResult.AvailableFileUpdates);
            }

            if (checkResult.ShouldLocalizationUpdate)
            {
                logger.Info("Updating installed localizations...");
                await UpdateLocalization(serverType);
            }

            if (checkResult.ShouldImagesUpdate)
            {
                logger.Info("Updating images. Can delta update: {0}", checkResult.CanImagesDeltaUpdate);
                await ImageUpdate(progressTracker, checkResult.CanImagesDeltaUpdate, checkResult.DataVersionName);
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
            string imageBasePath = appDataService.AppDataImageDirectory;
            var shipImageDirectory = fileSystem.DirectoryInfo.FromDirectoryName(fileSystem.Path.Combine(imageBasePath, "Ships"));
            if (!shipImageDirectory.Exists || !shipImageDirectory.GetFiles().Any() || !canDeltaUpdate)
            {
                progressTracker.Report((2, nameof(Translation.SplashScreen_ShipImages)));
                await awsClient.DownloadImages(fileSystem);
            }
            else
            {
                progressTracker.Report((2, nameof(Translation.SplashScreen_ShipImages)));
                await awsClient.DownloadImages(fileSystem, versionName);
            }
        }
    }
}
