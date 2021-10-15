// ReSharper disable InconsistentNaming

using System.Collections.Generic;
using System.Linq;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.Core.DataUI
{
    public record SurvivabilityUI
    {
        public decimal HitPoints { get; set; }

        public decimal FireDuration { get; set; }

        public decimal FireAmount { get; set; }

        public decimal FireReduction { get; set; }

        public decimal FireDPS { get; set; }

        public decimal FireTotalDamage { get; set; }

        public decimal FloodDuration { get; set; }

        public decimal FloodAmount { get; set; }

        public decimal FloodProbability { get; set; }

        public decimal FloodTorpedoProtection { get; set; }

        public decimal FloodDPS { get; set; }

        public decimal FloodTotalDamage { get; set; }

        public static SurvivabilityUI FromShip(Ship ship, List<ShipUpgrade> shipConfiguration, List<(string, float)> modifiers)
        {
            var shipHull = ship.Hulls[shipConfiguration.First(upgrade => upgrade.UcType == ComponentType.Hull).Components[ComponentType.Hull].First()];
            return new SurvivabilityUI
            {
                HitPoints = shipHull.Health,
                FireDuration = 0, // TODO
                FireAmount = 0, // TODO
                FireReduction = 0, // TODO
                FireDPS = 0, // TODO
                FireTotalDamage = 0, // TODO
                FloodDuration = 0, // TODO
                FloodAmount = 0, // TODO
                FloodProbability = 0, // TODO
                FloodTorpedoProtection = 0, // TODO
                FloodDPS = 0, // TODO
                FloodTotalDamage = 0, // TODO
            };
        }
    }
}
