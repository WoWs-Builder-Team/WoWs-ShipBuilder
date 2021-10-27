using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.DataUI.Projectiles;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.Core.DataUI
{
    public record RocketUI : ProjectileUI, IDataUi
    {
        [JsonIgnore]
        public string Name { get; set; } = default!;

        public decimal Damage { get; set; }

        public int Penetration { get; set; }

        public decimal FuseTimer { get; set; }

        public int ArmingTreshold { get; set; }

        public string RicochetAngles { get; set; } = default!;

        public int FireChance { get; set; }

        public static RocketUI FromRocketName(string name, List<(string name, float value)> modifiers)
        {
            var rocket = (Rocket)AppData.ProjectileList![name];

            decimal rocketDamage = (decimal)rocket.Damage;
            string ricochetAngle = "";
            if (rocket.RocketType.Equals(RocketType.AP))
            {
                var bombDamageModifiers = modifiers.FindModifiers("bombApAlphaDamageMultiplier").ToList();
                rocketDamage = Math.Round((decimal)bombDamageModifiers.Aggregate(rocketDamage, (current, modifier) => current * (decimal)modifier), 2);
                ricochetAngle = $"{rocket.RicochetAngle}-{rocket.AlwaysRicochetAngle}";
            }

            var fireChanceModifiers = modifiers.FindModifiers("rocketBurnChanceBonus");
            decimal fireChance = Math.Round((decimal)fireChanceModifiers.Aggregate(rocket.FireChance, (current, modifier) => current * modifier), 2);

            var rocketUI = new RocketUI
            {
                Name = rocket.Name,
                Damage = rocketDamage,
                Penetration = (int)Math.Round(rocket.Penetration, 0),
                FuseTimer = (decimal)rocket.FuseTimer,
                ArmingTreshold = (int)rocket.ArmingThreshold,
                RicochetAngles = ricochetAngle,
                FireChance = (int)(fireChance * 100),
            };

            rocketUI.ProjectileData = rocketUI.ToPropertyMapping();

            return rocketUI;
        }
    }
}
