using System.Collections.Immutable;
using FluentAssertions;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.Features.BallisticCharts;

namespace WoWsShipBuilder.Test.Features.BallisticCharts.CompatibleModuleSelectorTests;

/// <summary>
/// Selecting a shell in the ballistic charts forces the configuration onto a hull and artillery that can fire it.
/// The modules already selected have to survive that whenever they are compatible, because the resulting
/// configuration is written back into the build the user then edits.
/// </summary>
[TestFixture]
public class FindCompatibleModules
{
    private const string StockHullComponent = "A_Hull";

    private const string UpgradedHullComponent = "B_Hull";

    private const string StockArtilleryComponent = "A_Artillery";

    private const string UpgradedArtilleryComponent = "B_Artillery";

    private const string ShellName = "PTSA001";

    private const string OtherShellName = "PTSA002";

    [Test]
    public void FindCompatibleModules_UpgradedHullSelectedAndCompatible_KeepsUpgradedHull()
    {
        var ship = CreateShip(ShellCompatibility(ShellName, (StockHullComponent, StockArtilleryComponent), (UpgradedHullComponent, UpgradedArtilleryComponent)));

        var result = CompatibleModuleSelector.FindCompatibleModules(ship, [ShellName], Configuration(UpgradedHullComponent, UpgradedArtilleryComponent));

        result.Should().Be((UpgradedHullComponent, UpgradedArtilleryComponent));
    }

    [Test]
    public void FindCompatibleModules_StockHullSelectedAndCompatible_KeepsStockHull()
    {
        var ship = CreateShip(ShellCompatibility(ShellName, (StockHullComponent, StockArtilleryComponent), (UpgradedHullComponent, UpgradedArtilleryComponent)));

        var result = CompatibleModuleSelector.FindCompatibleModules(ship, [ShellName], Configuration(StockHullComponent, StockArtilleryComponent));

        result.Should().Be((StockHullComponent, StockArtilleryComponent), "a compatible selection must not be replaced");
    }

    [Test]
    public void FindCompatibleModules_SelectedHullIncompatible_PicksMostUpgradedCompatibleHull()
    {
        var ship = CreateShip(ShellCompatibility(ShellName, (UpgradedHullComponent, UpgradedArtilleryComponent)));

        var result = CompatibleModuleSelector.FindCompatibleModules(ship, [ShellName], Configuration(StockHullComponent, StockArtilleryComponent));

        result.Should().Be((UpgradedHullComponent, UpgradedArtilleryComponent));
    }

    [Test]
    public void FindCompatibleModules_SeveralCompatibleHullsAndNoneSelected_PicksMostUpgradedHull()
    {
        var ship = CreateShip(ShellCompatibility(ShellName, (StockHullComponent, StockArtilleryComponent), (UpgradedHullComponent, UpgradedArtilleryComponent)));

        var result = CompatibleModuleSelector.FindCompatibleModules(ship, [ShellName], new List<ShipUpgrade>());

        result.Should().Be((UpgradedHullComponent, UpgradedArtilleryComponent));
    }

    /// <summary>
    /// Regression guard. The compatible combinations are stored in an ImmutableDictionary, whose enumeration order
    /// depends on string hash codes and therefore changes between processes. Picking the first key made the resolved
    /// hull differ from run to run.
    /// </summary>
    [Test]
    public void FindCompatibleModules_CompatibleCombinationsInAnyOrder_ReturnsTheSameResult()
    {
        var forward = CreateShip(ShellCompatibility(ShellName, (StockHullComponent, StockArtilleryComponent), (UpgradedHullComponent, UpgradedArtilleryComponent)));
        var reversed = CreateShip(ShellCompatibility(ShellName, (UpgradedHullComponent, UpgradedArtilleryComponent), (StockHullComponent, StockArtilleryComponent)));

        var fromForward = CompatibleModuleSelector.FindCompatibleModules(forward, [ShellName], Configuration(StockHullComponent, StockArtilleryComponent));
        var fromReversed = CompatibleModuleSelector.FindCompatibleModules(reversed, [ShellName], Configuration(StockHullComponent, StockArtilleryComponent));

        fromForward.Should().Be(fromReversed);
    }

    [Test]
    public void FindCompatibleModules_SeveralShells_PicksCombinationSupportingAllOfThem()
    {
        var ship = CreateShip(
            ShellCompatibility(ShellName, (StockHullComponent, StockArtilleryComponent), (UpgradedHullComponent, UpgradedArtilleryComponent)),
            ShellCompatibility(OtherShellName, (StockHullComponent, StockArtilleryComponent)));

        var result = CompatibleModuleSelector.FindCompatibleModules(ship, [ShellName, OtherShellName], Configuration(UpgradedHullComponent, UpgradedArtilleryComponent));

        result.Should().Be((StockHullComponent, StockArtilleryComponent), "only the stock combination can fire both shells");
    }

    [Test]
    public void FindCompatibleModules_NoCombinationSupportsAllShells_ReturnsNull()
    {
        var ship = CreateShip(
            ShellCompatibility(ShellName, (UpgradedHullComponent, UpgradedArtilleryComponent)),
            ShellCompatibility(OtherShellName, (StockHullComponent, StockArtilleryComponent)));

        var result = CompatibleModuleSelector.FindCompatibleModules(ship, [ShellName, OtherShellName], Configuration(StockHullComponent, StockArtilleryComponent));

        result.Should().BeNull();
    }

    private static List<ShipUpgrade> Configuration(string hullComponent, string artilleryComponent) =>
    [
        Upgrade(ComponentType.Hull, hullComponent),
        Upgrade(ComponentType.Artillery, artilleryComponent),
    ];

    private static ShipUpgrade Upgrade(ComponentType componentType, string componentName, string previous = "") => new()
    {
        Name = $"{componentName}_MODULE",
        Prev = previous,
        UcType = componentType,
        Components = new Dictionary<ComponentType, ImmutableArray<string>> { { componentType, [componentName] } }.ToImmutableDictionary(),
    };

    private static KeyValuePair<string, ShellCompatibility> ShellCompatibility(string shellName, params (string Hull, string Artillery)[] combos) =>
        new(shellName, new(shellName, combos.ToImmutableDictionary(combo => combo.Hull, combo => ImmutableList.Create(combo.Artillery)), false));

    private static Ship CreateShip(params KeyValuePair<string, ShellCompatibility>[] shellCompatibilities) => new()
    {
        Index = "PTSA001",
        Id = 1,
        ShellCompatibilities = shellCompatibilities.ToImmutableDictionary(),
        ShipUpgradeInfo = new()
        {
            ShipUpgrades = ImmutableList.Create(
                Upgrade(ComponentType.Hull, StockHullComponent),
                Upgrade(ComponentType.Hull, UpgradedHullComponent, $"{StockHullComponent}_MODULE"),
                Upgrade(ComponentType.Artillery, StockArtilleryComponent),
                Upgrade(ComponentType.Artillery, UpgradedArtilleryComponent, $"{StockArtilleryComponent}_MODULE")),
        },
    };
}
