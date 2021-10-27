using System.Collections.Generic;
using WoWsShipBuilder.Core.DataUI.Aircrafts;
using WoWsShipBuilderDataStructures;

// ReSharper disable InconsistentNaming
namespace WoWsShipBuilder.Core.DataUI
{
    public record ShipUI(string Index) : IDataUi
    {
        public SurvivabilityUI SurvivabilityUI { get; set; } = default!;

        public MainBatteryUI? MainBatteryUI { get; set; }

        public List<ShellUI>? ShellUI { get; set; }

        public List<SecondaryBatteryUI>? SecondaryBatteryUI { get; set; }

        public TorpedoArmamentUI? TorpedoArmamentUI { get; set; }

        public List<AirstrikeUI>? AirstrikeUI { get; set; }

        public List<CVAircraftUI>? CVAircraftUI { get; set; }

        public ManeuverabilityUI ManeuverabilityUI { get; set; } = default!;

        public ConcealmentUI ConcealmentUI { get; set; } = default!;

        public AntiAirUI AntiAirUI { get; set; } = default!;

        public static ShipUI FromShip(Ship ship, List<ShipUpgrade> shipConfiguration, List<(string, float)> modifiers)
        {
            var shipUI = new ShipUI(ship.Index)
            {
                // Main weapons
                MainBatteryUI = MainBatteryUI.FromShip(ship, shipConfiguration, modifiers),
                TorpedoArmamentUI = TorpedoArmamentUI.FromShip(ship, shipConfiguration, modifiers),
                CVAircraftUI = Aircrafts.CVAircraftUI.FromShip(ship, shipConfiguration, modifiers),

                // Secondary weapons
                SecondaryBatteryUI = DataUI.SecondaryBatteryUI.FromShip(ship, shipConfiguration, modifiers),
                AntiAirUI = AntiAirUI.FromShip(ship, shipConfiguration, modifiers),

                // Misc
                ManeuverabilityUI = ManeuverabilityUI.FromShip(ship, shipConfiguration, modifiers),
                ConcealmentUI = ConcealmentUI.FromShip(ship, shipConfiguration, modifiers),
                SurvivabilityUI = SurvivabilityUI.FromShip(ship, shipConfiguration, modifiers),
            };
            return shipUI;
        }
    }
}
