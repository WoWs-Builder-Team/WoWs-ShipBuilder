using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NLog;
using WoWsShipBuilder.Core.HttpClients;
using WoWsShipBuilder.Core.HttpResponses;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Core.DataProvider.Updater
{
    /// <summary>
    /// The default <see cref="ILocalDataUpdater"/> implementation used to update the local application data.
    /// </summary>
    public class LocalDataUpdater : ILocalDataUpdater
    {
        private readonly AppDataHelper appDataHelper;
        private readonly IAwsClient awsClient;
        private readonly IFileSystem fileSystem;
        private readonly Logger logger;

        /// <summary>
        /// Creates a new instance of the <see cref="LocalDataUpdater"/> class.
        /// </summary>
        /// <param name="fileSystem">The <see cref="IFileSystem"/> used to access the local file system.</param>
        /// <param name="awsClient">The <see cref="IAwsClient"/> used to access data.</param>
        /// <param name="appDataHelper">The AppDataHelper used to access local application data.</param>
        public LocalDataUpdater(IFileSystem fileSystem, IAwsClient awsClient, AppDataHelper appDataHelper)
        {
            this.fileSystem = fileSystem;
            this.awsClient = awsClient;
            this.appDataHelper = appDataHelper;
            logger = Logging.GetLogger("DataUpdater");
        }

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
            if (!overrideDateCheck && !ShouldUpdaterRun(serverType))
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
            string dataBasePath = appDataHelper.GetDataPath(serverType);
            if (!ValidateData(serverType, dataBasePath))
            {
                logger.Info("Data validation failed. Clearing existing app data...");
                fileSystem.Directory.Delete(dataBasePath, true);
                await CheckFilesAndDownloadUpdates(serverType, progressTracker);
                if (!ValidateData(serverType, dataBasePath))
                {
                    logger.Error("Invalid application data after full reload detected.");
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
            var installedLocales = appDataHelper.GetInstalledLocales(serverType);

            if (!installedLocales.Contains(AppData.Settings.SelectedLanguage.LocalizationFileName + ".json"))
            {
                installedLocales.Add(AppData.Settings.SelectedLanguage.LocalizationFileName + ".json");
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
            VersionInfo? localVersionInfo = appDataHelper.ReadLocalVersionInfo(serverType);

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
#pragma warning disable CS0618
                    versionName = onlineVersionInfo.VersionName![..onlineVersionInfo.VersionName.IndexOf('#')];
#pragma warning restore CS0618
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

        /// <summary>
        /// Validates local application data by comparing the version info file with the actual local files.
        /// The method does not modify any local file and does not validate the content of the files.
        /// If a file exists, the validation will consider it to be valid.
        /// </summary>
        /// <param name="serverType">The currently selected <see cref="ServerType"/> of the application.</param>
        /// <param name="dataBasePath">The base file system path to the directory of the local VersionInfo file.</param>
        /// <returns><see langword="true"/> if the local data matches the structure of the version info file, <see langword="false"/> otherwise.</returns>
        public bool ValidateData(ServerType serverType, string dataBasePath)
        {
            var versionInfo = appDataHelper.ReadLocalVersionInfo(serverType);
            if (versionInfo == null)
            {
                logger.Error("VersionInfo does not exist. AppData validation failed.");
                return false;
            }

            var missingFiles = new List<string>();
            foreach ((string category, var files) in versionInfo.Categories)
            {
                missingFiles.AddRange(files.Where(file => !fileSystem.File.Exists(fileSystem.Path.Combine(dataBasePath, category, file.FileName)))
                    .Select(file => $"{category}: {file.FileName}"));
            }

            if (!missingFiles.Any())
            {
                return true;
            }

            logger.Warn("Missing files during data validation. These files were missing: {MissingFiles}", string.Join(", ", missingFiles));
            return false;
        }

        /// <summary>
        /// Checks whether the update should be executed or not.
        /// </summary>
        /// <param name="serverType">The currently selected <see cref="ServerType"/> of the application.</param>
        /// <returns><see langword="true"/> if the updater should run, <see langword="false"/> otherwise.</returns>
        public bool ShouldUpdaterRun(ServerType serverType)
        {
            var today = DateTime.Today;
            return AppData.Settings.LastDataUpdateCheck == null || (today - AppData.Settings.LastDataUpdateCheck).Value.TotalDays > 1 ||
                   appDataHelper.ReadLocalVersionInfo(serverType) == null;
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
                progressTracker.Report((1, "SplashScreen_Json"));
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
        private async Task ImageUpdate(IProgress<(int, string)> progressTracker, bool canDeltaUpdate, string versionName)
        {
            string imageBasePath = appDataHelper.AppDataImageDirectory;
            var shipImageDirectory = fileSystem.DirectoryInfo.FromDirectoryName(fileSystem.Path.Combine(imageBasePath, "Ships"));
            if (!shipImageDirectory.Exists || !shipImageDirectory.GetFiles().Any() || !canDeltaUpdate)
            {
                progressTracker.Report((2, "SplashScreen_ShipImages"));
                await awsClient.DownloadImages(ImageType.Ship);
            }
            else
            {
                progressTracker.Report((2, "SplashScreen_ShipImages"));
                await awsClient.DownloadImages(ImageType.Ship, versionName);
            }

            var camoImageDirectory = fileSystem.DirectoryInfo.FromDirectoryName(fileSystem.Path.Combine(imageBasePath, "Camos"));
            if (!camoImageDirectory.Exists || !camoImageDirectory.GetFiles().Any() || !canDeltaUpdate)
            {
                progressTracker.Report((3, "SplashScreen_CamoImages"));
                await awsClient.DownloadImages(ImageType.Camo);
            }
            else
            {
                progressTracker.Report((3, "SplashScreen_CamoImages"));
                await awsClient.DownloadImages(ImageType.Camo, versionName);
            }
        }

        public async Task CheckInstalledLocalizations(ServerType serverType)
        {
            List<string> installedLocales = appDataHelper.GetInstalledLocales(serverType, false);
            if (!installedLocales.Contains(AppData.Settings.SelectedLanguage.LocalizationFileName))
            {
                logger.Info("Selected localization is not installed. Downloading file...");
                string localizationFile = AppData.Settings.SelectedLanguage.LocalizationFileName + ".json";
                await awsClient.DownloadFiles(serverType, new() { ("Localization", localizationFile) });
                logger.Info("Downloaded localization file for selected localization. Updating localizer data...");
            }
            else
            {
                logger.Info("Selected localization is installed.");
            }
        }
    }
}
