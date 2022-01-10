// ReSharper disable InconsistentNaming

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Core.DataUI
{
    public record SurvivabilityUI : IDataUi
    {
        [DataUiUnit("HP")]
        public int HitPoints { get; set; }

        public decimal FireAmount { get; set; }

        [DataUiUnit("PerCent")]
        public decimal FireReduction { get; set; }

        [DataUiUnit("S")]
        public decimal FireDuration { get; set; }

        [DataUiUnit("DPS")]
        public decimal FireDPS { get; set; }

        [DataUiUnit("HP")]
        public decimal FireTotalDamage { get; set; }

        public decimal FloodAmount { get; set; }

        [DataUiUnit("PerCent")]
        public decimal FloodTorpedoProtection { get; set; }

        [DataUiUnit("S")]
        public decimal FloodDuration { get; set; }

        [DataUiUnit("DPS")]
        public decimal FloodDPS { get; set; }

        [DataUiUnit("HP")]
        public decimal FloodTotalDamage { get; set; }

        [JsonIgnore]
        public List<KeyValuePair<string, string>>? SurvivabilityData { get; set; }

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

            floodDuration = modifiers.FindModifiers("floodTime").Aggregate(floodDuration, (current, modifier) => current * (decimal)modifier);

            // fire chance reduction = base fire resistance +(100 - base fire resistance) *(1 - burnProb)
            decimal baseFireResistance = 1 - shipHull.FireResistance;
            decimal fireResistanceModifiers = modifiers.FindModifiers("burnProb").Aggregate(1M, (current, modifier) => current * (decimal)modifier);
            decimal fireResistance = baseFireResistance + ((1 - baseFireResistance) * (1 - fireResistanceModifiers));

            decimal modifiedFloodingCoeff = modifiers.FindModifiers("uwCoeffBonus").Aggregate(shipHull.FloodingResistance * 3, (current, modifier) => current - ((decimal)modifier / 100)) * 100;
            decimal torpedoProtection = Math.Round(100 - modifiedFloodingCoeff, 1);
            decimal fireDps = Math.Round(hitPoints * shipHull.FireTickDamage / 100);
            decimal fireTotalDamage = Math.Round(fireDuration * fireDps);

            decimal floodDps = Math.Round(hitPoints * shipHull.FloodingTickDamage / 100);
            decimal floodTotalDamage = Math.Round(floodDuration * floodDps);

            var survivability = new SurvivabilityUI
            {
                HitPoints = (int)hitPoints,
                FireDuration = Math.Round(fireDuration, 1),
                FireAmount = fireSpots,
                FireReduction = Math.Round(fireResistance * 100, 1),
                FireDPS = fireDps,
                FireTotalDamage = fireTotalDamage,
                FloodDuration = Math.Round(floodDuration, 1),
                FloodAmount = shipHull.FloodingSpots,
                FloodTorpedoProtection = torpedoProtection,
                FloodDPS = floodDps,
                FloodTotalDamage = floodTotalDamage,
            };

            survivability.SurvivabilityData = survivability.ToPropertyMapping();
            return survivability;
        }
    }
}
