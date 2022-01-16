using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Core.DataUI.Projectiles
{
    public record DepthChargeUI : ProjectileUI, IDataUi
    {
        [JsonIgnore]
        public string Name { get; set; } = null!;

        public int Damage { get; set; }

        [DataUiUnit("M")]
        public decimal ExplosionRadius { get; set; }

        [DataUiUnit("PerCent")]
        public decimal FireChance { get; set; }

        [DataUiUnit("PerCent")]
        public decimal FloodingChance { get; set; }

        public static DepthChargeUI FromChargesName(string name, List<(string name, float value)> modifiers)
        {
            var depthCharge = AppDataHelper.Instance.GetProjectile<DepthCharge>(name);
            float damage = modifiers.FindModifiers("dcAlphaDamageMultiplier").Aggregate(depthCharge.Damage, (current, modifier) => current *= modifier);

            var depthChargeUI = new DepthChargeUI
            {
                Damage = (int)Math.Round(damage, 0),
                FireChance = Math.Round((decimal)depthCharge.FireChance * 100, 2),
                FloodingChance = Math.Round((decimal)depthCharge.FloodChance * 100, 2),
                ExplosionRadius = Math.Round((decimal)depthCharge.ExplosionRadius, 2),
            };

            depthChargeUI.ProjectileData = depthChargeUI.ToPropertyMapping();

            return depthChargeUI;
        }
    }
}
