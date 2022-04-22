using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilder.DataStructures;

// ReSharper disable InconsistentNaming
namespace WoWsShipBuilder.Core.DataUI
{
    public record AntiAirUI : DataContainerBase
    {
        // These two are for the conversion in damage per second
        private const decimal ConstantDamageMultiplier = 1 / AntiAirAura.DamageInterval;

        private const decimal FlakDamageMultiplier = 1 / (AntiAirAura.DamageInterval / 2);

        private string expanderKey = default!;

        [JsonIgnore]
        public bool IsExpanderOpen
        {
            get => ShipUI.ExpanderStateMapper[expanderKey];
            set => ShipUI.ExpanderStateMapper[expanderKey] = value;
        }

        public AuraDataUI? LongRangeAura { get; set; }

        public AuraDataUI? MediumRangeAura { get; set; }

        public AuraDataUI? ShortRangeAura { get; set; }

        public static AntiAirUI? FromShip(Ship ship, List<ShipUpgrade> shipConfiguration, List<(string Name, float Value)> modifiers)
        {
            if (ship.ShipClass.Equals(ShipClass.Submarine))
            {
                return null;
            }

            var hull = ship.Hulls[shipConfiguration.First(upgrade => upgrade.UcType == ComponentType.Hull).Components[ComponentType.Hull].First()];
            TurretModule? guns = null!;
            if (ship.MainBatteryModuleList is { Count: > 0 })
            {
                guns = ship.MainBatteryModuleList[shipConfiguration.First(upgrade => upgrade.UcType == ComponentType.Artillery).Components[ComponentType.Artillery].First()];
            }

            // var flakBonus = 0;
            decimal flakDamageBonus = 1;
            decimal constantDamageBonus = 1;

            int flakDamageBonusIndex = modifiers.FindModifierIndex("AABubbleDamage");
            if (flakDamageBonusIndex > -1)
            {
                List<float> modifiersValues = modifiers.FindModifiers("AABubbleDamage").ToList();
                flakDamageBonus = modifiersValues.Aggregate(flakDamageBonus, (current, value) => current * (decimal)value);
            }

            int constantDamageBonusIndex = modifiers.FindModifierIndex("AAAuraDamage");
            if (constantDamageBonusIndex > -1)
            {
                List<float> modifiersValues = modifiers.FindModifiers("AAAuraDamage").ToList();
                constantDamageBonus = modifiersValues.Aggregate(constantDamageBonus, (current, value) => current * (decimal)value);
            }

            IEnumerable<float> constantDamageBonusModifiers = modifiers.FindModifiers("lastChanceReloadCoefficient");
            constantDamageBonus = Math.Round(constantDamageBonusModifiers.Aggregate(constantDamageBonus, (current, arModifier) => current * (1 + ((decimal)arModifier / 100))), 2);

            var aaUI = new AntiAirUI
            {
                expanderKey = $"{ship.Index}_AA",
            };
            if (!ShipUI.ExpanderStateMapper.ContainsKey(aaUI.expanderKey))
            {
                ShipUI.ExpanderStateMapper[aaUI.expanderKey] = true;
            }

            // Long Range Aura
            AntiAirAura? longRange = null;
            if (hull.AntiAir.LongRangeAura != null)
            {
                longRange = hull.AntiAir.LongRangeAura;
            }

            if (guns is not null && guns.AntiAir is not null)
            {
                if (longRange is null)
                {
                    longRange = guns.AntiAir;
                }
                else
                {
                    longRange += guns.AntiAir;
                }
            }

            var flakAmount = 0;
            if (longRange != null)
            {
                IEnumerable<float> extraFlak = modifiers.FindModifiers("AAExtraBubbles");
                flakAmount = extraFlak.Aggregate(longRange.FlakCloudsNumber, (current, modifier) => current + (int)modifier);
            }

            aaUI.LongRangeAura = FromAura(longRange, flakDamageBonus, constantDamageBonus, flakAmount);
            aaUI.MediumRangeAura = FromAura(hull.AntiAir.MediumRangeAura, flakDamageBonus, constantDamageBonus, 0);
            aaUI.ShortRangeAura = FromAura(hull.AntiAir.ShortRangeAura, flakDamageBonus, constantDamageBonus, 0);

            if (aaUI.ShortRangeAura == null && aaUI.MediumRangeAura == null && aaUI.LongRangeAura == null)
            {
                return null;
            }

            return aaUI;
        }

        private static AuraDataUI? FromAura(AntiAirAura? antiAirAura, decimal flakDamageBonus, decimal constantDamageBonus, int flakAmount)
        {
            if (antiAirAura == null)
            {
                return null;
            }
            else
            {
                var flakNumber = "";
                if (flakAmount > 0)
                {
                    var flakAverage = (int)(flakAmount * antiAirAura.HitChance);
                    int flakDelta = flakAmount - flakAverage;
                    flakNumber = $"{flakAverage} ± {flakDelta}";
                }

                var auraData = new AuraDataUI
                {
                    Range = Math.Round(antiAirAura.MaxRange / 1000, 2),
                    ConstantDamage = Math.Round(antiAirAura.ConstantDps * ConstantDamageMultiplier * constantDamageBonus, 2),
                    Flak = flakNumber,
                    FlakDamage = Math.Round(antiAirAura.FlakDamage * FlakDamageMultiplier * flakDamageBonus, 2),
                    HitChance = (int)Math.Round(antiAirAura.HitChance * 100, 2),
                };

                auraData.AntiAirData = auraData.ToPropertyMapping();
                return auraData;
            }
        }
    }

    public record AuraDataUI : DataContainerBase
    {
        [DataUiUnit("KM")]
        public decimal Range { get; set; }

        [DataUiUnit("DPS")]
        public decimal ConstantDamage { get; set; }

        public string Flak { get; set; } = default!;

        public decimal FlakDamage { get; set; }

        [DataUiUnit("PerCent")]
        public int HitChance { get; set; }

        [JsonIgnore]
        public List<KeyValuePair<string, string>>? AntiAirData { get; set; }
    }
}
