using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WoWsShipBuilder.Core.Localization;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Core.DataUI
{
    public record ShipDataContainer(string Index)
    {
        // TODO: check if really necessary
        public static readonly ConcurrentDictionary<string, bool> ExpanderStateMapper = new();

        public SurvivabilityDataContainer SurvivabilityDataContainer { get; set; } = default!;

        public MainBatteryUI? MainBatteryUI { get; set; }

        public SecondaryBatteryUiContainer SecondaryBatteryUI { get; set; } = default!;

        public PingerGunDataContainer? PingerGunDataContainer { get; set; }

        public TorpedoArmamentUI? TorpedoArmamentUI { get; set; }

        public AirstrikeDataContainer? AirstrikeUI { get; set; }

        public AirstrikeDataContainer? AswAirstrikeUI { get; set; }

        public DepthChargesLauncherUI? DepthChargeLauncherUI { get; set; }

        public List<CVAircraftUI>? CVAircraftUI { get; set; }

        public ManeuverabilityDataContainer ManeuverabilityDataContainer { get; set; } = default!;

        public ConcealmentDataContainer ConcealmentDataContainer { get; set; } = default!;

        public AntiAirDataContainer? AntiAirDataContainer { get; set; }

        public List<object> SecondColumnContent { get; set; } = default!;

        public SpecialAbilityDataContainer? SpecialAbilityDataContainer { get; set; }

        public static async Task<ShipDataContainer> FromShip(Ship ship, List<ShipUpgrade> shipConfiguration, List<(string, float)> modifiers, IAppDataService appDataService, ILocalizer localizer)
        {
            var shipDataContainer = new ShipDataContainer(ship.Index)
            {
                // Main weapons
                MainBatteryUI = await MainBatteryUI.FromShip(ship, shipConfiguration, modifiers, appDataService, localizer),
                TorpedoArmamentUI = await TorpedoArmamentUI.FromShip(ship, shipConfiguration, modifiers, appDataService, localizer),
                CVAircraftUI = await DataUI.CVAircraftUI.FromShip(ship, shipConfiguration, modifiers, appDataService),
                PingerGunDataContainer = PingerGunDataContainer.FromShip(ship, shipConfiguration, modifiers),

                // Secondary weapons
                SecondaryBatteryUI = await SecondaryBatteryUiContainer.FromShip(ship, shipConfiguration, modifiers, appDataService, localizer),
                AntiAirDataContainer = AntiAirDataContainer.FromShip(ship, shipConfiguration, modifiers),
                AirstrikeUI = await AirstrikeDataContainer.FromShip(ship, modifiers, false, appDataService),
                AswAirstrikeUI = await AirstrikeDataContainer.FromShip(ship, modifiers, true, appDataService),
                DepthChargeLauncherUI = await DepthChargesLauncherUI.FromShip(ship, shipConfiguration, modifiers, appDataService),

                // Misc
                ManeuverabilityDataContainer = ManeuverabilityDataContainer.FromShip(ship, shipConfiguration, modifiers),
                ConcealmentDataContainer = ConcealmentDataContainer.FromShip(ship, shipConfiguration, modifiers),
                SurvivabilityDataContainer = SurvivabilityDataContainer.FromShip(ship, shipConfiguration, modifiers),
                SpecialAbilityDataContainer = SpecialAbilityDataContainer.FromShip(ship, shipConfiguration, modifiers),
            };

            shipDataContainer.SecondColumnContent = new List<object?>
                {
                    shipDataContainer.AntiAirDataContainer,
                    shipDataContainer.AirstrikeUI,
                    shipDataContainer.AswAirstrikeUI,
                    shipDataContainer.DepthChargeLauncherUI,
                }.Where(item => item != null)
                .Cast<object>()
                .ToList();

            if (shipDataContainer.SecondaryBatteryUI.Secondaries != null)
            {
                shipDataContainer.SecondColumnContent.Insert(0, shipDataContainer.SecondaryBatteryUI);
            }

            return shipDataContainer;
        }
    }
}
