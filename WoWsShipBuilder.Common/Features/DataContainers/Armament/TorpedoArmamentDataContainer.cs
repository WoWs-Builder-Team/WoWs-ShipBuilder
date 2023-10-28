using System.Globalization;
using System.Text;
using WoWsShipBuilder.DataElements;
using WoWsShipBuilder.DataElements.DataElementAttributes;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.Infrastructure.Utility;

namespace WoWsShipBuilder.Features.DataContainers;

[DataContainer]
public partial record TorpedoArmamentDataContainer : DataContainerBase
{
    [DataElementType(DataElementTypes.FormattedText, ArgumentsCollectionName = "LauncherNames", ArgumentsTextKind = TextKind.LocalizationKey)]
    public string Name { get; set; } = default!;

    public List<string> LauncherNames { get; set; } = new();

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValue, GroupKey = "Loaders")]
    public string BowLoaders { get; set; } = default!;

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValue, GroupKey = "Loaders")]
    public string SternLoaders { get; set; } = default!;

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

    public static TorpedoArmamentDataContainer? FromShip(Ship ship, List<ShipUpgrade> shipConfiguration, List<(string name, float value)> modifiers)
    {
        var torpConfiguration = shipConfiguration.FirstOrDefault(c => c.UcType == ComponentType.Torpedoes);
        if (torpConfiguration == null)
        {
            return null;
        }

        string[] torpedoOptions = torpConfiguration.Components[ComponentType.Torpedoes];
        string[] supportedModules = torpConfiguration.Components[ComponentType.Torpedoes];

        TorpedoModule? torpedoModule;
        if (torpedoOptions.Length == 1)
        {
            torpedoModule = ship.TorpedoModules[supportedModules.First()];
        }
        else
        {
            string hullTorpedoName = shipConfiguration.First(c => c.UcType == ComponentType.Hull).Components[ComponentType.Torpedoes].First(torpedoName => supportedModules.Contains(torpedoName));
            torpedoModule = ship.TorpedoModules[hullTorpedoName];
        }

        var launcher = torpedoModule.TorpedoLaunchers.First();

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

        var turnSpeedModifiers = modifiers.FindModifiers("GTRotationSpeed");
        decimal traverseSpeed = turnSpeedModifiers.Aggregate(launcher.HorizontalRotationSpeed, (current, modifier) => current * (decimal)modifier);

        var reloadSpeedModifiers = modifiers.FindModifiers("GTShotDelay");
        decimal reloadSpeed = reloadSpeedModifiers.Aggregate(launcher.Reload, (current, modifier) => current * (decimal)modifier);

        var arModifiers = modifiers.FindModifiers("lastChanceReloadCoefficient");
        reloadSpeed = arModifiers.Aggregate(reloadSpeed, (current, arModifier) => current * (1 - ((decimal)arModifier / 100)));

        var talentModifiers = modifiers.FindModifiers("torpedoReloadCoeff");
        reloadSpeed = talentModifiers.Aggregate(reloadSpeed, (current, modifier) => current * (decimal)modifier);

        string torpedoArea = $"{launcher.TorpedoAngles[0]} - {Math.Round(launcher.TorpedoAngles[1], 1)}"; // only the second one needs rounding

        var torpedoes = TorpedoDataContainer.FromTorpedoName(launcher.AmmoList, modifiers, false);

        var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
        nfi.NumberGroupSeparator = "'";

        var fullSalvoDamage = (torpCount * torpedoes.First().Damage).ToString("n0", nfi);
        string torpFullSalvoDmg = default!;
        string altTorpFullSalvoDmg = default!;
        if (torpedoes.Count > 1)
        {
            torpFullSalvoDmg = fullSalvoDamage;
            altTorpFullSalvoDmg = (torpCount * torpedoes.Last().Damage).ToString("n0", nfi);
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
            TimeToSwitch = Math.Round(torpedoModule.TimeToChangeAmmo,  1),
            TorpedoLaunchers = torpedoModule.TorpedoLaunchers,
            TorpLayout = string.Join(" + ", torpLayout),
            TorpCount = torpCount,
            FullSalvoDamage = fullSalvoDamage,
            TorpFullSalvoDmg = torpFullSalvoDmg,
            AltTorpFullSalvoDmg = altTorpFullSalvoDmg,
        };

        torpedoModule.TorpedoLoaders.TryGetValue(SubTorpLauncherLoaderPosition.BowLoaders, out List<string>? bowLoaders);
        torpedoModule.TorpedoLoaders.TryGetValue(SubTorpLauncherLoaderPosition.SternLoaders, out List<string>? sternLoaders);

        var loadersSum = 0;
        if (bowLoaders is not null)
        {
            torpedoArmamentDataContainer.BowLoaders = string.Join(" + ", bowLoaders);
            loadersSum += bowLoaders.Select(x => x.Split('x').Select(int.Parse).First()).Sum();
        }

        if (sternLoaders is not null)
        {
            torpedoArmamentDataContainer.SternLoaders = string.Join(" + ", sternLoaders);
            loadersSum += sternLoaders.Select(x => x.Split('x').Select(int.Parse).First()).Sum();
        }

        torpedoArmamentDataContainer.LoadersCount = loadersSum;

        torpedoArmamentDataContainer.Torpedoes.Last().IsLast = true;

        torpedoArmamentDataContainer.UpdateDataElements();
        return torpedoArmamentDataContainer;
    }
}
