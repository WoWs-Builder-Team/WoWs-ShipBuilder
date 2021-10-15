using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWsShipBuilder.Core.DataUI
{
    public record ConcealmentUI
    {
        public decimal ConcealmentBySea { get; set; }

        public decimal ConcealmentBySeaFiring { get; set; }

        public decimal ConcealmentBySeaFiringSmoke { get; set; }

        public decimal ConcealmentBySeaFiringAA { get; set; }

        public decimal ConcealmentBySeaFire { get; set; }

        public decimal ConcealmentByAir { get; set; }

        public decimal ConcealmentByAirFiring { get; set; }

        public decimal ConcealmentByAirFiringAA { get; set; }

        public decimal ConcealmentByAirFire { get; set; }

        public decimal ConcealmentBySub { get; set; }

        public decimal ConcealmentBySubSurface { get; set; }

        public decimal ConcealmentBySubPeriscope { get; set; }

        public decimal ConcealmentBySubOperating { get; set; }

        public decimal ConcealmentBySubMaximum { get; set; }
    }
}
