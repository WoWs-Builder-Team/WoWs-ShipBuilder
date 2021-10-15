using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWsShipBuilder.Core.DataUI
{
    public record ShellUI
    {
        public string? ShellName { get; set; }

        public string? ShellType { get; set; }

        public decimal? ShellDamage { get; set; }

        public decimal? ShellTheoreticalDPM { get; set; }

        public decimal? ShellVelocity { get; set; }

        public decimal? ShellWeight { get; set; }

        public decimal? ShellPenetration { get; set; }

        public decimal? ShellFireChance { get; set; }

        public decimal? ShellRicochet { get; set; }

        public decimal? ShellOvermatch { get; set; }

        public decimal? ShellArmingThreshold { get; set; }

        public decimal? ShellFuseTimer { get; set; }

        public decimal? ShellDepthExplosion { get; set; }
    }
}
