using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using WoWsShipBuilder.DataElements;
using WoWsShipBuilder.DataElements.DataElementAttributes;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Modifiers;
using WoWsShipBuilder.DataStructures.Ship;

namespace WoWsShipBuilder.Features.DataContainers;

[DataContainer]
public partial class TorpedoArmamentDataContainer : DataContainerBase
{
    [DataElementType(DataElementTypes.FormattedText, ArgumentsCollectionName = "LauncherNames", ArgumentsTextKind = TextKind.LocalizationKey)]
    public string Name { get; set; } = default!;

    public List<string> LauncherNames { get; set; } = new();

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValue, GroupKey = "Loaders")]
    public string BowLoaders { get; set; } = default!;

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValue, GroupKey = "Loaders")]
    public string SternLoaders { get; set; } = default!;

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValue, GroupKey = "Loaders")]
    public string LeftSideLoaders { get; set; } = default!;

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValue, GroupKey = "Loaders")]
    public string RightSideLoaders { get; set; } = default!;

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "S")]
    public decimal TurnTime { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "DegreePerSecond")]
    public decimal TraverseSpeed { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "S")]
    public decimal Reload { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "Degree")]
    public string TorpedoArea { get; set; } = default!;

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "S")]
    public decimal TimeToSwitch { get; set; }

    [DataElementType(DataElementTypes.KeyValue)]
    public string FullSalvoDamage { get; set; } = default!;

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValue, GroupKey = "FullSalvoDamage", LocalizationKeyOverride = "FirstOption")]
    public string TorpFullSalvoDmg { get; set; } = default!;

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValue, GroupKey = "FullSalvoDamage", LocalizationKeyOverride = "SecondOption")]
    public string AltTorpFullSalvoDmg { get; set; } = default!;

    public int LoadersCount { get; set; }

    public int TorpCount { get; set; }

    public string TorpLayout { get; set; } = default!;

    public List<TorpedoDataContainer> Torpedoes { get; set; } = new();

    public IEnumerable<TorpedoLauncher> TorpedoLaunchers { get; private set; } = default!;

    public static TorpedoArmamentDataContainer? FromShip(Ship ship, List<ShipUpgrade> shipConfiguration, List<Modifier> modifiers)
    {
        var torpConfiguration = shipConfiguration.Find(c => c.UcType == ComponentType.Torpedoes);
        if (torpConfiguration == null)
        {
            return null;
        }

        ImmutableArray<string> torpedoOptions = torpConfiguration.Components[ComponentType.Torpedoes];
        ImmutableArray<string> supportedModules = torpConfiguration.Components[ComponentType.Torpedoes];

        TorpedoModule? torpedoModule;
        if (torpedoOptions.Length == 1)
        {
            torpedoModule = ship.TorpedoModules[supportedModules[0]];
        }
        else
        {
            string hullTorpedoName = shipConfiguration.First(c => c.UcType == ComponentType.Hull).Components[ComponentType.Torpedoes].First(torpedoName => supportedModules.Contains(torpedoName));
            torpedoModule = ship.TorpedoModules[hullTorpedoName];
        }

        var launcher = torpedoModule.TorpedoLaunchers[0];

        List<(int BarrelCount, int LauncherCount, string LauncherName)> arrangementList = torpedoModule.TorpedoLaunchers
            .GroupBy(torpModule => torpModule.NumBarrels)
            .Select(group => (BarrelCount: group.Key, TorpCount: group.Count(), LauncherName: group.First().Name))
            .OrderBy(item => item.TorpCount)
            .ToList();

        var torpCount = 0;
        StringBuilder arrangementString = new();
        var torpLayout = new string[arrangementList.Count];
        var launcherNames = new List<string>();

        for (var i = 0; i < arrangementList.Count; i++)
        {
            var current = arrangementList[i];
            launcherNames.Add(current.LauncherName);
            arrangementString.AppendLine(CultureInfo.InvariantCulture, $"{current.LauncherCount}x{current.BarrelCount} {{{i}}}");
            torpLayout[i] = $"{current.LauncherCount}x{current.BarrelCount}";
            torpCount += current.LauncherCount * current.BarrelCount;
        }

        decimal traverseSpeed = modifiers.ApplyModifiers("TorpedoArmamentDataContainer.TraverseSpeed", launcher.HorizontalRotationSpeed);

        decimal reloadSpeed = modifiers.ApplyModifiers("TorpedoArmamentDataContainer.Reload", launcher.Reload);

        string torpedoArea = $"{launcher.TorpedoAngles[0]} - {Math.Round(launcher.TorpedoAngles[1], 1)}"; // only the second one needs rounding

        var torpedoes = TorpedoDataContainer.FromTorpedoName(launcher.AmmoList, modifiers, false);

        var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
        nfi.NumberGroupSeparator = "'";

        var fullSalvoDamage = (torpCount * torpedoes[0].Damage).ToString("n0", nfi);
        string torpFullSalvoDmg = default!;
        string altTorpFullSalvoDmg = default!;
        if (torpedoes.Count > 1)
        {
            torpFullSalvoDmg = fullSalvoDamage;
            altTorpFullSalvoDmg = (torpCount * torpedoes[^1].Damage).ToString("n0", nfi);
            fullSalvoDamage = default!;
        }

        var torpedoArmamentDataContainer = new TorpedoArmamentDataContainer
        {
            Name = arrangementString.ToString(),
            LauncherNames = launcherNames,
            TurnTime = Math.Round(180 / traverseSpeed, 1),
            TraverseSpeed = Math.Round(traverseSpeed, 2),
            Reload = Math.Round(reloadSpeed, 2),
            TorpedoArea = torpedoArea,
            Torpedoes = torpedoes,
            TimeToSwitch = Math.Round(reloadSpeed * launcher.AmmoSwitchCoeff, 1),
            TorpedoLaunchers = torpedoModule.TorpedoLaunchers,
            TorpLayout = string.Join(" + ", torpLayout),
            TorpCount = torpCount,
            FullSalvoDamage = fullSalvoDamage,
            TorpFullSalvoDmg = torpFullSalvoDmg,
            AltTorpFullSalvoDmg = altTorpFullSalvoDmg,
        };

        var loadersSum = 0;
        if (torpedoModule.TorpedoLoaders.TryGetValue(TorpedoLauncherLoaderPosition.BowLoaders, out var bowLoaders))
        {
            torpedoArmamentDataContainer.BowLoaders = string.Join(" + ", bowLoaders);
            loadersSum += bowLoaders.Select(x => x.Split('x').Select(int.Parse).First()).Sum();
        }

        if (torpedoModule.TorpedoLoaders.TryGetValue(TorpedoLauncherLoaderPosition.SternLoaders, out var sternLoaders))
        {
            torpedoArmamentDataContainer.SternLoaders = string.Join(" + ", sternLoaders);
            loadersSum += sternLoaders.Select(x => x.Split('x').Select(int.Parse).First()).Sum();
        }

        if (torpedoModule.TorpedoLoaders.TryGetValue(TorpedoLauncherLoaderPosition.LeftSideLoaders, out var leftSideLoaders))
        {
            torpedoArmamentDataContainer.LeftSideLoaders = string.Join(" + ", leftSideLoaders);
            loadersSum += leftSideLoaders.Select(x => x.Split('x').Select(int.Parse).First()).Sum();
        }

        if (torpedoModule.TorpedoLoaders.TryGetValue(TorpedoLauncherLoaderPosition.RightSideLoaders, out var rightSideLoaders))
        {
            torpedoArmamentDataContainer.RightSideLoaders = string.Join(" + ", rightSideLoaders);
            loadersSum += rightSideLoaders.Select(x => x.Split('x').Select(int.Parse).First()).Sum();
        }

        torpedoArmamentDataContainer.LoadersCount = loadersSum;

        torpedoArmamentDataContainer.Torpedoes[^1].IsLast = true;

        torpedoArmamentDataContainer.UpdateDataElements();
        return torpedoArmamentDataContainer;
    }
}
