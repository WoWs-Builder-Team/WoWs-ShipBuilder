using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWsShipBuilder.Core.DataUI
{
    public record ManeuvrabilityUI
    {
        public decimal ManeuvrabilityMaxSpeed { get; set; }

        public decimal ManeuvrabilityFullPowerForward { get; set; }

        public decimal ManeuvrabilityFullPowerBackward { get; set; }

        public decimal ManeuvrabilityPowerToWeight { get; set; }

        public decimal ManeuvrabilityTurningCircle { get; set; }

        public decimal ManeuvrabilityRudderShiftTime { get; set; }
    }
}
