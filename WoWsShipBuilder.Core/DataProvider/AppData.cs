using System.Collections.Generic;
using System.Text;
using WoWsShipBuilder.Core.BuildCreator;
using WoWsShipBuilder.Core.Settings;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.Core.DataProvider
{
    public static class AppData
    {
        public static bool IsInitialized { get; set; } = false;

        public static AppSettings Settings { get; set; } = new();

        public static string? DataVersion { get; set; } = default!;

        public static Dictionary<string, string>? TranslationsData { get; set; }

        public static Dictionary<string, Ship>? ShipDictionary { get; set; }

        public static Nation? CurrentLoadedNation { get; set; }

        public static Dictionary<string, Projectile>? ProjectileList { get; set; }

        public static Dictionary<string, Aircraft>? AircraftList { get; set; }

        public static Dictionary<string, Consumable>? ConsumableList { get; set; }

        public static List<ShipSummary>? ShipSummaryList { get; set; }

        public static List<Build> Builds { get; set; } = new();

#if DEBUG
        public static bool IsDebug => true;
#else
        public static bool IsDebug => false;
#endif

        public static string GenerateLogDump()
        {
            var result = new StringBuilder();
            result.Append("DataVersion: ").AppendLine(DataVersion)
                .Append("CurrentNation: ").AppendLine(CurrentLoadedNation?.ToString())
                .Append("ServerType: ").AppendLine(Settings.SelectedServerType.ToString());
            return result.ToString();
        }
    }
}
