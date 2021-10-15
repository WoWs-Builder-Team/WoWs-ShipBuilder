using System.Collections.Generic;

// ReSharper disable InconsistentNaming
namespace WoWsShipBuilder.Core.DataUI
{
    public record ShipUI
    {
        public List<AirstrikeUI>? AirstrikeUI { get; set; }

        public List<CarrierPlaneUI>? CarrierPlaneUI { get; set; }

        public List<SecondaryBatteryUI>? SecondaryBatteryUI { get; set; }

        public List<ShellUI>? ShellUI { get; set; }

        public MainBatteryUI? MainBatteryUI { get; set; }

        public TorpedoUI? TorpedoUI { get; set; }

        public ConcealmentUI ConcealmentUI { get; set; } = default!;

        public ManeuverabilityUI ManeuverabilityUI { get; set; } = default!;

        public SurvivabilityUI SurvivabilityUI { get; set; } = default!;
    }
}
