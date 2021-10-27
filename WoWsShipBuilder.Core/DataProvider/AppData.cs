using System.Collections.Generic;
using WoWsShipBuilder.Core.Settings;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.Core.DataProvider
{
    public static class AppData
    {
        public static bool IsInitialized { get; set; } = false;

        public static AppSettings Settings { get; set; } = new();

        public static Dictionary<string, string>? TranslationsData { get; set; }

        public static Dictionary<string, Ship>? ShipDictionary { get; set; }

        public static Nation? CurrentLoadedNation { get; set; }

        public static Dictionary<string, Projectile>? ProjectileList { get; set; }

        public static Dictionary<string, Aircraft>? AircraftList { get; set; }

        public static List<ShipSummary>? ShipSummaryList { get; set; }

#if DEBUG
        public static bool IsDebug => true;
#else
        public static bool IsDebug => false;
#endif
    }
}
