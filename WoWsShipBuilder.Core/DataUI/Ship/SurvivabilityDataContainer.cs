// ReSharper disable InconsistentNaming

using System;
using System.Collections.Generic;
using System.Linq;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilder.DataElements.DataElementAttributes;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Core.DataUI
{
    public partial record SurvivabilityDataContainer : DataContainerBase
    {
        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "HP")]
        public int HitPoints { get; set; }

        [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValue, GroupKey = "Fire")]
        public decimal FireAmount { get; set; }

        [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "Fire", UnitKey = "S")]
        public decimal FireDuration { get; set; }

        [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "Fire", UnitKey = "DPS")]
        public decimal FireDPS { get; set; }

        [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "Fire", UnitKey = "HP")]
        public decimal FireTotalDamage { get; set; }

        [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "Fire", UnitKey = "PerCent")]
        public decimal FireReduction { get; set; }

        [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValue, GroupKey = "Flooding")]
        public decimal FloodAmount { get; set; }

        [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "Flooding", UnitKey = "S")]
        public decimal FloodDuration { get; set; }

        [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "Flooding", UnitKey = "DPS")]
        public decimal FloodDPS { get; set; }

        [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "Flooding", UnitKey = "HP")]
        public decimal FloodTotalDamage { get; set; }

        [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "Flooding", UnitKey = "PerCent")]
        public decimal FloodTorpedoProtection { get; set; }

        public static SurvivabilityDataContainer FromShip(Ship ship, List<ShipUpgrade> shipConfiguration, List<(string Key, float Value)> modifiers)
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
            decimal fireDps = hitPoints * shipHull.FireTickDamage / 100;
            decimal fireTotalDamage = fireDuration * fireDps;

            decimal floodDps = hitPoints * shipHull.FloodingTickDamage / 100;
            decimal floodTotalDamage = floodDuration * floodDps;

            var survivability = new SurvivabilityDataContainer
            {
                HitPoints = (int)hitPoints,
                FireDuration = Math.Round(fireDuration, 1),
                FireAmount = fireSpots,
                FireReduction = Math.Round(fireResistance * 100, 1),
                FireDPS = Math.Round(fireDps),
                FireTotalDamage = Math.Round(fireTotalDamage),
                FloodDuration = Math.Round(floodDuration, 1),
                FloodAmount = shipHull.FloodingSpots,
                FloodTorpedoProtection = Math.Round(100 - modifiedFloodingCoeff, 1),
                FloodDPS = Math.Round(floodDps),
                FloodTotalDamage = Math.Round(floodTotalDamage),
            };

            survivability.UpdateDataElements();

            return survivability;
        }
    }
}
