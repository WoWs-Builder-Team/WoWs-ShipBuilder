using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.DataElements.DataElementAttributes;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Core.DataUI
{
    public partial record DepthChargesLauncherDataContainer : DataContainerBase
    {
        private string expanderKey = default!;

        public bool IsExpanderOpen
        {
            get => ShipDataContainer.ExpanderStateMapper[expanderKey];
            set => ShipDataContainer.ExpanderStateMapper[expanderKey] = value;
        }

        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "S")]
        public decimal Reload { get; set; }

        [DataElementType(DataElementTypes.KeyValue)]
        public int NumberOfUses { get; set; }

        [DataElementType(DataElementTypes.KeyValue)]
        public int BombsPerCharge { get; set; }

        public ProjectileDataContainer? DepthCharge { get; set; }

        public static async Task<DepthChargesLauncherDataContainer?> FromShip(Ship ship, List<ShipUpgrade> shipConfiguration, List<(string Key, float Value)> modifiers, IAppDataService appDataService)
        {
            var shipHull = ship.Hulls[shipConfiguration.First(upgrade => upgrade.UcType == ComponentType.Hull).Components[ComponentType.Hull].First()];

            var depthChargesArray = shipHull.DepthChargeArray;

            if (depthChargesArray is null)
            {
                return null;
            }

            int ammoPerAttack = depthChargesArray.DepthCharges.Sum(charge => charge.DepthChargesNumber) * depthChargesArray.NumShots;
            string? ammoName = depthChargesArray.DepthCharges.First(charge => charge.DepthChargesNumber > 0).AmmoList.First();

            int numberOfUses = modifiers.FindModifiers("dcNumPacksBonus").Aggregate(depthChargesArray.MaxPacks, (current, modifier) => current + (int)modifier);

            var ammo = await DepthChargeDataContainer.FromChargesName(ammoName, modifiers, appDataService);

            var depthChargesLauncherDataContainer = new DepthChargesLauncherDataContainer
            {
                Reload = depthChargesArray.Reload,
                NumberOfUses = numberOfUses,
                BombsPerCharge = ammoPerAttack,
                DepthCharge = ammo,
            };

            depthChargesLauncherDataContainer.UpdateDataElements();
            depthChargesLauncherDataContainer.expanderKey = $"{ship.Index}_DC";
            if (!ShipDataContainer.ExpanderStateMapper.ContainsKey(depthChargesLauncherDataContainer.expanderKey))
            {
                ShipDataContainer.ExpanderStateMapper[depthChargesLauncherDataContainer.expanderKey] = true;
            }

            return depthChargesLauncherDataContainer;
        }
    }
}
