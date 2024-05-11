using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using WoWsShipBuilder.DataElements;
using WoWsShipBuilder.DataElements.DataElementAttributes;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Modifiers;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.Infrastructure.GameData;

// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace WoWsShipBuilder.Features.DataContainers;

[DataContainer]
public partial class MainBatteryDataContainer : DataContainerBase
{
    [DataElementType(DataElementTypes.FormattedText, ArgumentsCollectionName = "TurretNames", ArgumentsTextKind = TextKind.LocalizationKey)]
    public string Name { get; set; } = default!;

    public ImmutableList<string> TurretNames { get; set; } = ImmutableList<string>.Empty;

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "KM")]
    public decimal Range { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "S")]
    public decimal TurnTime { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "DegreePerSecond")]
    public decimal TraverseSpeed { get; set; }

    [DataElementType(DataElementTypes.Tooltip, UnitKey = "S", TooltipKey = "AmmoSwitchTimeTooltip")]
    public decimal AmmoSwitchTime { get; set; }

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

    public ImmutableList<ShellDataContainer> ShellData { get; set; } = ImmutableList<ShellDataContainer>.Empty;

    public Dispersion DispersionData { get; set; } = default!;

    public double DispersionModifier { get; set; }

    public TurretModule OriginalMainBatteryData { get; set; } = default!;

    public bool DisplayHeDpm { get; set; }

    public bool DisplayApDpm { get; set; }

    public bool DisplaySapDpm { get; set; }

    public decimal GunCaliber { get; set; }

    public int BarrelsCount { get; set; }

    public string BarrelsLayout { get; set; } = default!;

    public static MainBatteryDataContainer? FromShip(Ship ship, ImmutableList<ShipUpgrade> shipConfiguration, ImmutableList<Modifier> modifiers)
    {
        var artilleryConfiguration = shipConfiguration.Find(c => c.UcType == ComponentType.Artillery);
        if (artilleryConfiguration == null)
        {
            return null;
        }

        var artilleryOptions = artilleryConfiguration.Components[ComponentType.Artillery];
        var supportedModules = artilleryConfiguration.Components[ComponentType.Artillery];

        TurretModule? mainBattery;
        if (artilleryOptions.Length == 1)
        {
            mainBattery = ship.MainBatteryModuleList[supportedModules[0]];
        }
        else
        {
            string hullArtilleryName = shipConfiguration.First(c => c.UcType == ComponentType.Hull).Components[ComponentType.Artillery].First(artilleryName => supportedModules.Contains(artilleryName));
            mainBattery = ship.MainBatteryModuleList[hullArtilleryName];
        }

        var suoName = shipConfiguration.Find(c => c.UcType == ComponentType.Suo)?.Components[ComponentType.Suo][0];
        var suoConfiguration = suoName is not null ? ship.FireControlList[suoName] : null;

        var arrangementList = mainBattery.Guns
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
            arrangementString.AppendLine(CultureInfo.InvariantCulture, $"{current.TurretCount}x{current.BarrelCount} {{{i}}}");
            barrelLayout[i] = $"{current.TurretCount}x{current.BarrelCount}";
            barrelCount += current.TurretCount * current.BarrelCount;
        }

        var gun = mainBattery.Guns[0];

        // Calculate main battery reload
        decimal reload = modifiers.ApplyModifiers("MainBatteryDataContainer.Reload", gun.Reload);

        decimal ammoSwitchTime = modifiers.ApplyModifiers("MainBatteryDataContainer.AmmoSwitchTime", reload * gun.AmmoSwitchCoeff);

        // Rotation speed modifiers
        decimal traverseSpeed = modifiers.ApplyModifiers("MainBatteryDataContainer.TraverseSpeed", gun.HorizontalRotationSpeed);

        // Range modifiers
        decimal gunRange = mainBattery.MaxRange * (suoConfiguration?.MaxRangeModifier ?? 1);
        decimal range = modifiers.ApplyModifiers("MainBatteryDataContainer.Range", gunRange) / 1000;

        // Consider dispersion modifiers
        var dispersionModifier = (float)modifiers.ApplyModifiers("MainBatteryDataContainer.Dispersion.IdealRadius", 1m);
        var dispersion = gun.Dispersion;

        decimal rateOfFire = 60 / reload;

        var maxRangeBw = (double)(range / 30);
        double vRadiusCoeff = (dispersion.RadiusOnMax - dispersion.RadiusOnDelim) / (maxRangeBw * (1 - dispersion.Delim));

        var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
        nfi.NumberGroupSeparator = "'";

        var shellData = ShellDataContainer.FromShellName(gun.AmmoList, modifiers, barrelCount, true);

        var (horizontalDispersion, verticalDispersion) = dispersion.CalculateDispersion((double)range * 1000, dispersionModifier);

        // rounding reload in here to get a more accurate True reload
        var mainBatteryDataContainer = new MainBatteryDataContainer
        {
            Name = arrangementString.ToString(),
            TurretNames = turretNames.ToImmutableList(),
            Range = Math.Round(range, 2),
            Reload = Math.Round(reload, 2),
            AmmoSwitchTime = Math.Round(ammoSwitchTime, 2),
            RoF = Math.Round(rateOfFire * barrelCount, 1),
            TurnTime = Math.Round(180 / traverseSpeed, 1),
            TraverseSpeed = Math.Round(traverseSpeed, 2),
            Sigma = mainBattery.Sigma,
            DelimDist = (range * (decimal)dispersion.Delim) / 1000,
            TaperDist = (decimal)dispersion.TaperDist / 1000,
            HorizontalDisp = Math.Round((decimal)horizontalDispersion, 2),
            VerticalDisp = Math.Round((decimal)verticalDispersion, 2),
            HorizontalDispFormula = $"X * {Math.Round(((dispersion.IdealRadius - dispersion.MinRadius) / dispersion.IdealDistance) * 1000, 4)} + {30 * dispersion.MinRadius}",
            VerticalCoeffFormula = $"(X * {(decimal)Math.Round((vRadiusCoeff / 30) * 1000, 4)} + {(-maxRangeBw * dispersion.Delim * vRadiusCoeff) + dispersion.RadiusOnDelim})",
            HorizontalDispFormulaAtShortRange = $"X * {Math.Round((((dispersion.IdealRadius - dispersion.MinRadius) / dispersion.IdealDistance) * 1000) + (dispersion.MinRadius / (dispersion.TaperDist / 30)), 4)}",
            VerticalCoeffFormulaAtShortRange = $"(X * {(decimal)Math.Round(((dispersion.RadiusOnDelim - dispersion.RadiusOnZero) / (maxRangeBw * dispersion.Delim) / 30) * 1000, 4)} + {dispersion.RadiusOnZero})",
            DispersionData = dispersion,
            DispersionModifier = dispersionModifier,
            OriginalMainBatteryData = mainBattery,
            ShellData = shellData.ToImmutableList(),
            DisplayHeDpm = shellData.Select(x => x.Type).Contains($"ArmamentType_{ShellType.HE.ShellTypeToString()}"),
            DisplayApDpm = shellData.Select(x => x.Type).Contains($"ArmamentType_{ShellType.AP.ShellTypeToString()}"),
            DisplaySapDpm = shellData.Select(x => x.Type).Contains($"ArmamentType_{ShellType.SAP.ShellTypeToString()}"),
            GunCaliber = Math.Round(gun.BarrelDiameter * 1000),
            BarrelsCount = barrelCount,
            BarrelsLayout = string.Join(" + ", barrelLayout),
        };

        if (mainBatteryDataContainer.DisplayHeDpm)
        {
            var heShell = shellData.First(x => x.Type.Equals($"ArmamentType_{ShellType.HE.ShellTypeToString()}", StringComparison.Ordinal));
            mainBatteryDataContainer.TheoreticalHeDpm = Math.Round(heShell.Damage * barrelCount * rateOfFire).ToString("n0", nfi);
            mainBatteryDataContainer.HeSalvo = Math.Round(heShell.Damage * barrelCount).ToString("n0", nfi);
            mainBatteryDataContainer.PotentialFpm = Math.Round(heShell.ShellFireChance / 100 * barrelCount * rateOfFire, 2);
        }

        if (mainBatteryDataContainer.DisplayApDpm)
        {
            decimal shellDamage = shellData.First(x => x.Type.Equals($"ArmamentType_{ShellType.AP.ShellTypeToString()}", StringComparison.Ordinal)).Damage;
            mainBatteryDataContainer.TheoreticalApDpm = Math.Round(shellDamage * barrelCount * rateOfFire).ToString("n0", nfi);
            mainBatteryDataContainer.ApSalvo = Math.Round(shellDamage * barrelCount).ToString("n0", nfi);
        }

        if (mainBatteryDataContainer.DisplaySapDpm)
        {
            decimal shellDamage = shellData.First(x => x.Type.Equals($"ArmamentType_{ShellType.SAP.ShellTypeToString()}", StringComparison.Ordinal)).Damage;
            mainBatteryDataContainer.TheoreticalSapDpm = Math.Round(shellDamage * barrelCount * rateOfFire).ToString("n0", nfi);
            mainBatteryDataContainer.SapSalvo = Math.Round(shellDamage * barrelCount).ToString("n0", nfi);
        }

        mainBatteryDataContainer.UpdateDataElements();
        return mainBatteryDataContainer;
    }

    private bool ShouldDisplayHeDpm(object obj)
    {
        return this.DisplayHeDpm;
    }

    private bool ShouldDisplayApDpm(object obj)
    {
        return this.DisplayApDpm;
    }

    private bool ShouldDisplaySapDpm(object obj)
    {
        return this.DisplaySapDpm;
    }
}
