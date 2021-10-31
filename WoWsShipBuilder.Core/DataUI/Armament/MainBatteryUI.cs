using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilderDataStructures;

// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace WoWsShipBuilder.Core.DataUI
{
    // ReSharper disable once InconsistentNaming
    public record MainBatteryUI : IDataUi
    {
        [JsonIgnore]
        public string Name { get; set; } = default!;

        [DataUiUnit("KM")]
        public decimal Range { get; set; }

        [DataUiUnit("S")]
        public decimal Reload { get; set; }

        [DataUiUnit("ShotsPerSecond")]
        public decimal RoF { get; set; }

        [DataUiUnit("S")]
        public decimal TurnTime { get; set; }

        [DataUiUnit("DegreePerSecond")]
        public decimal TraverseSpeed { get; set; }

        public decimal Sigma { get; set; }

        [JsonIgnore]
        public string HorizontalDisp { get; set; } = default!;

        [JsonIgnore]
        public string VerticalDisp { get; set; } = default!;

        [JsonIgnore]
        public List<ShellUI> ShellData { get; set; } = default!;

        [JsonIgnore]
        public Dispersion DispersionData { get; set; } = default!;

        [JsonIgnore]
        public TurretModule OriginalMainBatteryData { get; set; } = default!;

        [JsonIgnore]
        public List<KeyValuePair<string, string>> PropertyValueMapper { get; set; } = default!;

        public static MainBatteryUI? FromShip(Ship ship, List<ShipUpgrade> shipConfiguration, List<(string name, float value)> modifiers)
        {
            ShipUpgrade? artilleryConfiguration = shipConfiguration.FirstOrDefault(c => c.UcType == ComponentType.Artillery);
            if (artilleryConfiguration == null)
            {
                return null;
            }

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

            FireControl? suoConfiguration = ship.FireControlList[shipConfiguration.First(c => c.UcType == ComponentType.Suo).Components[ComponentType.Suo].First()];

            List<(int BarrelCount, int TurretCount)> arrangementList = mainBattery.Guns
                .GroupBy(gun => gun.NumBarrels)
                .Select(group => (BarrelCount: group.Key, TurretCount: group.Count()))
                .OrderBy(item => item.TurretCount)
                .ToList();
            string turretArrangement = string.Join(", ", arrangementList.Select(item => $"{item.TurretCount} x {item.BarrelCount}"));
            int barrelCount = arrangementList.Select(item => item.TurretCount * item.BarrelCount).Sum();
            Gun gun = mainBattery.Guns.First();

            // Calculate main battery reload
            var reloadModifiers = modifiers.FindModifiers("GMShotDelay");
            decimal reload = Math.Round(reloadModifiers.Aggregate(gun.Reload, (current, reloadModifier) => current * (decimal)reloadModifier), 2);

            // Rotation speed modifiers
            var turnSpeedModifiers = modifiers.FindModifiers("GMRotationSpeed");
            decimal traverseSpeed =
                Math.Round(turnSpeedModifiers.Aggregate(gun.HorizontalRotationSpeed, (current, modifier) => current * (decimal)modifier), 2);

            // Range modifiers
            var rangeModifiers = modifiers.FindModifiers("GMMaxDist");
            decimal gunRange = mainBattery.MaxRange * suoConfiguration.MaxRangeModifier;
            decimal range = Math.Round(rangeModifiers.Aggregate(gunRange, (current, modifier) => current * (decimal)modifier) / 1000, 2);

            // Consider dispersion modifiers
            var modifiedDispersion = new Dispersion
            {
                IdealRadius = mainBattery.DispersionValues.IdealRadius,
                MinRadius = mainBattery.DispersionValues.MinRadius,
                IdealDistance = mainBattery.DispersionValues.IdealDistance,
                TaperDist = mainBattery.DispersionValues.TaperDist,
                RadiusOnZero = mainBattery.DispersionValues.RadiusOnZero,
                RadiusOnDelim = mainBattery.DispersionValues.RadiusOnDelim,
                RadiusOnMax = mainBattery.DispersionValues.RadiusOnMax,
                Delim = mainBattery.DispersionValues.Delim,
            };

            modifiedDispersion.IdealRadius = modifiers.FindModifiers("GMIdealRadius")
                .Aggregate(modifiedDispersion.IdealRadius, (current, modifier) => current * modifier);

            // Adjust dispersion for range modifiers
            decimal hDispersion = Math.Round((decimal)modifiedDispersion.CalculateHorizontalDispersion((double)mainBattery.MaxRange), 2);
            decimal vDispersion = Math.Round((decimal)modifiedDispersion.CalculateVerticalDispersion((double)mainBattery.MaxRange), 2);

            var mainBatteryUi = new MainBatteryUI
            {
                Name = turretArrangement + " " + Localizer.Instance[mainBattery.Guns.First().Name].Localization,
                Range = range,
                Reload = reload,
                RoF = Math.Round(60 / reload),
                TurnTime = Math.Round(180 / traverseSpeed, 1),
                TraverseSpeed = traverseSpeed,
                Sigma = mainBattery.Sigma,
                HorizontalDisp = hDispersion + " m",
                VerticalDisp = vDispersion + " m",
                DispersionData = mainBattery.DispersionValues,
                OriginalMainBatteryData = mainBattery,
            };
            var shellNames = mainBattery.Guns.First().AmmoList;
            mainBatteryUi.ShellData = ShellUI.FromShip(shellNames, modifiers, mainBatteryUi.RoF * barrelCount);
            mainBatteryUi.PropertyValueMapper = mainBatteryUi.ToPropertyMapping();
            return mainBatteryUi;
        }
    }
}
