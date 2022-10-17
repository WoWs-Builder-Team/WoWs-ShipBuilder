// ReSharper disable InconsistentNaming

using System;
using System.Collections.Generic;
using System.Linq;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilder.DataElements.DataElementAttributes;
using WoWsShipBuilder.DataElements.DataElements;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Ship;

namespace WoWsShipBuilder.Core.DataContainers
{
    public partial record SurvivabilityDataContainer : DataContainerBase
    {
        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "HP", NameLocalizationKey = "ShipHp")]
        public int HitPoints { get; set; }

        [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "Citadel", UnitKey = "HP", NameLocalizationKey = "HitPoints")]
        public int CitadelHp { get; set; }

        [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "Citadel", UnitKey = "PerCent", NameLocalizationKey = "RegenRatio")]
        public decimal CitadelRegenRatio { get; set; }

        [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "Casemate", UnitKey = "HP", NameLocalizationKey = "HitPoints")]
        public int CasemateHp { get; set; }

        [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "Casemate", UnitKey = "PerCent", NameLocalizationKey = "RegenRatio")]
        public decimal CasemateRegenRatio { get; set; }

        [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "Bow", UnitKey = "HP", NameLocalizationKey = "HitPoints")]
        public int BowHp { get; set; }

        [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "Bow", UnitKey = "PerCent", NameLocalizationKey = "RegenRatio")]
        public decimal BowRegenRatio { get; set; }

        [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "Stern", UnitKey = "HP", NameLocalizationKey = "HitPoints")]
        public int SternHp { get; set; }

        [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "Stern", UnitKey = "PerCent", NameLocalizationKey = "RegenRatio")]
        public decimal SternRegenRatio { get; set; }

        [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "Superstructure", UnitKey = "HP", NameLocalizationKey = "HitPoints")]
        public int SuperstructureHp { get; set; }

        [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "Superstructure", UnitKey = "PerCent", NameLocalizationKey = "RegenRatio")]
        public decimal SuperstructureRegenRatio { get; set; }

        [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "AuxiliaryRooms", UnitKey = "HP", NameLocalizationKey = "HitPoints")]
        public int AuxiliaryRoomsHp { get; set; }

        [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "AuxiliaryRooms", UnitKey = "PerCent", NameLocalizationKey = "RegenRatio")]
        public decimal AuxiliaryRoomsRegenRatio { get; set; }

        [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "Hull", UnitKey = "HP", NameLocalizationKey = "HitPoints")]
        public int HullHp { get; set; }

        [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "Hull", UnitKey = "PerCent", NameLocalizationKey = "RegenRatio")]
        public decimal HullRegenRatio { get; set; }

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

            foreach (var location in shipHull.HitLocations)
            {
                switch (location.Name)
                {
                    case ShipHitLocation.Citadel:
                        survivability.CitadelHp = (int)Math.Ceiling(location.Hp * (survivability.HitPoints / shipHull.Health) / 50) * 50;
                        survivability.CitadelRegenRatio = Math.Round((decimal)location.RepairableDamage * 100);
                        break;
                    case ShipHitLocation.Bow:
                        survivability.BowHp = (int)Math.Ceiling(location.Hp * (survivability.HitPoints / shipHull.Health) / 50) * 50;
                        survivability.BowRegenRatio = Math.Round((decimal)location.RepairableDamage * 100);
                        break;
                    case ShipHitLocation.Stern:
                        survivability.SternHp = (int)Math.Ceiling(location.Hp * (survivability.HitPoints / shipHull.Health) / 50) * 50;
                        survivability.SternRegenRatio = Math.Round((decimal)location.RepairableDamage * 100);
                        break;
                    case ShipHitLocation.Superstructure:
                        survivability.SuperstructureHp = (int)Math.Ceiling(location.Hp * (survivability.HitPoints / shipHull.Health) / 50) * 50;
                        survivability.SuperstructureRegenRatio = Math.Round((decimal)location.RepairableDamage * 100);
                        break;
                    case ShipHitLocation.AuxiliaryRooms:
                        survivability.AuxiliaryRoomsHp = (int)Math.Ceiling(location.Hp * (survivability.HitPoints / shipHull.Health) / 50) * 50;
                        survivability.AuxiliaryRoomsRegenRatio = Math.Round((decimal)location.RepairableDamage * 100);
                        break;
                    case ShipHitLocation.Casemate:
                        survivability.CasemateHp = (int)Math.Ceiling(location.Hp * (survivability.HitPoints / shipHull.Health) / 50) * 50;
                        survivability.CasemateRegenRatio = Math.Round((decimal)location.RepairableDamage * 100);
                        break;
                    default:
                        survivability.HullHp = (int)Math.Ceiling(location.Hp * (survivability.HitPoints / shipHull.Health) / 50) * 50;
                        survivability.HullRegenRatio = Math.Round((decimal)location.RepairableDamage * 100);
                        break;
                }
            }

            survivability.UpdateDataElements();

            return survivability;
        }
    }
}
