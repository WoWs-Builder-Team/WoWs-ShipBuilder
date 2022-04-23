using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WoWsShipBuilder.Core.DataUI.Projectiles;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.DataElements.DataElementAttributes;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Core.DataUI
{
    public partial record BombDataContainer : ProjectileDataContainer
    {
        [DataElementType(DataElementTypes.KeyValue)]
        public string Name { get; set; } = default!;

        [DataElementType(DataElementTypes.KeyValue)]
        public decimal Damage { get; set; }

        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "MM")]
        public int Penetration { get; set; }

        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "M")]
        public decimal ExplosionRadius { get; set; }

        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "S")]
        public decimal FuseTimer { get; set; }

        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "MM")]
        public int ArmingThreshold { get; set; }

        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "Degree")]
        public string RicochetAngles { get; set; } = default!;

        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "PerCent")]
        public decimal FireChance { get; set; }

        [DataElementType(DataElementTypes.KeyValue)]
        [DataElementFiltering(false)]
        public decimal SplashCoeff { get; set; }

        public static async Task<BombDataContainer> FromBombName(string name, List<(string name, float value)> modifiers, IAppDataService appDataService)
        {
            var bomb = await appDataService.GetProjectile<Bomb>(name);

            decimal bombDamage = 0;
            var ricochetAngle = "";
            var armingThreshold = 0;
            decimal fuseTimer = 0;
            if (bomb.BombType.Equals(BombType.AP))
            {
                List<float> bombDamageModifiers = modifiers.FindModifiers("bombApAlphaDamageMultiplier").ToList();
                bombDamage = (decimal)bombDamageModifiers.Aggregate(bomb.Damage, (current, modifier) => current * modifier);
                ricochetAngle = $"{bomb.RicochetAngle}-{bomb.AlwaysRicochetAngle}";
                armingThreshold = (int)bomb.ArmingThreshold;
                fuseTimer = (decimal)bomb.FuseTimer;
            }
            else
            {
                List<float> bombDamageModifiers = modifiers.FindModifiers("bombAlphaDamageMultiplier").ToList();
                bombDamage = (decimal)bombDamageModifiers.Aggregate(bomb.Damage, (current, modifier) => current * modifier);
            }

            var fireChanceModifiers = modifiers.FindModifiers("bombBurnChanceBonus");
            var fireChance = (decimal)fireChanceModifiers.Aggregate(bomb.FireChance, (current, modifier) => current + modifier);
            var fireChanceModifiersBombs = modifiers.FindModifiers("burnChanceFactorBig");
            fireChance = fireChanceModifiersBombs.Aggregate(fireChance, (current, modifier) => current + (decimal)modifier);

            var bombDataContainer = new BombDataContainer
            {
                Name = bomb.Name,
                Damage = Math.Round(bombDamage, 2),
                Penetration = (int)Math.Truncate(bomb.Penetration),
                FuseTimer = fuseTimer,
                ArmingThreshold = armingThreshold,
                RicochetAngles = ricochetAngle,
                FireChance = Math.Round(fireChance * 100, 1),
                ExplosionRadius = (decimal)bomb.ExplosionRadius,
                SplashCoeff = (decimal)bomb.SplashCoeff,
            };

            bombDataContainer.UpdateDataElement();

            return bombDataContainer;
        }
    }
}
