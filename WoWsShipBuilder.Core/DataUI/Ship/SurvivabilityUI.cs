// ReSharper disable InconsistentNaming

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.Core.DataUI
{
    public record SurvivabilityUI : IDataUi
    {
        [DataUiUnit("HP")]
        public int HitPoints { get; set; }

        public decimal FireAmount { get; set; }

        [DataUiUnit("PerCent")]
        public int FireReduction { get; set; }

        [DataUiUnit("S")]
        public decimal FireDuration { get; set; }

        [DataUiUnit("DPS")]
        public decimal FireDPS { get; set; }

        [DataUiUnit("HP")]
        public decimal FireTotalDamage { get; set; }

        public decimal FloodAmount { get; set; }

        [DataUiUnit("PerCent")]
        public int FloodTorpedoProtection { get; set; }

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

            decimal fireResistance = Math.Round(modifiers.FindModifiers("burnProb").Aggregate(shipHull.FireResistance, (current, modifier) => current * (decimal)modifier), 4);

            decimal fireDps = Math.Round(hitPoints * shipHull.FireTickDamage / 100);
            decimal fireTotalDamage = Math.Round(fireDuration * fireDps);

            decimal floodDps = Math.Round(hitPoints * shipHull.FloodingTickDamage / 100);
            decimal floodTotalDamage = Math.Round(floodDuration * floodDps);

            var survivability = new SurvivabilityUI
            {
                HitPoints = (int)hitPoints,
                FireDuration = fireDuration,
                FireAmount = fireSpots,
                FireReduction = (int)Math.Round(fireResistance * 100, 0),
                FireDPS = fireDps,
                FireTotalDamage = fireTotalDamage,
                FloodDuration = floodDuration,
                FloodAmount = shipHull.FloodingSpots,
                FloodTorpedoProtection = (int)Math.Round(shipHull.FloodingResistance * 100, 0),
                FloodDPS = floodDps,
                FloodTotalDamage = floodTotalDamage,
            };

            survivability.SurvivabilityData = survivability.ToPropertyMapping();
            return survivability;
        }
    }
}
