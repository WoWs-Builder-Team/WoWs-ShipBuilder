using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WoWsShipBuilder.Core.Localization;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.DataStructures;

// ReSharper disable InconsistentNaming
namespace WoWsShipBuilder.Core.DataUI
{
    public partial record ShipUI(string Index)
    {
        // TODO: check if really necessary
        public static readonly ConcurrentDictionary<string, bool> ExpanderStateMapper = new();

        public SurvivabilityDataContainer SurvivabilityDataContainer { get; set; } = default!;

        public MainBatteryUI? MainBatteryUI { get; set; }

        public SecondaryBatteryUiContainer SecondaryBatteryUI { get; set; } = default!;

        public PingerGunDataContainer? PingerGunUI { get; set; }

        public TorpedoArmamentUI? TorpedoArmamentUI { get; set; }

        public AirstrikeUI? AirstrikeUI { get; set; }

        public AirstrikeUI? AswAirstrikeUI { get; set; }

        public DepthChargesLauncherUI? DepthChargeLauncherUI { get; set; }

        public List<CVAircraftUI>? CVAircraftUI { get; set; }

        public ManeuverabilityDataContainer ManeuverabilityDataContainer { get; set; } = default!;

        public ConcealmentUIDataContainer ConcealmentUiDataContainer { get; set; } = default!;

        public AntiAirDataContainer? AntiAirUI { get; set; }

        public List<object> SecondColumnContent { get; set; } = default!;

        public SpecialAbilityDataContainer? SpecialAbilityUI { get; set; }

        public static async Task<ShipUI> FromShip(Ship ship, List<ShipUpgrade> shipConfiguration, List<(string, float)> modifiers, IAppDataService appDataService, ILocalizer localizer)
        {
            var shipUI = new ShipUI(ship.Index)
            {
                // Main weapons
                MainBatteryUI = await MainBatteryUI.FromShip(ship, shipConfiguration, modifiers, appDataService, localizer),
                TorpedoArmamentUI = await TorpedoArmamentUI.FromShip(ship, shipConfiguration, modifiers, appDataService, localizer),
                CVAircraftUI = await DataUI.CVAircraftUI.FromShip(ship, shipConfiguration, modifiers, appDataService),
                PingerGunUI = PingerGunDataContainer.FromShip(ship, shipConfiguration, modifiers),

                // Secondary weapons
                SecondaryBatteryUI = await SecondaryBatteryUiContainer.FromShip(ship, shipConfiguration, modifiers, appDataService, localizer),
                AntiAirUI = AntiAirDataContainer.FromShip(ship, shipConfiguration, modifiers),
                AirstrikeUI = await AirstrikeUI.FromShip(ship, modifiers, false, appDataService),
                AswAirstrikeUI = await AirstrikeUI.FromShip(ship, modifiers, true, appDataService),
                DepthChargeLauncherUI = await DepthChargesLauncherUI.FromShip(ship, shipConfiguration, modifiers, appDataService),

                // Misc
                ManeuverabilityDataContainer = ManeuverabilityDataContainer.FromShip(ship, shipConfiguration, modifiers),
                ConcealmentUiDataContainer = ConcealmentUIDataContainer.FromShip(ship, shipConfiguration, modifiers),
                SurvivabilityDataContainer = SurvivabilityDataContainer.FromShip(ship, shipConfiguration, modifiers),
                SpecialAbilityUI = SpecialAbilityDataContainer.FromShip(ship, shipConfiguration, modifiers),
            };

            shipUI.SecondColumnContent = new List<object?>
                {
                    shipUI.AntiAirUI,
                    shipUI.AirstrikeUI,
                    shipUI.AswAirstrikeUI,
                    shipUI.DepthChargeLauncherUI,
                }.Where(item => item != null)
                .Cast<object>()
                .ToList();

            if (shipUI.SecondaryBatteryUI.Secondaries != null)
            {
                shipUI.SecondColumnContent.Insert(0, shipUI.SecondaryBatteryUI);
            }

            return shipUI;
        }
    }
}
