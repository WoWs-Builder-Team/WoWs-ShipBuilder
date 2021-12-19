// ReSharper disable InconsistentNaming
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.Core.DataUI
{
    public record ConcealmentUI : IDataUi
    {
        [JsonIgnore]
        public List<KeyValuePair<string, string>>? ConcealmentData { get; set; }

        [DataUiUnit("KM")]
        public decimal ConcealmentBySea { get; set; }

        [DataUiUnit("KM")]
        public decimal ConcealmentBySeaFiringSmoke { get; set; }

        [DataUiUnit("KM")]
        public decimal ConcealmentBySeaFire { get; set; }

        [DataUiUnit("KM")]
        public decimal ConcealmentByAir { get; set; }

        [DataUiUnit("KM")]
        public decimal ConcealmentByAirFire { get; set; }

        [DataUiUnit("KM")]
        public decimal ConcealmentBySubPeriscope { get; set; }

        [DataUiUnit("KM")]
        public decimal ConcealmentBySubOperating { get; set; }

        public static ConcealmentUI FromShip(Ship ship, List<ShipUpgrade> shipConfiguration, List<(string Key, float Value)> modifiers)
        {
            var hull = ship.Hulls[shipConfiguration.First(upgrade => upgrade.UcType == ComponentType.Hull).Components[ComponentType.Hull].First()];

            // Sea Detection
            var concealmentBySea = hull.SurfaceDetection;
            var concealmentBySeaFiringSmoke = hull.SmokeFiringDetection;

            // AA Detection
            var concealmentByAir = hull.AirDetection;

            var concealmentBySubPeriscope = hull.DetectionBySubPeriscope;
            var concealmentBySubOperating = hull.DetectionBySubOperating;

            int concealmentExpertIndex = modifiers.FindModifierIndex("visibilityDistCoeff");
            if (concealmentExpertIndex > -1)
            {
                var modifiersValues = modifiers.FindModifiers("visibilityDistCoeff").ToList();
                foreach (decimal value in modifiersValues)
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
            ShipUpgrade? artilleryConfiguration = shipConfiguration.FirstOrDefault(c => c.UcType == ComponentType.Artillery);
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

                Gun gun = mainBattery.Guns.First();

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

            var concealmentBySeaFire = concealmentBySea + 2.0m;
            var concealmentByAirFire = concealmentByAir + 3.0m;

            var concealment = new ConcealmentUI
            {
                ConcealmentBySea = Math.Round(concealmentBySea, 2),
                ConcealmentBySeaFiringSmoke = Math.Round(concealmentBySeaFiringSmoke, 2),
                ConcealmentBySeaFire = Math.Round(concealmentBySeaFire, 2),
                ConcealmentByAir = Math.Round(concealmentByAir, 2),
                ConcealmentByAirFire = Math.Round(concealmentByAirFire, 2),
                ConcealmentBySubOperating = Math.Round(concealmentBySubOperating, 2),
                ConcealmentBySubPeriscope = Math.Round(concealmentBySubPeriscope, 2),
            };

            concealment.ConcealmentData = concealment.ToPropertyMapping();

            return concealment;
        }
    }
}
