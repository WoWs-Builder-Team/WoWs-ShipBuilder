using System.Collections.Generic;
using WoWsShipBuilder.Core.DataUI;

namespace WoWsShipBuilder.Core.DataProvider
{
    public record ShipUI
    {
        public List<AirstrikeUI> AirstrikeUI;
        public List<CarrierPlaneUI> CarrierPlaneUI;
        public List<SecondaryBatteryUI> SecondaryBatteryUI;
        public List<ShellUI> ShellUI;

        public ConcealmentUI ConcealmentUI;
        public MainBatteryUI MainBatteryUI;
        public ManeuvrabilityUI ManeuvrabilityUI;
        public SurvivabilityUI SurvivabilityUI;
        public TorpedoUI TorpedoUI;
    }
}
