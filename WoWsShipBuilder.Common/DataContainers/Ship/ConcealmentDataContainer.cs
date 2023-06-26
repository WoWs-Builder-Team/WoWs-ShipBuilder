using WoWsShipBuilder.DataElements.DataElementAttributes;
using WoWsShipBuilder.DataElements.DataElements;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.Infrastructure.Utility;

namespace WoWsShipBuilder.DataContainers
{
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

        [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "FromSubs", UnitKey = "KM")]
        public decimal ConcealmentBySubPeriscope { get; set; }

        [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "FromSubs", UnitKey = "KM")]
        [DataElementFiltering(false)]
        public decimal ConcealmentBySubOperating { get; set; }

        public static ConcealmentDataContainer FromShip(Ship ship, List<ShipUpgrade> shipConfiguration, List<(string Key, float Value)> modifiers)
        {
            var hull = ship.Hulls[shipConfiguration.First(upgrade => upgrade.UcType == ComponentType.Hull).Components[ComponentType.Hull].First()];

            // Sea Detection
            decimal concealmentBySea = hull.SurfaceDetection;
            decimal concealmentBySeaFiringSmoke = hull.SmokeFiringDetection;

            // AA Detection
            decimal concealmentByAir = hull.AirDetection;

            decimal concealmentBySubPeriscope = hull.DetectionBySubPeriscope;
            decimal concealmentBySubOperating = hull.DetectionBySubOperating;

            int concealmentExpertIndex = modifiers.FindModifierIndex("visibilityDistCoeff");
            if (concealmentExpertIndex > -1)
            {
                List<float> modifiersValues = modifiers.FindModifiers("visibilityDistCoeff").ToList();
                foreach (decimal value in modifiersValues.Select(f => (decimal)f))
                {
                    concealmentBySea *= value;
                    concealmentByAir *= value;
                    concealmentBySubPeriscope *= value;
                    concealmentBySubOperating *= value;
                }
            }

            int visibilityFactorIndex = modifiers.FindModifierIndex("visibilityFactor", true);
            if (visibilityFactorIndex > -1)
            {
                var visibilityFactorModifier = (decimal)modifiers[visibilityFactorIndex].Value;
                concealmentBySea *= visibilityFactorModifier;
            }

            // Checks for Heavy He
            var artilleryConfiguration = shipConfiguration.FirstOrDefault(c => c.UcType == ComponentType.Artillery);
            if (artilleryConfiguration != null)
            {
                string[]? artilleryOptions = artilleryConfiguration.Components[ComponentType.Artillery];
                TurretModule? mainBattery;
                if (artilleryOptions.Length == 1)
                {
                    mainBattery = ship.MainBatteryModuleList[artilleryConfiguration.Components[ComponentType.Artillery].First()];
                }
                else
                {
                    string? hullArtilleryName = shipConfiguration.First(c => c.UcType == ComponentType.Hull).Components[ComponentType.Artillery].First();
                    mainBattery = ship.MainBatteryModuleList[hullArtilleryName];
                }

                var gun = mainBattery.Guns.First();

                // GMBigGunVisibilityCoeff
                if (gun.BarrelDiameter >= 0.149M)
                {
                    int bigGunVisibilityFactorIndex = modifiers.FindModifierIndex("GMBigGunVisibilityCoeff", true);
                    if (bigGunVisibilityFactorIndex > -1)
                    {
                        var bigGunVisibilityFactorModifier = (decimal)modifiers[bigGunVisibilityFactorIndex].Value;
                        concealmentBySea *= bigGunVisibilityFactorModifier;
                        concealmentByAir *= bigGunVisibilityFactorModifier;
                        concealmentBySubOperating *= bigGunVisibilityFactorModifier;
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
                ConcealmentBySubOperating = Math.Round(concealmentBySubOperating, 2),
                ConcealmentBySubPeriscope = Math.Round(concealmentBySubPeriscope, 2),
            };

            concealment.UpdateDataElements();

            return concealment;
        }
    }
}
