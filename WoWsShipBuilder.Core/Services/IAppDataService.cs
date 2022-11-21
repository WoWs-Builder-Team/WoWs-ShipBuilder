using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Aircraft;
using WoWsShipBuilder.DataStructures.Captain;
using WoWsShipBuilder.DataStructures.Consumable;
using WoWsShipBuilder.DataStructures.Exterior;
using WoWsShipBuilder.DataStructures.Module;
using WoWsShipBuilder.DataStructures.Projectile;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.DataStructures.Upgrade;
using WoWsShipBuilder.DataStructures.Versioning;

namespace WoWsShipBuilder.Core.Services;

public interface IAppDataService
{
    string DefaultAppDataDirectory { get; }

    string AppDataDirectory { get; }

    string AppDataImageDirectory { get; }

    static Nation GetNationFromIndex(string index)
    {
        if (index.Length < 7)
        {
            Logging.Logger.Error("Invalid index, received value {}.", index);
            return Nation.Common;
        }

        return index[1] switch
        {
            'A' => Nation.Usa,
            'B' => Nation.UnitedKingdom,
            'F' => Nation.France,
            'G' => Nation.Germany,
            'H' => Nation.Netherlands,
            'I' => Nation.Italy,
            'J' => Nation.Japan,
            'R' => Nation.Russia,
            'S' => Nation.Spain,
            'U' => Nation.Commonwealth,
            'V' => Nation.PanAmerica,
            'W' => Nation.Europe,
            'Z' => Nation.PanAsia,
            _ => Nation.Common,
        };
    }

    /// <summary>
    /// Read the current version info from the app data directory.
    /// </summary>
    /// <param name="serverType">The selected server type.</param>
    /// <returns>The local VersionInfo or null if none was found.</returns>
    Task<VersionInfo?> GetCurrentVersionInfo(ServerType serverType);

    Task<Dictionary<string, string>?> ReadLocalizationData(ServerType serverType, string language);

    Ship GetShipFromSummary(ShipSummary summary);

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

    public static string GetNationString(Nation nation)
    {
        return nation switch
        {
            Nation.PanAmerica => "Pan_America",
            Nation.PanAsia => "Pan_Asia",
            Nation.UnitedKingdom => "United_Kingdom",
            Nation.Usa => "USA",
            _ => nation.ToString() ?? throw new InvalidOperationException("Unable to retrieve enum name."),
        };
    }

    public static string GetCategoryString<T>()
    {
        return typeof(T) switch
        {
            var consumableType when consumableType == typeof(Consumable) => "Ability",
            var aircraftType when aircraftType == typeof(Aircraft) => "Aircraft",
            var crewType when crewType == typeof(Captain) => "Crew",
            var exteriorType when exteriorType == typeof(Exterior) => "Exterior",
            var gunType when gunType == typeof(Gun) => "Gun",
            var modernizationType when modernizationType == typeof(Modernization) => "Modernization",
            var projectileType when typeof(Projectile).IsAssignableFrom(projectileType) => "Projectile",
            var shipType when shipType == typeof(Ship) => "Ship",
            var moduleType when moduleType == typeof(Module) => "Unit",
            _ => throw new InvalidOperationException(),
        };
    }
}
