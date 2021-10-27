using System.Collections.Generic;
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

        public List<TorpedoUI>? TorpedoUI { get; set; }

        public List<AirstrikeUI>? AirstrikeUI { get; set; }

        public List<CarrierPlaneUI>? CarrierPlaneUI { get; set; }

        public ManeuverabilityUI ManeuverabilityUI { get; set; } = default!;

        public ConcealmentUI ConcealmentUI { get; set; } = default!;

        public static ShipUI FromShip(Ship ship, List<ShipUpgrade> shipConfiguration, List<(string, float)> modifiers)
        {
            var shipUI = new ShipUI(ship.Index)
            {
                SurvivabilityUI = SurvivabilityUI.FromShip(ship, shipConfiguration, modifiers),
                MainBatteryUI = MainBatteryUI.FromShip(ship, shipConfiguration, modifiers),
                ShellUI = DataUI.ShellUI.FromShip(ship, shipConfiguration, modifiers),
                SecondaryBatteryUI = DataUI.SecondaryBatteryUI.FromShip(ship, shipConfiguration, modifiers),
            };
            return shipUI;
        }
    }
}
