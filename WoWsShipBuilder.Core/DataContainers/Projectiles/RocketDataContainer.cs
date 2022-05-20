using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.DataElements.DataElementAttributes;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Core.DataContainers
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

        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "M")]
        public decimal ExplosionRadius { get; set; }

        [DataElementType(DataElementTypes.Tooltip, TooltipKey = "BlastExplanation")]
        [DataElementFiltering(true, "ShouldDisplayBlastPenetration")]
        public decimal SplashCoeff { get; set; }

        public bool ShowBlastPenetration { get; private set; }

        public static async Task<RocketDataContainer> FromRocketName(string name, List<(string name, float value)> modifiers, IAppDataService appDataService)
        {
            var rocket = await appDataService.GetProjectile<Rocket>(name);

            var rocketDamage = (decimal)rocket.Damage;
            var fireChanceModifiers = modifiers.FindModifiers("rocketBurnChanceBonus");
            var fireChance = (decimal)fireChanceModifiers.Aggregate(rocket.FireChance, (current, modifier) => current + modifier);
            var fireChanceModifiersRockets = modifiers.FindModifiers("burnChanceFactorSmall");
            fireChance = fireChanceModifiersRockets.Aggregate(fireChance, (current, modifier) => current + (decimal)modifier);

            var showBlastPenetration = true;
            var ricochetAngle = "";
            decimal fuseTimer = 0;
            var armingThreshold = 0;
            if (rocket.RocketType.Equals(DataStructures.RocketType.AP))
            {
                List<float> rocketDamageModifiers = modifiers.FindModifiers("rocketApAlphaDamageMultiplier").ToList();
                rocketDamage = rocketDamageModifiers.Aggregate(rocketDamage, (current, modifier) => current * (decimal)modifier);
                ricochetAngle = $"{rocket.RicochetAngle}-{rocket.AlwaysRicochetAngle}";
                fuseTimer = (decimal)rocket.FuseTimer;
                armingThreshold = (int)rocket.ArmingThreshold;
                fireChance = 0;
                showBlastPenetration = false;
            }

            var rocketDataContainer = new RocketDataContainer
            {
                Name = rocket.Name,
                RocketType = $"ArmamentType_{rocket.RocketType}",
                Damage = Math.Round(rocketDamage, 2),
                Penetration = (int)Math.Truncate(rocket.Penetration),
                FuseTimer = fuseTimer,
                ArmingThreshold = armingThreshold,
                RicochetAngles = ricochetAngle,
                FireChance = Math.Round(fireChance * 100, 1),
                ExplosionRadius = (decimal)rocket.ExplosionRadius,
                SplashCoeff = (decimal)rocket.SplashCoeff,
                ShowBlastPenetration = showBlastPenetration,
                SplashRadius = (decimal)rocket.DepthSplashRadius,
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
