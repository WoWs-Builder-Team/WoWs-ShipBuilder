using System.Collections.Immutable;
using WoWsShipBuilder.DataElements;
using WoWsShipBuilder.DataElements.DataElementAttributes;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Modifiers;
using WoWsShipBuilder.DataStructures.Ship;

namespace WoWsShipBuilder.Features.DataContainers;

[DataContainer]
public partial class ConcealmentDataContainer : DataContainerBase
{
    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "Sea", UnitKey = "KM")]
    public decimal ConcealmentBySea { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "Sea", UnitKey = "KM")]
    public decimal ConcealmentBySeaFire { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "Sea", UnitKey = "KM")]
    public decimal ConcealmentBySeaFiringSmoke { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "Air", UnitKey = "KM")]
    public decimal ConcealmentByAir { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "Air", UnitKey = "KM")]
    public decimal ConcealmentByAirFire { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "KM")]
    public decimal FromSubsAtPeriscopeDepth { get; set; }

    public static ConcealmentDataContainer FromShip(Ship ship, ImmutableList<ShipUpgrade> shipConfiguration, List<Modifier> modifiers)
    {
        var hull = ship.Hulls[shipConfiguration.First(upgrade => upgrade.UcType == ComponentType.Hull).Components[ComponentType.Hull][0]];

        // Sea Detection
        decimal concealmentBySea = hull.SurfaceDetection;
        decimal concealmentBySeaFiringSmoke = hull.SmokeFiringDetection;

        // AA Detection
        decimal concealmentByAir = hull.AirDetection;
        decimal concealmentBySubPeriscope = hull.DetectionBySubPeriscope;

        concealmentBySea = modifiers.ApplyModifiers("ConcealmentDataContainer.Visibility", concealmentBySea);
        concealmentByAir = modifiers.ApplyModifiers("ConcealmentDataContainer.Visibility", concealmentByAir);
        concealmentBySubPeriscope = modifiers.ApplyModifiers("ConcealmentDataContainer.Visibility", concealmentBySubPeriscope);

        // Checks for Heavy He
        var artilleryConfiguration = shipConfiguration.Find(c => c.UcType == ComponentType.Artillery);
        if (artilleryConfiguration != null)
        {
            var artilleryOptions = artilleryConfiguration.Components[ComponentType.Artillery];
            TurretModule? mainBattery;
            if (artilleryOptions.Length == 1)
            {
                mainBattery = ship.MainBatteryModuleList[artilleryConfiguration.Components[ComponentType.Artillery][0]];
            }
            else
            {
                string hullArtilleryName = shipConfiguration.First(c => c.UcType == ComponentType.Hull).Components[ComponentType.Artillery][0];
                mainBattery = ship.MainBatteryModuleList[hullArtilleryName];
            }

            var gun = mainBattery.Guns[0];

            // GMBigGunVisibilityCoeff
            if (gun.BarrelDiameter >= 0.149M)
            {
                concealmentBySea = modifiers.ApplyModifiers("ConcealmentDataContainer.Visibility.Big", concealmentBySea);
                concealmentByAir = modifiers.ApplyModifiers("ConcealmentDataContainer.Visibility.Big", concealmentByAir);
                concealmentBySubPeriscope = modifiers.ApplyModifiers("ConcealmentDataContainer.Visibility.Big", concealmentBySubPeriscope);
            }
        }

        decimal concealmentBySeaFire = concealmentBySea + 2.0m;
        decimal concealmentByAirFire = concealmentByAir + 3.0m;

        var concealment = new ConcealmentDataContainer
        {
            ConcealmentBySea = Math.Round(concealmentBySea, 2),
            ConcealmentBySeaFiringSmoke = Math.Round(concealmentBySeaFiringSmoke, 2),
            ConcealmentBySeaFire = Math.Round(concealmentBySeaFire, 2),
            ConcealmentByAir = Math.Round(concealmentByAir, 2),
            ConcealmentByAirFire = Math.Round(concealmentByAirFire, 2),
            FromSubsAtPeriscopeDepth = Math.Round(concealmentBySubPeriscope, 2),
        };

        concealment.UpdateDataElements();

        return concealment;
    }
}
