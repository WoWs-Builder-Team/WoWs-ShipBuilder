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

        public AuraDataUI? LongRangeAura { get; set; }

        public AuraDataUI? MediumRangeAura { get; set; }

        public AuraDataUI? ShortRangeAura { get; set; }

        public static AntiAirUI FromShip(Ship ship, List<ShipUpgrade> shipConfiguration, List<(string Name, float Value)> modifiers)
        {
            var hull = ship.Hulls[shipConfiguration.First(upgrade => upgrade.UcType == ComponentType.Hull).Components[ComponentType.Hull].First()];
            var guns = ship.MainBatteryModuleList[shipConfiguration.First(upgrade => upgrade.UcType == ComponentType.Artillery).Components[ComponentType.Artillery].First()];

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

            if (guns.AntiAir != null)
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

            var longRangeUI = FromAura(longRange, flakDamageBonus, constantDamageBonus);
            aaUI.LongRangeAura = longRangeUI;
            aaUI.MediumRangeAura = FromAura(hull.AntiAir.MediumRangeAura, flakDamageBonus, constantDamageBonus);
            aaUI.ShortRangeAura = FromAura(hull.AntiAir.ShortRangeAura, flakDamageBonus, constantDamageBonus);
            return aaUI;
        }

        private static AuraDataUI? FromAura(AntiAirAura? antiAirAura, decimal flakDamageBonus, decimal constantDamageBonus)
        {
            if (antiAirAura == null)
            {
                return null;
            }
            else
            {
                return new AuraDataUI
                {
                    Range = antiAirAura.MaxRange,
                    ConstantDamage = antiAirAura.ConstantDps * ConstantDamageMultiplier * constantDamageBonus,
                    Flak = antiAirAura.FlakCloudsNumber,
                    FlakDamage = antiAirAura.FlakDamage * FlakDamageMultiplier * flakDamageBonus,
                    HitChance = (int)Math.Round(antiAirAura.HitChance * 100, 2),
                };
            }
        }
    }

    public record AuraDataUI
    {
        [DataUiUnit("KM")]
        public decimal Range { get; set; }

        public decimal ConstantDamage { get; set; }

        public int Flak { get; set; }

        public decimal FlakDamage { get; set; }

        [DataUiUnit("PerCent")]
        public int HitChance { get; set; }

        [JsonIgnore]
        public List<KeyValuePair<string, string>>? AntiAirData { get; set; }
    }
}
