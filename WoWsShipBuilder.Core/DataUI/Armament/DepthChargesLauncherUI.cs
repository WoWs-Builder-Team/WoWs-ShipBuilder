using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using WoWsShipBuilder.Core.DataUI.Projectiles;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Core.DataUI
{
    public record DepthChargesLauncherUI : IDataUi
    {
        private string expanderKey = default!;

        [JsonIgnore]
        public bool IsExpanderOpen
        {
            get => ShipUI.ExpanderStateMapper[expanderKey];
            set => ShipUI.ExpanderStateMapper[expanderKey] = value;
        }

        [DataUiUnit("S")]
        public decimal Reload { get; set; }

        public int NumberOfUses { get; set; }

        public int AmmoPerAttack { get; set; }

        [JsonIgnore]
        public ProjectileUI? DepthCharge { get; set; }

        [JsonIgnore]
        public List<KeyValuePair<string, string>>? DepthChargesLauncherData { get; set; }

        public static DepthChargesLauncherUI? FromShip(Ship ship, List<ShipUpgrade> shipConfiguration, List<(string Key, float Value)> modifiers)
        {
            Hull shipHull = ship.Hulls[shipConfiguration.First(upgrade => upgrade.UcType == ComponentType.Hull).Components[ComponentType.Hull].First()];

            var depthChargesArray = shipHull.DepthChargeArray;

            if (depthChargesArray is null)
            {
                return null;
            }

            var ammoPerAttack = depthChargesArray.DepthCharges.Sum(charge => charge.DepthChargesNumber);
            var ammoName = depthChargesArray.DepthCharges.First(charge => charge.DepthChargesNumber > 0).AmmoList.First();

            var numberOfUses = modifiers.FindModifiers("dcNumPacksBonus").Aggregate(depthChargesArray.MaxPacks, (current, modifier) => current + (int)modifier);

            var ammo = DepthChargeUI.FromChargesName(ammoName, modifiers);

            var depthChargesLauncherUI = new DepthChargesLauncherUI
            {
                Reload = depthChargesArray.Reload,
                NumberOfUses = numberOfUses,
                AmmoPerAttack = ammoPerAttack,
                DepthCharge = ammo,
            };

            depthChargesLauncherUI.DepthChargesLauncherData = depthChargesLauncherUI.ToPropertyMapping();
            depthChargesLauncherUI.expanderKey = $"{ship.Index}_DC";
            if (!ShipUI.ExpanderStateMapper.ContainsKey(depthChargesLauncherUI.expanderKey))
            {
                ShipUI.ExpanderStateMapper[depthChargesLauncherUI.expanderKey] = true;
            }

            return depthChargesLauncherUI;
        }
    }
}
