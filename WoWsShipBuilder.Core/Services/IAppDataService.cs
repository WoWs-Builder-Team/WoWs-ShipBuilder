using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WoWsShipBuilder.Core.BuildCreator;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Core.Services
{
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
        /// Read a dictionary from the local app data directory.
        /// </summary>
        /// <param name="nation">The selected nation.</param>
        /// <param name="serverType">The selected server type.</param>
        /// <typeparam name="T">The data type of the values of the dictionary.</typeparam>
        /// <returns>A dictionary containing the deserialized file content.</returns>
        Task<Dictionary<string, T>?> ReadLocalJsonData<T>(Nation nation, ServerType serverType);

        /// <summary>
        /// Read the current version info from the app data directory.
        /// </summary>
        /// <param name="serverType">The selected server type.</param>
        /// <returns>The local VersionInfo or null if none was found.</returns>
        Task<VersionInfo?> ReadLocalVersionInfo(ServerType serverType);

        Task<List<ShipSummary>> GetShipSummaryList(ServerType serverType);

        Task LoadNationFiles(Nation nation);

        /// <summary>
        /// Reads projectile data from the current <see cref="AppData.ProjectileCache"/> and returns the result.
        /// Initializes the data for the nation of the provided projectile name if it is not loaded already.
        /// </summary>
        /// <param name="projectileName">The name of the projectile, <b>MUST</b> start with the projectile's index.</param>
        /// <returns>The projectile for the specified name.</returns>
        /// <exception cref="KeyNotFoundException">Occurs if the projectile name does not exist in the projectile data.</exception>
        Task<Projectile> GetProjectile(string projectileName);

        /// <summary>
        /// Reads projectile data from the current <see cref="AppData.ProjectileCache"/> and returns the result, cast to the requested type.
        /// Initializes the data for the nation of the provided projectile name if it is not loaded already.
        /// </summary>
        /// <param name="projectileName">The name of the projectile, <b>MUST</b> start with the projectile's index.</param>
        /// <typeparam name="T">
        /// The requested return type. Must be a sub type of <see cref="Projectile"/>.
        /// The caller is responsible for ensuring that the requested projectile is of the specified type.<br/>
        /// <b>This method does not handle exceptions caused by an invalid cast!</b>
        /// </typeparam>
        /// <returns>The requested projectile, cast to the specified type.</returns>
        /// <exception cref="KeyNotFoundException">Occurs if the projectile name does not exist in the projectile data.</exception>
        Task<T> GetProjectile<T>(string projectileName) where T : Projectile;

        /// <summary>
        /// Reads aircraft data from the current <see cref="AppData.AircraftCache"/> and returns the result.
        /// Initializes the data for the nation of the provided aircraft name if it is not loaded already.
        /// </summary>
        /// <param name="aircraftName">The name of the aircraft, <b>MUST</b> start with the aircraft's index.</param>
        /// <returns>The requested aircraft.</returns>
        /// <exception cref="KeyNotFoundException">Occurs if the aircraft name does not exist in the aircraft data.</exception>
        Task<Aircraft> GetAircraft(string aircraftName);

        Task<Dictionary<string, string>?> ReadLocalizationData(ServerType serverType, string language);

        Task<Ship?> GetShipFromSummary(ShipSummary summary, bool changeDictionary = true);

        /// <summary>
        /// Save string compressed <see cref="Build"/> to the disk.
        /// </summary>
        void SaveBuilds();

        void LoadBuilds();

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

        protected static string GetNationString(Nation nation)
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

        protected static string GetCategoryString<T>()
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
}
