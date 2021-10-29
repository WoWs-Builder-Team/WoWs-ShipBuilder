using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.Core.DataUI.Projectiles
{
    public record DepthChargeUI : ProjectileUI, IDataUi
    {
        [JsonIgnore]
        public string Name { get; set; } = null!;

        [DataUiUnit("HP")]
        public int Damage { get; set; }

        [DataUiUnit("PerCent")]
        public decimal FireChance { get; set; }

        [DataUiUnit("PerCent")]
        public decimal FloodChance { get; set; }

        public static DepthChargeUI? FromChargesName(string name, List<(string name, float value)> modifiers)
        {
            var depthCharge = (DepthCharge)AppData.ProjectileList![name];

            var depthChargeUI = new DepthChargeUI
            {
                Damage = (int)Math.Round(depthCharge.Damage, 0),
                FireChance = Math.Round((decimal)depthCharge.FireChance * 100, 2),
                FloodChance = Math.Round((decimal)depthCharge.FloodChance * 100, 2),
            };

            depthChargeUI.ProjectileData = depthChargeUI.ToPropertyMapping();

            return depthChargeUI;
        }
    }
}
