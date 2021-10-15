using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWsShipBuilder.Core.DataUI
{
    public record TorpedoUI
    {
        public string? TorpedoName { get; set; }

        public decimal? TorpedoRange { get; set; }

        public decimal? TorpedoDamage { get; set; }

        public decimal? TorpedoReload { get; set; }

        public decimal? TorpedoSpeed { get; set; }

        public decimal? TorpedoDetectability { get; set; }

        public decimal? TorpedoReactionTime { get; set; }

        public decimal? TorpedoFloodingChance { get; set; }

        public decimal? TorpedoTurnTime { get; set; }

        public decimal? TorpedoTraverseSpeed { get; set; }
    }
}
