using System.Collections.Generic;

namespace WoWsShipBuilder.Core.DataProvider.Updater
{
    /// <summary>
    /// Represents the results of an update check.
    /// </summary>
    /// <param name="AvailableFileUpdates">A list of files that can be updated.</param>
    /// <param name="ShouldImagesUpdate">Indicates whether the image directory should be updated.</param>
    /// <param name="CanImagesDeltaUpdate">Indicates whether a differential image update is possible.</param>
    /// <param name="ShouldLocalizationUpdate">Indicates whether the localization data should be updated.</param>
    /// <param name="DataVersionName">The name of the new data version.</param>
    /// <param name="ServerType">The server type of the update check.</param>
    public record UpdateCheckResult(
        List<(string category, string fileName)> AvailableFileUpdates,
        bool ShouldImagesUpdate,
        bool CanImagesDeltaUpdate,
        bool ShouldLocalizationUpdate,
        string DataVersionName,
        ServerType ServerType);
}
