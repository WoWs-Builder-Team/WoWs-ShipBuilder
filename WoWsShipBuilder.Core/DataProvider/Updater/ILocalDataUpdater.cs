using System;
using System.Threading.Tasks;

namespace WoWsShipBuilder.Core.DataProvider.Updater
{
    /// <summary>
    /// A class that can be used to update and validate local app data for the ship builder application.
    /// An implementation of this interface should never modify local files directly, it can however trigger other operations that can modify files.
    /// </summary>
    public interface ILocalDataUpdater
    {
        /// <summary>
        /// A method that runs a full update check, using one or more of the methods defined by this interface.
        /// <b>This method should be implemented as async method because it launch network requests!</b>
        /// </summary>
        /// <param name="serverType">The currently selected <see cref="ServerType"/> of the application.</param>
        /// <param name="progressTracker">An <see cref="IProgress{T}"/> instance that is used to track and report the progress of the data update.</param>
        /// <param name="overrideDateCheck">A bool forcing the update to run, regardless of other checks.</param>
        /// <returns>The task running the operation.</returns>
        public Task RunDataUpdateCheck(ServerType serverType, IProgress<(int, string)> progressTracker, bool overrideDateCheck = false);

        /// <summary>
        /// Checks the versions of the local application data and compares them with the newest version available online.
        /// This method does not modify any files and should only return information about the next actions.
        /// <b>This method should be implemented as async method because it launch network requests!</b>
        /// </summary>
        /// <param name="serverType">The currently selected <see cref="ServerType"/> of the application.</param>
        /// <returns>The task running the operation.</returns>
        public Task<UpdateCheckResult> CheckJsonFileVersions(ServerType serverType);

        /// <summary>
        /// Checks the localization data and triggers update requests for all localizations matching the update criteria.
        /// <b>This method should be implemented as async method because it launch network requests!</b>
        /// </summary>
        /// <param name="serverType">The currently selected <see cref="ServerType"/> of the application.</param>
        /// <returns>The task running the operation.</returns>
        public Task UpdateLocalization(ServerType serverType);

        /// <summary>
        /// Validates local application data by comparing the version info file with the actual local files.
        /// </summary>
        /// <param name="serverType">The currently selected <see cref="ServerType"/> of the application.</param>
        /// <param name="dataBasePath">The base file system path to the directory of the local VersionInfo file.</param>
        /// <returns><see langword="true"/> if the local data matches the structure of the version info file, <see langword="false"/> otherwise.</returns>
        public bool ValidateData(ServerType serverType, string dataBasePath);

        /// <summary>
        /// Checks whether the update should be executed or not.
        /// </summary>
        /// <param name="serverType">The currently selected <see cref="ServerType"/> of the application.</param>
        /// <returns><see langword="true"/> if the updater should run, <see langword="false"/> otherwise.</returns>
        public bool ShouldUpdaterRun(ServerType serverType);
    }
}
