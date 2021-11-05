using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.Core.DataUI
{
    public record AntiAirUI : IDataUi
    {
        // These two are for the conversion in damage per second
        private const decimal ConstantDamageMultiplier = 1 / AntiAirAura.DamageInterval;

        private const decimal FlakDamageMultiplier = 1 / (AntiAirAura.DamageInterval / 2);

        public AuraDataUI? LongRangeAura { get; set; } = default!;

        public AuraDataUI? MediumRangeAura { get; set; } = default!;

        public AuraDataUI? ShortRangeAura { get; set; } = default!;

        public static AntiAirUI? FromShip(Ship ship, List<ShipUpgrade> shipConfiguration, List<(string Name, float Value)> modifiers)
        {
            if (ship.ShipClass.Equals(ShipClass.Submarine))
            {
                return null;
            }

            var hull = ship.Hulls[shipConfiguration.First(upgrade => upgrade.UcType == ComponentType.Hull).Components[ComponentType.Hull].First()];
            TurretModule? guns = null!;
            if (ship.MainBatteryModuleList != null && ship.MainBatteryModuleList.Count > 0)
            {
                guns = ship.MainBatteryModuleList[shipConfiguration.First(upgrade => upgrade.UcType == ComponentType.Artillery).Components[ComponentType.Artillery].First()];
            }

            var aaList = new List<AntiAirUI>();

            // var flakBonus = 0;
            decimal flakDamageBonus = 1;
            decimal constantDamageBonus = 1;

            int flakDamageBonusIndex = modifiers.FindModifierIndex("AABubbleDamage");
            if (flakDamageBonusIndex > -1)
            {
                var modifiersValues = modifiers.FindModifiers("AABubbleDamage").ToList();
                foreach (decimal value in modifiersValues)
                {
                    flakDamageBonus *= value;
                }
            }

            int constantDamageBonusIndex = modifiers.FindModifierIndex("AAAuraDamage");
            if (constantDamageBonusIndex > -1)
            {
                var modifiersValues = modifiers.FindModifiers("AAAuraDamage").ToList();
                foreach (decimal value in modifiersValues)
                {
                    constantDamageBonus *= value;
                }
            }

            var aaUI = new AntiAirUI();

            // Long Range Aura
            AntiAirAura? longRange = null;
            if (hull.AntiAir.LongRangeAura != null)
            {
                longRange = hull.AntiAir.LongRangeAura;
            }

            if (guns != null && guns.AntiAir != null)
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

            int flakAmount = 0;
            if (longRange != null)
            {
                var extraFlak = modifiers.FindModifiers("AAExtraBubbles");
                flakAmount = extraFlak.Aggregate(longRange.FlakCloudsNumber, (current, modifier) => current + (int)modifier);
            }

            aaUI.LongRangeAura = FromAura(longRange, flakDamageBonus, constantDamageBonus, flakAmount);
            aaUI.MediumRangeAura = FromAura(hull.AntiAir.MediumRangeAura, flakDamageBonus, constantDamageBonus, 0);
            aaUI.ShortRangeAura = FromAura(hull.AntiAir.ShortRangeAura, flakDamageBonus, constantDamageBonus, 0);

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
                string flakNumber = "";
                if (flakAmount > 0)
                {
                    var flakAverage = (int)(flakAmount * antiAirAura.HitChance);
                    var flakDelta = flakAmount - flakAverage;
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

    public record AuraDataUI : IDataUi
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
