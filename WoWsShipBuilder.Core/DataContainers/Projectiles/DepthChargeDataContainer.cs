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

        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "M")]
        public decimal ExplosionRadius { get; set; }

        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "PerCent")]
        public decimal FireChance { get; set; }

        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "PerCent")]
        public decimal FloodingChance { get; set; }

        public static async Task<DepthChargeDataContainer> FromChargesName(string name, IEnumerable<(string name, float value)> modifiers, IAppDataService appDataService)
        {
            var depthCharge = await appDataService.GetProjectile<DepthCharge>(name);
            float damage = modifiers.FindModifiers("dcAlphaDamageMultiplier").Aggregate(depthCharge.Damage, (current, modifier) => current *= modifier);

            var depthChargeDataContainer = new DepthChargeDataContainer
            {
                // Name = depthCharge.Name,
                Damage = (int)Math.Round(damage, 0),
                FireChance = Math.Round((decimal)depthCharge.FireChance * 100, 2),
                FloodingChance = Math.Round((decimal)depthCharge.FloodChance * 100, 2),
                ExplosionRadius = Math.Round((decimal)depthCharge.ExplosionRadius, 2),
            };

            depthChargeDataContainer.UpdateDataElements();

            return depthChargeDataContainer;
        }
    }
}
