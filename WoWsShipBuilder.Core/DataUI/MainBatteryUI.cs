using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWsShipBuilder.Core.DataUI
{
    public record MainBatteryUI
    {
        public string? MainBatteryName { get; set; }

        public decimal? MainBatteryRange { get; set; }

        public decimal? MainBatteryReload { get; set; }

        public decimal? MainBatteryRoF { get; set; }

        public decimal? MainBatteryTurnTime { get; set; }

        public decimal? MainBatteryTraverseSpeed { get; set; }

        public decimal? MainBatterySigma { get; set; }

        public decimal? MainBatteryHorizontalDisp { get; set; }

        public decimal? MainBatteryVerticalDisp { get; set; }
    }
}
