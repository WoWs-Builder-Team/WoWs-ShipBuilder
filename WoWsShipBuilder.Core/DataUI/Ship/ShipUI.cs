using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.DataStructures;

// ReSharper disable InconsistentNaming
namespace WoWsShipBuilder.Core.DataUI
{
    public record ShipUI(string Index) : IDataUi
    {
        public static readonly Dictionary<string, bool> ExpanderStateMapper = new();

        public SurvivabilityUI SurvivabilityUI { get; set; } = default!;

        public MainBatteryUI? MainBatteryUI { get; set; }

        public SecondaryBatteryUiContainer SecondaryBatteryUI { get; set; } = default!;

        public PingerGunUI? PingerGunUI { get; set; }

        public TorpedoArmamentUI? TorpedoArmamentUI { get; set; }

        public AirstrikeUI? AirstrikeUI { get; set; }

        public AirstrikeUI? AswAirstrikeUI { get; set; }

        public DepthChargesLauncherUI? DepthChargeLauncherUI { get; set; }

        public List<CVAircraftUI>? CVAircraftUI { get; set; }

        public ManeuverabilityUI ManeuverabilityUI { get; set; } = default!;

        public ConcealmentUI ConcealmentUI { get; set; } = default!;

        public AntiAirUI? AntiAirUI { get; set; }

        public List<object> SecondColumnContent { get; set; } = default!;

        public SpecialAbilityUI? SpecialAbilityUI { get; set; }

        public static async Task<ShipUI> FromShip(Ship ship, List<ShipUpgrade> shipConfiguration, List<(string, float)> modifiers, IAppDataService appDataService)
        {
            var shipUI = new ShipUI(ship.Index)
            {
                // Main weapons
                MainBatteryUI = await MainBatteryUI.FromShip(ship, shipConfiguration, modifiers, appDataService),
                TorpedoArmamentUI = await TorpedoArmamentUI.FromShip(ship, shipConfiguration, modifiers, appDataService),
                CVAircraftUI = await DataUI.CVAircraftUI.FromShip(ship, shipConfiguration, modifiers, appDataService),
                PingerGunUI = PingerGunUI.FromShip(ship, shipConfiguration, modifiers),

                // Secondary weapons
                SecondaryBatteryUI = await SecondaryBatteryUiContainer.FromShip(ship, shipConfiguration, modifiers, appDataService),
                AntiAirUI = AntiAirUI.FromShip(ship, shipConfiguration, modifiers),
                AirstrikeUI = await AirstrikeUI.FromShip(ship, modifiers, false, appDataService),
                AswAirstrikeUI = await AirstrikeUI.FromShip(ship, modifiers, true, appDataService),
                DepthChargeLauncherUI = await DepthChargesLauncherUI.FromShip(ship, shipConfiguration, modifiers, appDataService),

                // Misc
                ManeuverabilityUI = ManeuverabilityUI.FromShip(ship, shipConfiguration, modifiers),
                ConcealmentUI = ConcealmentUI.FromShip(ship, shipConfiguration, modifiers),
                SurvivabilityUI = SurvivabilityUI.FromShip(ship, shipConfiguration, modifiers),
                SpecialAbilityUI = SpecialAbilityUI.FromShip(ship, shipConfiguration, modifiers),
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
