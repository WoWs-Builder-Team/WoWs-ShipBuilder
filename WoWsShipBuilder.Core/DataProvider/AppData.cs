using System.Collections.Generic;
using System.Text;
using WoWsShipBuilder.Core.BuildCreator;
using WoWsShipBuilder.Core.Settings;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Core.DataProvider
{
    /// <summary>
    /// The main storage for app data and settings at runtime.
    /// </summary>
    public static class AppData
    {
        private static Nation? currentLoadedNation;

        /// <summary>
        /// Gets or sets a value indicating whether application settings have been initialized.
        /// </summary>
        public static bool IsInitialized { get; set; }

        /// <summary>
        /// Gets or sets the current <see cref="AppSettings"/> for the application.
        /// </summary>
        // public static AppSettings Settings { get; set; } = new();

        /// <summary>
        /// Gets or sets the current data version name.
        /// </summary>
        public static string? DataVersion { get; set; }

        /// <summary>
        /// Gets or sets the ship dictionary for the currently selected nation.
        /// </summary>
        /// <seealso cref="CurrentLoadedNation"/>
        public static Dictionary<string, Ship>? ShipDictionary { get; set; }

        /// <summary>
        /// Gets or sets the currently selected <see cref="Nation"/>.
        /// When the nation is changed, the <see cref="ProjectileCache"/> and <see cref="AircraftCache"/> are automatically cleared.
        /// </summary>
        public static Nation? CurrentLoadedNation
        {
            get => currentLoadedNation;
            set
            {
                if (value != currentLoadedNation)
                {
                    ProjectileCache.Clear();
                    AircraftCache.Clear();
                }

                currentLoadedNation = value;
            }
        }

        /// <summary>
        /// Gets or sets the list of available consumables.
        /// </summary>
        public static Dictionary<string, Consumable>? ConsumableList { get; set; }

        /// <summary>
        /// Gets the projectile cache, mapping a nation to the actual projectile dictionary for that nation.
        /// This property should not be accessed directly, use <see cref="DesktopAppDataService.GetProjectile"/> instead.
        /// </summary>
        /// <seealso cref="DesktopAppDataService.GetProjectile"/>
        /// <seealso cref="DesktopAppDataService.GetProjectile{T}"/>
        public static Dictionary<Nation, Dictionary<string, Projectile>> ProjectileCache { get; } = new();

        /// <summary>
        /// Gets the aircraft cache, mapping a nation to the actual aircraft dictionary for that nation.
        /// This property should not be accessed directly, use <see cref="DesktopAppDataService.GetAircraft"/> instead.
        /// </summary>
        /// <seealso cref="DesktopAppDataService.GetAircraft"/>
        public static Dictionary<Nation, Dictionary<string, Aircraft>> AircraftCache { get; } = new();

        public static Dictionary<Nation, Dictionary<string, Exterior>> ExteriorCache { get; } = new();

        public static Dictionary<Nation, Dictionary<string, Captain>> CaptainCache { get; } = new();

        public static Dictionary<string, Modernization> ModernizationCache { get; } = new();

        /// <summary>
        /// Gets or sets the list of <see cref="ShipSummary">ship summaries</see> that are currently available.
        /// </summary>
        public static List<ShipSummary>? ShipSummaryList { get; set; }

        /// <summary>
        /// Gets or sets the list of the last used builds.
        /// </summary>
        public static List<Build> Builds { get; set; } = new();

#if DEBUG
        public static bool IsDebug => true;
#else
        public static bool IsDebug => false;
#endif

        public static string GenerateLogDump(AppSettings appSettings)
        {
            var result = new StringBuilder();
            result.Append("DataVersion: ").AppendLine(DataVersion)
                .Append("CurrentNation: ").AppendLine(CurrentLoadedNation?.ToString())
                .Append("ServerType: ").AppendLine(appSettings.ToString());
            return result.ToString();
        }

        public static void ResetCaches()
        {
            ShipSummaryList = null;
            ConsumableList = null;
            ProjectileCache.Clear();
            AircraftCache.Clear();
            ShipDictionary = null;
            DataVersion = null;
            Logging.Logger.Info("Cleared all appdata caches.");
        }
    }
}
