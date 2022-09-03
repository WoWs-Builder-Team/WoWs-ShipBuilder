using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.DataElements.DataElementAttributes;
using WoWsShipBuilder.DataElements.DataElements;
using WoWsShipBuilder.DataStructures;

// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace WoWsShipBuilder.Core.DataContainers
{
    public partial record MainBatteryDataContainer : DataContainerBase
    {
        [DataElementType(DataElementTypes.FormattedText, ValuesPropertyName = "TurretNames", ArePropertyNameValuesKeys = true)]
        public string Name { get; set; } = default!;

        public List<string> TurretNames { get; set; } = new();

        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "KM")]
        public decimal Range { get; set; }

        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "S")]
        public decimal TurnTime { get; set; }

        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "DegreePerSecond")]
        public decimal TraverseSpeed { get; set; }

        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "S")]
        public decimal Reload { get; set; }

        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "ShotsPerMinute")]
        public decimal RoF { get; set; }

        [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValue, GroupKey = "TheoreticalDpm")]
        [DataElementFiltering(true, "ShouldDisplayHeDpm")]
        public string TheoreticalHeDpm { get; set; } = default!;

        [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValue, GroupKey = "TheoreticalDpm")]
        [DataElementFiltering(true, "ShouldDisplaySapDpm")]
        public string TheoreticalSapDpm { get; set; } = default!;

        [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValue, GroupKey = "TheoreticalDpm")]
        [DataElementFiltering(true, "ShouldDisplayApDpm")]
        public string TheoreticalApDpm { get; set; } = default!;

        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "FPM")]
        [DataElementFiltering(true, "ShouldDisplayHeDpm")]
        public decimal PotentialFpm { get; set; }

        [DataElementType(DataElementTypes.KeyValue)]
        public decimal Sigma { get; set; }

        public decimal HorizontalDisp { get; set; }

        public decimal VerticalDisp { get; set; }

        public string HorizontalDispFormula { get; set; } = default!;

        public string VerticalCoeffFormula { get; set; } = default!;

        public string HorizontalDispFormulaAtShortRange { get; set; } = default!;

        public string VerticalCoeffFormulaAtShortRange { get; set; } = default!;

        public decimal DelimDist { get; set; }

        public decimal TaperDist { get; set; }

        public List<ShellDataContainer> ShellData { get; set; } = default!;

        public Dispersion DispersionData { get; set; } = default!;

        public TurretModule OriginalMainBatteryData { get; set; } = default!;

        public bool DisplayHeDpm { get; set; }

        public bool DisplayApDpm { get; set; }

        public bool DisplaySapDpm { get; set; }

        public static async Task<MainBatteryDataContainer?> FromShip(Ship ship, List<ShipUpgrade> shipConfiguration, List<(string name, float value)> modifiers, IAppDataService appDataService)
        {
            var artilleryConfiguration = shipConfiguration.FirstOrDefault(c => c.UcType == ComponentType.Artillery);
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

            var suoName = shipConfiguration.FirstOrDefault(c => c.UcType == ComponentType.Suo)?.Components[ComponentType.Suo].First();
            var suoConfiguration = suoName is not null ? ship.FireControlList[suoName] : null;

            List<(int BarrelCount, int TurretCount, string GunName)> arrangementList = mainBattery.Guns
                .GroupBy(gun => gun.NumBarrels)
                .Select(group => (BarrelCount: group.Key, TurretCount: group.Count(), GunName: group.First().Name))
                .OrderBy(item => item.TurretCount)
                .ToList();

            int barrelCount = arrangementList.Select(item => item.TurretCount * item.BarrelCount).Sum();

            var arrangementString = "";
            var turretNames = new List<string>();

            for (var i = 0; i < arrangementList.Count; i++)
            {
                var current = arrangementList[i];
                turretNames.Add(current.GunName);
                arrangementString += $"{current.TurretCount}x{current.BarrelCount} {{{i}}}\n";
            }

            var gun = mainBattery.Guns.First();

            // Calculate main battery reload
            var reloadModifiers = modifiers.FindModifiers("GMShotDelay");
            decimal reload = reloadModifiers.Aggregate(gun.Reload, (current, reloadModifier) => current * (decimal)reloadModifier);

            var arModifiers = modifiers.FindModifiers("lastChanceReloadCoefficient");
            reload = arModifiers.Aggregate(reload, (current, arModifier) => current * (1 - ((decimal)arModifier / 100)));

            var artilleryReloadCoeffModifiers = modifiers.FindModifiers("artilleryReloadCoeff");
            reload = artilleryReloadCoeffModifiers.Aggregate(reload, (current, artilleryReloadCoeff) => current * (decimal)artilleryReloadCoeff);

            // Rotation speed modifiers
            var turnSpeedModifiers = modifiers.FindModifiers("GMRotationSpeed");
            decimal traverseSpeed = turnSpeedModifiers.Aggregate(gun.HorizontalRotationSpeed, (current, modifier) => current * (decimal)modifier);

            // Range modifiers
            var rangeModifiers = modifiers.FindModifiers("GMMaxDist");
            decimal gunRange = mainBattery.MaxRange * (suoConfiguration?.MaxRangeModifier ?? 1);
            decimal range = rangeModifiers.Aggregate(gunRange, (current, modifier) => current * (decimal)modifier);
            var consumableRangeModifiers = modifiers.FindModifiers("artilleryDistCoeff");
            range = consumableRangeModifiers.Aggregate(range, (current, modifier) => current * (decimal)modifier);

            var talentRangeModifiers = modifiers.FindModifiers("talentMaxDistGM");
            range = talentRangeModifiers.Aggregate(range, (current, modifier) => current * (decimal)modifier) / 1000;

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

            modifiedDispersion.IdealRadius = modifiers.FindModifiers("GMIdealRadius").Aggregate(modifiedDispersion.IdealRadius, (current, modifier) => current * modifier);

            decimal rateOfFire = 60 / reload;

            var maxRangeBw = (double)(mainBattery.MaxRange / 30);
            double vRadiusCoeff = (modifiedDispersion.RadiusOnMax - modifiedDispersion.RadiusOnDelim) / (maxRangeBw * (1 - modifiedDispersion.Delim));

            var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            nfi.NumberGroupSeparator = "'";

            var shellData = await ShellDataContainer.FromShellName(mainBattery.Guns.First().AmmoList, modifiers, barrelCount, appDataService);

            // rounding reload in here to get a more accurate True reload
            var mainBatteryDataContainer = new MainBatteryDataContainer
            {
                Name = arrangementString,
                TurretNames = turretNames,
                Range = Math.Round(range, 2),
                Reload = Math.Round(reload, 2),
                RoF = Math.Round(rateOfFire * barrelCount, 1),
                TurnTime = Math.Round(180 / traverseSpeed, 1),
                TraverseSpeed = Math.Round(traverseSpeed, 2),
                Sigma = mainBattery.Sigma,
                DelimDist = mainBattery.MaxRange * (decimal)modifiedDispersion.Delim / 1000,
                TaperDist = (decimal)modifiedDispersion.TaperDist / 1000,
                HorizontalDisp = Math.Round((decimal)modifiedDispersion.CalculateHorizontalDispersion((double)range * 1000), 2),
                VerticalDisp = Math.Round((decimal)modifiedDispersion.CalculateVerticalDispersion((double)range * 1000), 2),
                HorizontalDispFormula = $"X * {Math.Round((modifiedDispersion.IdealRadius - modifiedDispersion.MinRadius) / modifiedDispersion.IdealDistance * 1000, 4)} + {30 * modifiedDispersion.MinRadius}",
                VerticalCoeffFormula = $"(X * {(decimal)Math.Round(vRadiusCoeff / 30 * 1000, 4)} + {((-maxRangeBw * modifiedDispersion.Delim) * vRadiusCoeff) + modifiedDispersion.RadiusOnDelim})",
                HorizontalDispFormulaAtShortRange = $"X * {Math.Round(((modifiedDispersion.IdealRadius - modifiedDispersion.MinRadius) / modifiedDispersion.IdealDistance * 1000) + (modifiedDispersion.MinRadius / (modifiedDispersion.TaperDist / 30)), 4)}",
                VerticalCoeffFormulaAtShortRange = $"(X * {(decimal)Math.Round(((modifiedDispersion.RadiusOnDelim - modifiedDispersion.RadiusOnZero) / (maxRangeBw * modifiedDispersion.Delim)) / 30 * 1000, 4)} + {modifiedDispersion.RadiusOnZero})",
                DispersionData = mainBattery.DispersionValues,
                OriginalMainBatteryData = mainBattery,
                ShellData = shellData,
                DisplayHeDpm = shellData.Select(x => x.Type).Contains($"ArmamentType_{ShellType.HE}"),
                DisplayApDpm = shellData.Select(x => x.Type).Contains($"ArmamentType_{ShellType.AP}"),
                DisplaySapDpm = shellData.Select(x => x.Type).Contains($"ArmamentType_{ShellType.SAP}"),
            };

            if (mainBatteryDataContainer.DisplayHeDpm)
            {
                var heShell = shellData.First(x => x.Type.Equals($"ArmamentType_{ShellType.HE}"));
                mainBatteryDataContainer.TheoreticalHeDpm = Math.Round(heShell.Damage * barrelCount * rateOfFire).ToString("n0", nfi);
                mainBatteryDataContainer.PotentialFpm = Math.Round(heShell.ShellFireChance / 100 * barrelCount * rateOfFire, 2);
            }

            if (mainBatteryDataContainer.DisplayApDpm)
            {
                decimal shellDamage = shellData.First(x => x.Type.Equals($"ArmamentType_{ShellType.AP}")).Damage;
                mainBatteryDataContainer.TheoreticalApDpm = Math.Round(shellDamage * barrelCount * rateOfFire).ToString("n0", nfi);
            }

            if (mainBatteryDataContainer.DisplaySapDpm)
            {
                decimal shellDamage = shellData.First(x => x.Type.Equals($"ArmamentType_{ShellType.SAP}")).Damage;
                mainBatteryDataContainer.TheoreticalSapDpm = Math.Round(shellDamage * barrelCount * rateOfFire).ToString("n0", nfi);
            }

            mainBatteryDataContainer.UpdateDataElements();
            return mainBatteryDataContainer;
        }

        private bool ShouldDisplayHeDpm(object obj)
        {
            return DisplayHeDpm;
        }

        private bool ShouldDisplayApDpm(object obj)
        {
            return DisplayApDpm;
        }

        private bool ShouldDisplaySapDpm(object obj)
        {
            return DisplaySapDpm;
        }
    }
}
