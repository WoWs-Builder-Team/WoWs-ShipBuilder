using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWsShipBuilder.Core.DataUI
{
    public record AirstrikeUI
    {
        public string? AirstrikeName { get; set; }

        public string? AirstrikeFlights { get; set; }

        public decimal? AirstrikeReload { get; set; }

        public decimal? AirstrikeAircraftHP { get; set; }

        public decimal? AirstrikePayload { get; set; }

        public decimal? AirstrikeBombType { get; set; }

        public decimal? AirstrikeTorpedoRange { get; set; }

        public decimal? AirstrikeTorpedoArmDistance { get; set; }

        public decimal? AirstrikeTorpedoFloodingChance { get; set; }

        public decimal? AirstrikeDamage { get; set; }

        public decimal? AirstrikePenetration { get; set; }

        public decimal? AirstrikeFireChance { get; set; }

        public decimal? AirstrikeRange { get; set; }

        public decimal? AirstrikeDelay { get; set; }

        public decimal? AirstrikeSpeed { get; set; }

        public decimal? AirstrikeDepthExplosion { get; set; }
    }
}
