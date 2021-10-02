using System.Collections.Generic;
using WoWsShipBuilder.Core.Settings;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.Core.DataProvider
{
    public static class AppData
    {
        public static AppSettings Settings { get; set; } = null!;

        public static Dictionary<string, string>? TranslationsData { get; set; }

        public static Dictionary<string, Ship>? ShipList { get; set; }

        public static Dictionary<string, Projectile>? ProjectileList { get; set; }

        public static Dictionary<string, Aircraft>? AircraftList { get; set; }

        public static List<ShipSummary>? ShipSummaryList { get; set; }
    }
}
