using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilder.DataElements.DataElementAttributes;
using WoWsShipBuilder.DataElements.DataElements;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Ship;

// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace WoWsShipBuilder.Core.DataContainers;

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

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValue, GroupKey = "FullSalvoDamage")]
    [DataElementFiltering(true, "ShouldDisplayHeDpm")]
    public string HeSalvo { get; set; } = default!;

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValue, GroupKey = "FullSalvoDamage")]
    [DataElementFiltering(true, "ShouldDisplayApDpm")]
    public string ApSalvo { get; set; } = default!;

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValue, GroupKey = "FullSalvoDamage")]
    [DataElementFiltering(true, "ShouldDisplaySapDpm")]
    public string SapSalvo { get; set; } = default!;

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

    public double DispersionModifier { get; set; }

    public TurretModule OriginalMainBatteryData { get; set; } = default!;

    public bool DisplayHeDpm { get; set; }

    public bool DisplayApDpm { get; set; }

    public bool DisplaySapDpm { get; set; }

    public decimal GunCaliber { get; set; }

    public int BarrelsCount { get; set; }

    public string BarrelsLayout { get; set; } = default!;

    public static MainBatteryDataContainer? FromShip(Ship ship, List<ShipUpgrade> shipConfiguration, List<(string name, float value)> modifiers)
    {
        var artilleryConfiguration = shipConfiguration.FirstOrDefault(c => c.UcType == ComponentType.Artillery);
        if (artilleryConfiguration == null)
        {
            return null;
        }

        string[] artilleryOptions = artilleryConfiguration.Components[ComponentType.Artillery];
        string[] supportedModules = artilleryConfiguration.Components[ComponentType.Artillery];

        TurretModule? mainBattery;
        if (artilleryOptions.Length == 1)
        {
            mainBattery = ship.MainBatteryModuleList[supportedModules.First()];
        }
        else
        {
            string hullArtilleryName = shipConfiguration.First(c => c.UcType == ComponentType.Hull).Components[ComponentType.Artillery].First(artilleryName => supportedModules.Contains(artilleryName));
            mainBattery = ship.MainBatteryModuleList[hullArtilleryName];
        }

        var suoName = shipConfiguration.FirstOrDefault(c => c.UcType == ComponentType.Suo)?.Components[ComponentType.Suo].First();
        var suoConfiguration = suoName is not null ? ship.FireControlList[suoName] : null;

        List<(int BarrelCount, int TurretCount, string GunName)> arrangementList = mainBattery.Guns
            .GroupBy(gun => gun.NumBarrels)
            .Select(group => (BarrelCount: group.Key, TurretCount: group.Count(), GunName: group.First().Name))
            .OrderBy(item => item.TurretCount)
            .ToList();

        var barrelCount = 0;
        var barrelLayout = new string[arrangementList.Count];
        StringBuilder arrangementString = new();
        var turretNames = new List<string>();

        for (var i = 0; i < arrangementList.Count; i++)
        {
            var current = arrangementList[i];
            turretNames.Add(current.GunName);
            arrangementString.AppendLine($"{current.TurretCount}x{current.BarrelCount} {{{i}}}");
            barrelLayout[i] = $"{current.TurretCount}x{current.BarrelCount}";
            barrelCount += current.TurretCount * current.BarrelCount;
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
        var dispersionModifier = modifiers.FindModifiers("GMIdealRadius").Aggregate(1f, (current, modifier) => current * modifier);
        var dispersion = mainBattery.DispersionValues;

        decimal rateOfFire = 60 / reload;

        var maxRangeBw = (double)(mainBattery.MaxRange / 30);
        double vRadiusCoeff = (dispersion.RadiusOnMax - dispersion.RadiusOnDelim) / (maxRangeBw * (1 - dispersion.Delim));

        var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
        nfi.NumberGroupSeparator = "'";

        var shellData = ShellDataContainer.FromShellName(mainBattery.Guns.First().AmmoList, modifiers, barrelCount, true);

        var (horizontalDispersion, verticalDispersion) = dispersion.CalculateDispersion((double)range * 1000, dispersionModifier);

        // rounding reload in here to get a more accurate True reload
        var mainBatteryDataContainer = new MainBatteryDataContainer
        {
            Name = arrangementString.ToString(),
            TurretNames = turretNames,
            Range = Math.Round(range, 2),
            Reload = Math.Round(reload, 2),
            RoF = Math.Round(rateOfFire * barrelCount, 1),
            TurnTime = Math.Round(180 / traverseSpeed, 1),
            TraverseSpeed = Math.Round(traverseSpeed, 2),
            Sigma = mainBattery.Sigma,
            DelimDist = mainBattery.MaxRange * (decimal)dispersion.Delim / 1000,
            TaperDist = (decimal)dispersion.TaperDist / 1000,
            HorizontalDisp = Math.Round((decimal)horizontalDispersion, 2),
            VerticalDisp = Math.Round((decimal)verticalDispersion, 2),
            HorizontalDispFormula = $"X * {Math.Round((dispersion.IdealRadius - dispersion.MinRadius) / dispersion.IdealDistance * 1000, 4)} + {30 * dispersion.MinRadius}",
            VerticalCoeffFormula = $"(X * {(decimal)Math.Round(vRadiusCoeff / 30 * 1000, 4)} + {((-maxRangeBw * dispersion.Delim) * vRadiusCoeff) + dispersion.RadiusOnDelim})",
            HorizontalDispFormulaAtShortRange = $"X * {Math.Round(((dispersion.IdealRadius - dispersion.MinRadius) / dispersion.IdealDistance * 1000) + (dispersion.MinRadius / (dispersion.TaperDist / 30)), 4)}",
            VerticalCoeffFormulaAtShortRange = $"(X * {(decimal)Math.Round(((dispersion.RadiusOnDelim - dispersion.RadiusOnZero) / (maxRangeBw * dispersion.Delim)) / 30 * 1000, 4)} + {dispersion.RadiusOnZero})",
            DispersionData = dispersion,
            DispersionModifier = dispersionModifier,
            OriginalMainBatteryData = mainBattery,
            ShellData = shellData,
            DisplayHeDpm = shellData.Select(x => x.Type).Contains($"ArmamentType_{ShellType.HE.ShellTypeToString()}"),
            DisplayApDpm = shellData.Select(x => x.Type).Contains($"ArmamentType_{ShellType.AP.ShellTypeToString()}"),
            DisplaySapDpm = shellData.Select(x => x.Type).Contains($"ArmamentType_{ShellType.SAP.ShellTypeToString()}"),
            GunCaliber = Math.Round(gun.BarrelDiameter * 1000),
            BarrelsCount = barrelCount,
            BarrelsLayout = string.Join(" + ", barrelLayout),
        };

        if (mainBatteryDataContainer.DisplayHeDpm)
        {
            var heShell = shellData.First(x => x.Type.Equals($"ArmamentType_{ShellType.HE.ShellTypeToString()}"));
            mainBatteryDataContainer.TheoreticalHeDpm = Math.Round(heShell.Damage * barrelCount * rateOfFire).ToString("n0", nfi);
            mainBatteryDataContainer.HeSalvo = Math.Round(heShell.Damage * barrelCount).ToString("n0", nfi);
            mainBatteryDataContainer.PotentialFpm = Math.Round(heShell.ShellFireChance / 100 * barrelCount * rateOfFire, 2);
        }

        if (mainBatteryDataContainer.DisplayApDpm)
        {
            decimal shellDamage = shellData.First(x => x.Type.Equals($"ArmamentType_{ShellType.AP.ShellTypeToString()}")).Damage;
            mainBatteryDataContainer.TheoreticalApDpm = Math.Round(shellDamage * barrelCount * rateOfFire).ToString("n0", nfi);
            mainBatteryDataContainer.ApSalvo = Math.Round(shellDamage * barrelCount).ToString("n0", nfi);
        }

        if (mainBatteryDataContainer.DisplaySapDpm)
        {
            decimal shellDamage = shellData.First(x => x.Type.Equals($"ArmamentType_{ShellType.SAP.ShellTypeToString()}")).Damage;
            mainBatteryDataContainer.TheoreticalSapDpm = Math.Round(shellDamage * barrelCount * rateOfFire).ToString("n0", nfi);
            mainBatteryDataContainer.SapSalvo = Math.Round(shellDamage * barrelCount).ToString("n0", nfi);
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
