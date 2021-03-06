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
    public partial record DepthChargeDataContainer : ProjectileDataContainer
    {
        // Some DC are missing in game name localization. Name property disabled until their addition.
        // [DataElementType(DataElementTypes.KeyValue, IsValueLocalizationKey = true)]
        // public string Name { get; set; } = null!;
        [DataElementType(DataElementTypes.KeyValue)]
        public int Damage { get; set; }

        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "Knots")]
        public string SinkSpeed { get; set; } = default!;

        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "S")]
        public string DetonationTimer { get; set; } = default!;

        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "M")]
        public string DetonationDepth { get; set; } = default!;

        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "M")]
        public decimal DcSplashRadius { get; set; }

        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "PerCent")]
        public decimal FireChance { get; set; }

        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "PerCent")]
        public decimal FloodingChance { get; set; }

        public Dictionary<float, List<float>> PointsOfDmg { get; set; } = default!;

        public static async Task<DepthChargeDataContainer> FromChargesName(string name, IEnumerable<(string name, float value)> modifiers, IAppDataService appDataService)
        {
            var depthCharge = await appDataService.GetProjectile<DepthCharge>(name);
            float damage = modifiers.FindModifiers("dcAlphaDamageMultiplier").Aggregate(depthCharge.Damage, (current, modifier) => current *= modifier);
            float minSpeed = depthCharge.SinkingSpeed * (1 - depthCharge.SinkingSpeedRng);
            float maxSpeed = depthCharge.SinkingSpeed * (1 + depthCharge.SinkingSpeedRng);
            float minTimer = depthCharge.DetonationTimer - depthCharge.DetonationTimerRng;
            float maxTimer = depthCharge.DetonationTimer + depthCharge.DetonationTimerRng;
            decimal minDetDepth = (decimal)(minSpeed * minTimer) * Constants.KnotsToMps;
            decimal maxDetDepth = (decimal)(maxSpeed * maxTimer) * Constants.KnotsToMps;

            var depthChargeDataContainer = new DepthChargeDataContainer
            {
                // Name = depthCharge.Name,
                Damage = (int)Math.Round(damage, 0),
                FireChance = Math.Round((decimal)depthCharge.FireChance * 100, 2),
                FloodingChance = Math.Round((decimal)depthCharge.FloodChance * 100, 2),
                DcSplashRadius = Math.Round((decimal)depthCharge.ExplosionRadius, 2),
                SinkSpeed = $"{Math.Round(minSpeed, 1)} ~ {Math.Round(maxSpeed, 1)}",
                DetonationTimer = $"{Math.Round(minTimer, 1)} ~ {Math.Round(maxTimer, 1)}",
                DetonationDepth = $"{Math.Round(minDetDepth, 1)} ~ {Math.Round(maxDetDepth, 1)}",
                PointsOfDmg = depthCharge.PointsOfDamage,
            };

            depthChargeDataContainer.UpdateDataElements();

            return depthChargeDataContainer;
        }
    }
}
