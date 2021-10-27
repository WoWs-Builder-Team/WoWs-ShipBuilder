using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.DataUI.Projectiles;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.Core.DataUI
{
public record BombUI : ProjectileUI, IDataUi
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

            decimal bombDamage = 0;
            string ricochetAngle = "";
            if (bomb.BombType.Equals(BombType.AP))
            {
                var bombDamageModifiers = modifiers.FindModifiers("bombApAlphaDamageMultiplier").ToList();
                bombDamage = Math.Round((decimal)bombDamageModifiers.Aggregate(bomb.Damage, (current, modifier) => current * modifier), 2);
                ricochetAngle = $"{bomb.RicochetAngle}-{bomb.AlwaysRicochetAngle}";
            }
            else
            {
                var bombDamageModifiers = modifiers.FindModifiers("bombAlphaDamageMultiplier").ToList();
                bombDamage = Math.Round((decimal)bombDamageModifiers.Aggregate(bomb.Damage, (current, modifier) => current * modifier), 2);
            }

            var fireChanceModifiers = modifiers.FindModifiers("bombBurnChanceBonus");
            decimal fireChance = Math.Round((decimal)fireChanceModifiers.Aggregate(bomb.FireChance, (current, modifier) => current * modifier), 2);

            var bombUI = new BombUI
            {
                Damage = bombDamage,
                Penetration = (int)Math.Round(bomb.Penetration, 0),
                FuseTimer = (decimal)bomb.FuseTimer,
                ArmingTreshold = (int)bomb.ArmingThreshold,
                RicochetAngles = ricochetAngle,
                FireChance = (int)(fireChance * 100),
            };

            bombUI.ProjectileData = bombUI.ToPropertyMapping();

            return bombUI;
        }
    }
}
