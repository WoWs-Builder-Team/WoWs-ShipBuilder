using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.Core.Translations;
using WoWsShipBuilder.DataStructures;

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
        public decimal TurnTime { get; set; }

        [DataUiUnit("DegreePerSecond")]
        public decimal TraverseSpeed { get; set; }

        public decimal Sigma { get; set; }

        [DataUiUnit("ShotsPerMinute")]
        public decimal RoF { get; set; }

        [DataUiUnit("ShotsPerMinute")]
        public decimal TrueRoF { get; set; } = default!;

        [DataUiUnit("S")]
        public decimal Reload { get; set; }

        [JsonIgnore]
        public string TrueReload { get; set; } = default!;

        [JsonIgnore]
        public string HorizontalDisp { get; set; } = default!;

        [JsonIgnore]
        public string VerticalDisp { get; set; } = default!;

        [JsonIgnore]
        public string HorizontalDispFormula { get; set; } = default!;

        [JsonIgnore]
        public string VerticalCoeffFormula { get; set; } = default!;

        [JsonIgnore]
        public string HorizontalDispFormulaAtShortRange { get; set; } = default!;

        [JsonIgnore]
        public string VerticalCoeffFormulaAtShortRange { get; set; } = default!;

        [JsonIgnore]
        public string DelimDist { get; set; } = default!;

        [JsonIgnore]
        public string TaperDist { get; set; } = default!;

        [JsonIgnore]
        public List<ShellUI> ShellData { get; set; } = default!;

        [JsonIgnore]
        public Dispersion DispersionData { get; set; } = default!;

        [JsonIgnore]
        public TurretModule OriginalMainBatteryData { get; set; } = default!;

        [JsonIgnore]
        public List<KeyValuePair<string, string>> PropertyValueMapper { get; set; } = default!;

        public static MainBatteryUI? FromShip(Ship ship, List<ShipUpgrade> shipConfiguration, List<(string name, float value)> modifiers, IAppDataService appDataService)
        {
            ShipUpgrade? artilleryConfiguration = shipConfiguration.FirstOrDefault(c => c.UcType == ComponentType.Artillery);
            if (artilleryConfiguration == null)
            {
                return null;
            }

            string[]? artilleryOptions = artilleryConfiguration.Components[ComponentType.Artillery];
            string[]? supportedModules = artilleryConfiguration.Components[ComponentType.Artillery];

            TurretModule? mainBattery;
            if (artilleryOptions.Length == 1)
            {
                mainBattery = ship.MainBatteryModuleList[supportedModules.First()];
            }
            else
            {
                string? hullArtilleryName = shipConfiguration.First(c => c.UcType == ComponentType.Hull).Components[ComponentType.Artillery].First(artilleryName => supportedModules.Contains(artilleryName));
                mainBattery = ship.MainBatteryModuleList[hullArtilleryName];
            }

            FireControl? suoConfiguration = ship.FireControlList[shipConfiguration.First(c => c.UcType == ComponentType.Suo).Components[ComponentType.Suo].First()];

            List<(int BarrelCount, int TurretCount, string GunName)> arrangementList = mainBattery.Guns
                .GroupBy(gun => gun.NumBarrels)
                .Select(group => (BarrelCount: group.Key, TurretCount: group.Count(), GunName: group.First().Name))
                .OrderBy(item => item.TurretCount)
                .ToList();
            string turretArrangement = string.Join($"\n", arrangementList.Select(item => $"{item.TurretCount} x {item.BarrelCount} {Localizer.Instance[item.GunName].Localization}"));
            int barrelCount = arrangementList.Select(item => item.TurretCount * item.BarrelCount).Sum();
            Gun gun = mainBattery.Guns.First();

            // Calculate main battery reload
            var reloadModifiers = modifiers.FindModifiers("GMShotDelay");
            decimal reload = reloadModifiers.Aggregate(gun.Reload, (current, reloadModifier) => current * (decimal)reloadModifier);

            var arModifiers = modifiers.FindModifiers("lastChanceReloadCoefficient");
            reload = arModifiers.Aggregate(reload, (current, arModifier) => current * (1 - ((decimal)arModifier / 100)));

            var artilleryReloadCoeffModifiers = modifiers.FindModifiers("artilleryReloadCoeff");
            reload = artilleryReloadCoeffModifiers.Aggregate(reload, (current, artilleryReloadCoeff) => current * (decimal)artilleryReloadCoeff);

            // Rotation speed modifiers
            var turnSpeedModifiers = modifiers.FindModifiers("GMRotationSpeed");
            decimal traverseSpeed =
                Math.Round(turnSpeedModifiers.Aggregate(gun.HorizontalRotationSpeed, (current, modifier) => current * (decimal)modifier), 2);

            // Range modifiers
            var rangeModifiers = modifiers.FindModifiers("GMMaxDist");
            decimal gunRange = mainBattery.MaxRange * suoConfiguration.MaxRangeModifier;
            decimal range = rangeModifiers.Aggregate(gunRange, (current, modifier) => current * (decimal)modifier);

            var talentRangeModifiers = modifiers.FindModifiers("talentMaxDistGM");
            range = Math.Round(talentRangeModifiers.Aggregate(range, (current, modifier) => current * (decimal)modifier) / 1000, 2);

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

            decimal rateOfFire = 60 / reload;

            var maxRangeBW = (double)(mainBattery.MaxRange / 30);
            var vRadiusCoeff = (modifiedDispersion.RadiusOnMax - modifiedDispersion.RadiusOnDelim) / (maxRangeBW * (1 - modifiedDispersion.Delim));

            var trueReload = Math.Ceiling(reload / Constants.TickRate) * Constants.TickRate;
            decimal trueRateOfFire = 60 / trueReload;

            // rounding reload in here to get a more accurate True reload
            var mainBatteryUi = new MainBatteryUI
            {
                Name = turretArrangement,
                Range = range,
                Reload = Math.Round(reload, 2),
                TrueReload = Math.Round(trueReload, 2) + " " + Translation.Unit_S,
                RoF = Math.Round(rateOfFire * barrelCount, 1),
                TrueRoF = Math.Round(trueRateOfFire * barrelCount, 1),
                TurnTime = Math.Round(180 / traverseSpeed, 1),
                TraverseSpeed = traverseSpeed,
                Sigma = mainBattery.Sigma,
                DelimDist = $"{(double)mainBattery.MaxRange * modifiedDispersion.Delim / 1000} " + Translation.Unit_KM,
                TaperDist = $"{modifiedDispersion.TaperDist / 1000} " + Translation.Unit_KM,
                HorizontalDisp = hDispersion + " " + Translation.Unit_M,
                VerticalDisp = vDispersion + " " + Translation.Unit_M,
                HorizontalDispFormula = $"X * {Math.Round((modifiedDispersion.IdealRadius - modifiedDispersion.MinRadius) / modifiedDispersion.IdealDistance * 1000, 4)} + {30 * modifiedDispersion.MinRadius}",
                VerticalCoeffFormula = $"(X * {(decimal)Math.Round(vRadiusCoeff / 30 * 1000, 4)} + {((-maxRangeBW * modifiedDispersion.Delim) * vRadiusCoeff) + modifiedDispersion.RadiusOnDelim})",
                HorizontalDispFormulaAtShortRange = $"X * {Math.Round(((modifiedDispersion.IdealRadius - modifiedDispersion.MinRadius) / modifiedDispersion.IdealDistance * 1000) + (modifiedDispersion.MinRadius / (modifiedDispersion.TaperDist / 30)), 4)}",
                VerticalCoeffFormulaAtShortRange = $"(X * {(decimal)Math.Round(((modifiedDispersion.RadiusOnDelim - modifiedDispersion.RadiusOnZero) / (maxRangeBW * modifiedDispersion.Delim)) / 30 * 1000, 4)} + {modifiedDispersion.RadiusOnZero})",
                DispersionData = mainBattery.DispersionValues,
                OriginalMainBatteryData = mainBattery,
            };

            var shellNames = mainBattery.Guns.First().AmmoList;
            mainBatteryUi.ShellData = ShellUI.FromShellName(shellNames, modifiers, barrelCount, rateOfFire, trueRateOfFire, appDataService);
            mainBatteryUi.PropertyValueMapper = mainBatteryUi.ToPropertyMapping();
            return mainBatteryUi;
        }
    }
}
