// ReSharper disable InconsistentNaming

using System;
using System.Collections.Generic;
using System.Linq;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.Core.DataUI
{
    public record SurvivabilityUI : IDataUi
    {
        public decimal HitPoints { get; set; }

        public decimal FireDuration { get; set; }

        public decimal FireAmount { get; set; }

        public decimal FireReduction { get; set; }

        public decimal FireDPS { get; set; }

        public decimal FireTotalDamage { get; set; }

        public decimal FloodDuration { get; set; }

        public decimal FloodAmount { get; set; }

        public decimal FloodTorpedoProtection { get; set; }

        public decimal FloodDPS { get; set; }

        public decimal FloodTotalDamage { get; set; }

        public static SurvivabilityUI FromShip(Ship ship, List<ShipUpgrade> shipConfiguration, List<(string Key, float Value)> modifiers)
        {
            Hull shipHull = ship.Hulls[shipConfiguration.First(upgrade => upgrade.UcType == ComponentType.Hull).Components[ComponentType.Hull].First()];

            // Survivability expert
            decimal hitPoints = shipHull.Health;
            int survivabilityExpertIndex = modifiers.FindModifierIndex("healthPerLevel");
            if (survivabilityExpertIndex > -1)
            {
                hitPoints += (decimal)modifiers[survivabilityExpertIndex].Value * ship.Tier;
            }

            int fireSpots = shipHull.FireSpots;
            if (modifiers.FindModifierIndex("fireResistanceEnabled") > -1)
            {
                fireSpots--;
            }

            decimal fireDuration = shipHull.FireDuration;
            decimal floodDuration = shipHull.FloodingDuration;
            foreach (float modifier in modifiers.FindModifiers("hlCritTimeCoeff"))
            {
                fireDuration *= (decimal)modifier;
                floodDuration *= (decimal)modifier;
            }

            fireDuration = modifiers.FindModifiers("burnTime").Aggregate(fireDuration, (current, modifier) => current * (decimal)modifier);

            decimal fireResistance = Math.Round(modifiers.FindModifiers("burnProb").Aggregate(shipHull.FireResistance, (current, modifier) => current * (decimal)modifier), 4);

            decimal fireDps = Math.Round(hitPoints * shipHull.FireTickDamage);
            decimal fireTotalDamage = Math.Round(fireDuration * fireDps);

            decimal floodDps = Math.Round(hitPoints * shipHull.FloodingTickDamage);
            decimal floodTotalDamage = Math.Round(floodDuration * floodDps);

            return new SurvivabilityUI
            {
                HitPoints = hitPoints,
                FireDuration = fireDuration,
                FireAmount = fireSpots,
                FireReduction = fireResistance,
                FireDPS = fireDps,
                FireTotalDamage = fireTotalDamage,
                FloodDuration = floodDuration,
                FloodAmount = shipHull.FloodingSpots,
                FloodTorpedoProtection = shipHull.FloodingResistance,
                FloodDPS = floodDps,
                FloodTotalDamage = floodTotalDamage,
            };
        }
    }
}
