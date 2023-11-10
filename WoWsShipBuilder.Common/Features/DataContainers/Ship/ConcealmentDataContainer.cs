using WoWsShipBuilder.DataElements;
using WoWsShipBuilder.DataElements.DataElementAttributes;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.Infrastructure.Utility;

namespace WoWsShipBuilder.Features.DataContainers;

[DataContainer]
public partial record ConcealmentDataContainer : DataContainerBase
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

    public static ConcealmentDataContainer FromShip(Ship ship, List<ShipUpgrade> shipConfiguration, List<(string Key, float Value)> modifiers)
    {
        var hull = ship.Hulls[shipConfiguration.First(upgrade => upgrade.UcType == ComponentType.Hull).Components[ComponentType.Hull][0]];

        // Sea Detection
        decimal concealmentBySea = hull.SurfaceDetection;
        decimal concealmentBySeaFiringSmoke = hull.SmokeFiringDetection;

        // AA Detection
        decimal concealmentByAir = hull.AirDetection;
        decimal concealmentBySubPeriscope = hull.DetectionBySubPeriscope;

        int concealmentExpertIndex = modifiers.FindModifierIndex("visibilityDistCoeff");
        if (concealmentExpertIndex > -1)
        {
            var modifiersValues = modifiers.FindModifiers("visibilityDistCoeff").ToList();
            foreach (decimal value in modifiersValues.Select(f => (decimal)f))
            {
                concealmentBySea *= value;
                concealmentByAir *= value;
                concealmentBySubPeriscope *= value;
            }
        }

        int visibilityFactorIndex = modifiers.FindModifierIndex("visibilityFactor", true);
        if (visibilityFactorIndex > -1)
        {
            var visibilityFactorModifier = (decimal)modifiers[visibilityFactorIndex].Value;
            concealmentBySea *= visibilityFactorModifier;
        }

        // Checks for Heavy He
        var artilleryConfiguration = shipConfiguration.Find(c => c.UcType == ComponentType.Artillery);
        if (artilleryConfiguration != null)
        {
            string[] artilleryOptions = artilleryConfiguration.Components[ComponentType.Artillery];
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
                int bigGunVisibilityFactorIndex = modifiers.FindModifierIndex("GMBigGunVisibilityCoeff", true);
                if (bigGunVisibilityFactorIndex > -1)
                {
                    var bigGunVisibilityFactorModifier = (decimal)modifiers[bigGunVisibilityFactorIndex].Value;
                    concealmentBySea *= bigGunVisibilityFactorModifier;
                    concealmentByAir *= bigGunVisibilityFactorModifier;
                    concealmentBySubPeriscope *= bigGunVisibilityFactorModifier;
                }
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
