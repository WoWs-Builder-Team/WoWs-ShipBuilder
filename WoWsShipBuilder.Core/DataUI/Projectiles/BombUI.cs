using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.DataUI.Projectiles;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Core.DataUI
{
    public record BombUI : ProjectileUI, IDataUi
{
        [JsonIgnore]
        public string Name { get; set; } = default!;

        public decimal Damage { get; set; }

        [DataUiUnit("MM")]
        public int Penetration { get; set; }

        [DataUiUnit("M")]
        public decimal ExplosionRadius { get; set; }

        [JsonIgnore]
        public decimal SplashCoeff { get; set; }

        [DataUiUnit("S")]
        public decimal FuseTimer { get; set; }

        [DataUiUnit("MM")]
        public int ArmingThreshold { get; set; }

        [DataUiUnit("Degree")]
        public string RicochetAngles { get; set; } = default!;

        [DataUiUnit("PerCent")]
        public decimal FireChance { get; set; }

        public static BombUI FromBombName(string name, List<(string name, float value)> modifiers)
        {
            var bomb = AppDataHelper.Instance.GetProjectile<Bomb>(name);

            decimal bombDamage = 0;
            string ricochetAngle = "";
            int armingTreshold = 0;
            decimal fuseTimer = 0;
            if (bomb.BombType.Equals(BombType.AP))
            {
                var bombDamageModifiers = modifiers.FindModifiers("bombApAlphaDamageMultiplier").ToList();
                bombDamage = Math.Round((decimal)bombDamageModifiers.Aggregate(bomb.Damage, (current, modifier) => current * modifier), 2);
                ricochetAngle = $"{bomb.RicochetAngle}-{bomb.AlwaysRicochetAngle}";
                armingTreshold = (int)bomb.ArmingThreshold;
                fuseTimer = (decimal)bomb.FuseTimer;
            }
            else
            {
                var bombDamageModifiers = modifiers.FindModifiers("bombAlphaDamageMultiplier").ToList();
                bombDamage = Math.Round((decimal)bombDamageModifiers.Aggregate(bomb.Damage, (current, modifier) => current * modifier), 2);
            }

            var fireChanceModifiers = modifiers.FindModifiers("bombBurnChanceBonus");
            decimal fireChance = Math.Round((decimal)fireChanceModifiers.Aggregate(bomb.FireChance, (current, modifier) => current + modifier), 3);
            var fireChanceModifiersBombs = modifiers.FindModifiers("burnChanceFactorBig");
            fireChance = fireChanceModifiersBombs.Aggregate(fireChance, (current, modifier) => current + (decimal)modifier);

            var bombUI = new BombUI
            {
                Name = bomb.Name,
                Damage = bombDamage,
                Penetration = (int)Math.Truncate(bomb.Penetration),
                FuseTimer = fuseTimer,
                ArmingThreshold = armingTreshold,
                RicochetAngles = ricochetAngle,
                FireChance = Math.Round(fireChance * 100, 1),
                ExplosionRadius = (decimal)bomb.ExplosionRadius,
                SplashCoeff = (decimal)bomb.SplashCoeff,
            };

            bombUI.ProjectileData = bombUI.ToPropertyMapping();

            return bombUI;
        }
    }
}
