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

        [DataUiUnit("MM")]
        public int Penetration { get; set; }

        [DataUiUnit("S")]
        public decimal FuseTimer { get; set; }

        [DataUiUnit("MM")]
        public int ArmingTreshold { get; set; }

        [DataUiUnit("Degree")]
        public string RicochetAngles { get; set; } = default!;

        [DataUiUnit("PerCent")]
        public decimal FireChance { get; set; }

        public static RocketUI FromRocketName(string name, List<(string name, float value)> modifiers)
        {
            var rocket = (Rocket)AppData.ProjectileList![name];

            decimal rocketDamage = (decimal)rocket.Damage;
            var fireChanceModifiers = modifiers.FindModifiers("rocketBurnChanceBonus");
            decimal fireChance = Math.Round((decimal)fireChanceModifiers.Aggregate(rocket.FireChance, (current, modifier) => current + modifier), 3);
            var fireChanceModifiersRockets = modifiers.FindModifiers("burnChanceFactorSmall");
            fireChance = fireChanceModifiersRockets.Aggregate(fireChance, (current, modifier) => current + (decimal)modifier);

            string ricochetAngle = "";
            decimal fuseTimer = 0;
            int armingTreshold = 0;
            if (rocket.RocketType.Equals(RocketType.AP))
            {
                var rocketDamageModifiers = modifiers.FindModifiers("rocketApAlphaDamageMultiplier").ToList();
                rocketDamage = Math.Round(rocketDamageModifiers.Aggregate(rocketDamage, (current, modifier) => current * (decimal)modifier), 2);
                ricochetAngle = $"{rocket.RicochetAngle}-{rocket.AlwaysRicochetAngle}";
                fuseTimer = (decimal)rocket.FuseTimer;
                armingTreshold = (int)rocket.ArmingThreshold;
                fireChance = 0;
            }

            var rocketUI = new RocketUI
            {
                Name = rocket.Name,
                Damage = rocketDamage,
                Penetration = (int)Math.Round(rocket.Penetration, 0),
                FuseTimer = fuseTimer,
                ArmingTreshold = armingTreshold,
                RicochetAngles = ricochetAngle,
                FireChance = Math.Round(fireChance * 100, 1),
            };

            rocketUI.ProjectileData = rocketUI.ToPropertyMapping();

            return rocketUI;
        }
    }
}
