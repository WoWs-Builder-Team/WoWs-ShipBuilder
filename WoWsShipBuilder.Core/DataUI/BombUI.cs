using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.Core.DataUI
{
public record BombUI
{
        public decimal Damage { get; set; }

        public int Penetration { get; set; }

        public decimal FuseTimer { get; set; }

        public int ArmingTreshold { get; set; }

        public string RicochetAngles { get; set; } = default!;

        public int FireChance { get; set; }

        public static BombUI FromBombName(string name, List<(string name, float value)> modifiers)
        {
            var bomb = (Bomb)AppData.ProjectileList![name];

            var bombDamageModifiers = modifiers.FindModifiers("bombApAlphaDamageMultiplier").ToList();
            bombDamageModifiers.AddRange(modifiers.FindModifiers("bombAlphaDamageMultiplier"));
            decimal bombDamage = Math.Round((decimal)bombDamageModifiers.Aggregate(bomb.Damage, (current, modifier) => current * modifier), 2);

            var fireChanceModifiers = modifiers.FindModifiers("bombBurnChanceBonus");
            decimal fireChance = Math.Round((decimal)fireChanceModifiers.Aggregate(bomb.FireChance, (current, modifier) => current * modifier), 2);

            return new BombUI
            {
                Damage = bombDamage,
                Penetration = (int)Math.Round(bomb.Penetration, 0),
                FuseTimer = (decimal)bomb.FuseTimer,
                ArmingTreshold = (int)bomb.ArmingThreshold,
                RicochetAngles = $"{bomb.RicochetAngle}-{bomb.AlwaysRicochetAngle}",
                FireChance = (int)(fireChance * 100),
            };
        }
    }
}
