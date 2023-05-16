using WoWsShipBuilder.DataStructures.Versioning;
using WoWsShipBuilder.Infrastructure.GameData;

namespace WoWsShipBuilder.Infrastructure.Data;

public interface IAppDataService
{
    string DefaultAppDataDirectory { get; }

    string AppDataDirectory { get; }

    string AppDataImageDirectory { get; }

    /// <summary>
    /// Read the current version info from the app data directory.
    /// </summary>
    /// <param name="serverType">The selected server type.</param>
    /// <returns>The local VersionInfo or null if none was found.</returns>
    Task<VersionInfo?> GetCurrentVersionInfo(ServerType serverType);

    Task<Dictionary<string, string>?> ReadLocalizationData(ServerType serverType, string language);

    string GetDataPath(ServerType serverType);

    /// <summary>
    /// Gets the localization directory for the selected server type.
    /// </summary>
    /// <param name="serverType">The selected server type.</param>
    /// <returns>The directory path of the current localization directory.</returns>
    string GetLocalizationPath(ServerType serverType);

    /// <summary>
    /// Find the list of currently installed localizations.
    /// </summary>
    /// <param name="serverType">The selected server type.</param>
    /// <param name="includeFileType">Specifies whether the list of installed locales should contain the file extensions for each file.</param>
    /// <returns>A possibly empty list of installed locales.</returns>
    Task<List<string>> GetInstalledLocales(ServerType serverType, bool includeFileType = true);

    Task LoadLocalFilesAsync(ServerType serverType);
}
