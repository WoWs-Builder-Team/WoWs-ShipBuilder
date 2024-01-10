using System.Collections.Immutable;
using WoWsShipBuilder.DataElements;
using WoWsShipBuilder.DataElements.DataElementAttributes;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Modifiers;
using WoWsShipBuilder.DataStructures.Ship;

namespace WoWsShipBuilder.Features.DataContainers;

[DataContainer]
public partial class DepthChargesLauncherDataContainer : DataContainerBase
{
    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "S")]
    public decimal Reload { get; set; }

    [DataElementType(DataElementTypes.KeyValue)]
    public int NumberOfUses { get; set; }

    [DataElementType(DataElementTypes.KeyValue)]
    public int BombsPerCharge { get; set; }

    public DepthChargeDataContainer? DepthCharge { get; set; }

    public static DepthChargesLauncherDataContainer? FromShip(Ship ship, ImmutableList<ShipUpgrade> shipConfiguration, List<Modifier> modifiers)
    {
        var shipHull = ship.Hulls[shipConfiguration.First(upgrade => upgrade.UcType == ComponentType.Hull).Components[ComponentType.Hull][0]];

        var depthChargesArray = shipHull.DepthChargeArray;

        if (depthChargesArray is null)
        {
            return null;
        }

        int ammoPerAttack = depthChargesArray.DepthCharges.Sum(charge => charge.DepthChargesNumber) * depthChargesArray.NumShots;
        string ammoName = depthChargesArray.DepthCharges.First(charge => charge.DepthChargesNumber > 0).AmmoList[0];

        int numberOfUses = modifiers.ApplyModifiers("DepthChargesLauncherDataContainer.NumberOfUses", depthChargesArray.MaxPacks);

        var ammo = DepthChargeDataContainer.FromChargesName(ammoName, modifiers);

        var depthChargesLauncherDataContainer = new DepthChargesLauncherDataContainer
        {
            Reload = depthChargesArray.Reload,
            NumberOfUses = numberOfUses,
            BombsPerCharge = ammoPerAttack,
            DepthCharge = ammo,
        };

        depthChargesLauncherDataContainer.UpdateDataElements();

        return depthChargesLauncherDataContainer;
    }
}
