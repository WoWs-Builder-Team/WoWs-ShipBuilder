using WoWsShipBuilder.DataElements.DataElementAttributes;
using WoWsShipBuilder.DataStructures.Projectile;
using WoWsShipBuilder.Infrastructure;
using WoWsShipBuilder.Infrastructure.Data;

namespace WoWsShipBuilder.DataContainers
{
    public partial record RocketDataContainer : ProjectileDataContainer
    {
        [DataElementType(DataElementTypes.KeyValue, IsValueLocalizationKey = true, IsValueAppLocalization = true)]
        public string RocketType { get; set; } = default!;

        [DataElementType(DataElementTypes.KeyValue, IsValueLocalizationKey = true)]
        public string Name { get; set; } = default!;

        [DataElementType(DataElementTypes.KeyValue)]
        public decimal Damage { get; set; }

        [DataElementType(DataElementTypes.Grouped | DataElementTypes.Tooltip, GroupKey = "Splash", TooltipKey = "SplashExplanation", UnitKey = "M")]
        public decimal SplashRadius { get; set; }

        [DataElementType(DataElementTypes.Grouped | DataElementTypes.Tooltip, GroupKey = "Splash", TooltipKey = "SplashExplanation")]
        public decimal SplashDmg { get; set; }

        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "MM")]
        public int Penetration { get; set; }

        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "S")]
        public decimal FuseTimer { get; set; }

        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "MM")]
        public int ArmingThreshold { get; set; }

        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "Degree")]
        public string RicochetAngles { get; set; } = default!;

        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "PerCent")]
        public decimal FireChance { get; set; }

        [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "Blast", UnitKey = "M")]
        public decimal ExplosionRadius { get; set; }

        [DataElementType(DataElementTypes.Grouped | DataElementTypes.Tooltip, GroupKey = "Blast", TooltipKey = "BlastExplanation")]
        [DataElementFiltering(true, "ShouldDisplayBlastPenetration")]
        public decimal SplashCoeff { get; set; }

        public bool ShowBlastPenetration { get; private set; }

        public static RocketDataContainer FromRocketName(string name, List<(string name, float value)> modifiers)
        {
            var rocket = AppData.FindProjectile<Rocket>(name);

            var rocketDamage = (decimal)rocket.Damage;
            var showBlastPenetration = true;
            var ricochetAngle = "";
            decimal fuseTimer = 0;
            var armingThreshold = 0;
            decimal fireChance = 0;
            if (rocket.RocketType.Equals(DataStructures.RocketType.AP))
            {
                List<float> rocketDamageModifiers = modifiers.FindModifiers("rocketApAlphaDamageMultiplier").ToList();
                rocketDamage = rocketDamageModifiers.Aggregate(rocketDamage, (current, modifier) => current * (decimal)modifier);
                ricochetAngle = $"{rocket.RicochetAngle}-{rocket.AlwaysRicochetAngle}";
                fuseTimer = (decimal)rocket.FuseTimer;
                armingThreshold = (int)rocket.ArmingThreshold;
                showBlastPenetration = false;
            }
            else
            {
                var fireChanceModifiers = modifiers.FindModifiers("rocketBurnChanceBonus");
                fireChance = (decimal)fireChanceModifiers.Aggregate(rocket.FireChance, (current, modifier) => current + modifier);
                var fireChanceModifiersRockets = modifiers.FindModifiers("burnChanceFactorSmall");
                fireChance = fireChanceModifiersRockets.Aggregate(fireChance, (current, modifier) => current + (decimal)modifier);
            }

            var rocketDataContainer = new RocketDataContainer
            {
                Name = rocket.Name,
                RocketType = $"ArmamentType_{rocket.RocketType.RocketTypeToString()}",
                Damage = Math.Round(rocketDamage, 2),
                Penetration = (int)Math.Truncate(rocket.Penetration),
                FuseTimer = fuseTimer,
                ArmingThreshold = armingThreshold,
                RicochetAngles = ricochetAngle,
                FireChance = Math.Round(fireChance * 100, 1),
                ExplosionRadius = (decimal)rocket.ExplosionRadius,
                SplashCoeff = (decimal)rocket.SplashCoeff,
                ShowBlastPenetration = showBlastPenetration,
                SplashRadius = Math.Round((decimal)rocket.DepthSplashRadius, 1),
                SplashDmg = Math.Round(rocketDamage * (decimal)rocket.SplashDamageCoefficient),
            };

            rocketDataContainer.UpdateDataElements();

            return rocketDataContainer;
        }

        private bool ShouldDisplayBlastPenetration(object obj)
        {
            return ShowBlastPenetration;
        }
    }
}
