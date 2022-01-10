using System.Collections.Generic;
using System.Linq;
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

        public static ShipUI FromShip(Ship ship, List<ShipUpgrade> shipConfiguration, List<(string, float)> modifiers)
        {
            var shipUI = new ShipUI(ship.Index)
            {
                // Main weapons
                MainBatteryUI = MainBatteryUI.FromShip(ship, shipConfiguration, modifiers),
                TorpedoArmamentUI = TorpedoArmamentUI.FromShip(ship, shipConfiguration, modifiers),
                CVAircraftUI = DataUI.CVAircraftUI.FromShip(ship, shipConfiguration, modifiers),
                PingerGunUI = PingerGunUI.FromShip(ship, shipConfiguration, modifiers),

                // Secondary weapons
                SecondaryBatteryUI = SecondaryBatteryUiContainer.FromShip(ship, shipConfiguration, modifiers),
                AntiAirUI = AntiAirUI.FromShip(ship, shipConfiguration, modifiers),
                AirstrikeUI = AirstrikeUI.FromShip(ship, modifiers, false),
                AswAirstrikeUI = AirstrikeUI.FromShip(ship, modifiers, true),
                DepthChargeLauncherUI = DepthChargesLauncherUI.FromShip(ship, shipConfiguration, modifiers),

                // Misc
                ManeuverabilityUI = ManeuverabilityUI.FromShip(ship, shipConfiguration, modifiers),
                ConcealmentUI = ConcealmentUI.FromShip(ship, shipConfiguration, modifiers),
                SurvivabilityUI = SurvivabilityUI.FromShip(ship, shipConfiguration, modifiers),
                SpecialAbilityUI = SpecialAbilityUI.FromShip(ship, modifiers),
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
