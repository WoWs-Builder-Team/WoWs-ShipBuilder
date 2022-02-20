using System.Collections.Generic;
using WoWsShipBuilder.Core.BuildCreator;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Core.DataProvider
{
    public interface IAppDataService
    {
        CultureDetails DefaultCultureDetails { get; }

        IEnumerable<CultureDetails> SupportedLanguages { get; }

        /// <summary>
        /// Read a dictionary from the local app data directory.
        /// </summary>
        /// <param name="nation">The selected nation.</param>
        /// <param name="serverType">The selected server type.</param>
        /// <typeparam name="T">The data type of the values of the dictionary.</typeparam>
        /// <returns>A dictionary containing the deserialized file content.</returns>
        Dictionary<string, T>? ReadLocalJsonData<T>(Nation nation, ServerType serverType);

        /// <summary>
        /// Read the current version info from the app data directory.
        /// </summary>
        /// <param name="serverType">The selected server type.</param>
        /// <returns>The local VersionInfo or null if none was found.</returns>
        VersionInfo? ReadLocalVersionInfo(ServerType serverType);

        List<ShipSummary> GetShipSummaryList(ServerType serverType);

        void LoadNationFiles(Nation nation);

        /// <summary>
        /// Reads projectile data from the current <see cref="AppData.ProjectileCache"/> and returns the result.
        /// Initializes the data for the nation of the provided projectile name if it is not loaded already.
        /// </summary>
        /// <param name="projectileName">The name of the projectile, <b>MUST</b> start with the projectile's index.</param>
        /// <returns>The projectile for the specified name.</returns>
        /// <exception cref="KeyNotFoundException">Occurs if the projectile name does not exist in the projectile data.</exception>
        Projectile GetProjectile(string projectileName);

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
        T GetProjectile<T>(string projectileName) where T : Projectile;

        /// <summary>
        /// Reads aircraft data from the current <see cref="AppData.AircraftCache"/> and returns the result.
        /// Initializes the data for the nation of the provided aircraft name if it is not loaded already.
        /// </summary>
        /// <param name="aircraftName">The name of the aircraft, <b>MUST</b> start with the aircraft's index.</param>
        /// <returns>The requested aircraft.</returns>
        /// <exception cref="KeyNotFoundException">Occurs if the aircraft name does not exist in the aircraft data.</exception>
        Aircraft GetAircraft(string aircraftName);

        Dictionary<string, string>? ReadLocalizationData(ServerType serverType, string language);

        Ship? GetShipFromSummary(ShipSummary summary, bool changeDictionary = true);

        /// <summary>
        /// Save string compressed <see cref="Build"/> to the disk.
        /// </summary>
        void SaveBuilds();

        void LoadBuilds();
    }
}
