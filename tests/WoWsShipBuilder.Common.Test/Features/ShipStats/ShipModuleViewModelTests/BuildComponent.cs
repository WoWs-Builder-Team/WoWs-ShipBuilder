using System.Collections.Immutable;
using FluentAssertions;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.Features.ShipStats.ViewModels;

namespace WoWsShipBuilder.Test.Features.ShipStats.ShipModuleViewModelTests;

/// <summary>
/// Covers the IBuildComponent implementation, which the owning viewmodels use to aggregate all build components
/// instead of reaching into the selection of each one.
/// </summary>
[TestFixture]
public class BuildComponent
{
    private const string StockHullName = "PTUA702_TEST_STOCK";

    private const string UpgradedHullName = "PTUA703_TEST_UPGRADED";

    [Test]
    public void IsModified_StockModuleSelected_ReturnsFalse()
    {
        var vm = new ShipModuleViewModel(CreateUpgradeInfoWithUpgradePath());

        vm.SelectedModules.Single().Name.Should().Be(StockHullName, "the stock module is the default selection");
        vm.IsModified.Should().BeFalse();
    }

    [Test]
    public void IsModified_UpgradedModuleSelected_ReturnsTrue()
    {
        var upgradeInfo = CreateUpgradeInfoWithUpgradePath();
        var vm = new ShipModuleViewModel(upgradeInfo);

        vm.SelectModuleExecute(upgradeInfo.ShipUpgrades.Single(upgrade => upgrade.Name == UpgradedHullName));

        vm.IsModified.Should().BeTrue();
    }

    [Test]
    public void GetModifiers_UpgradedModuleSelected_ReturnsNoModifiers()
    {
        var upgradeInfo = CreateUpgradeInfoWithUpgradePath();
        var vm = new ShipModuleViewModel(upgradeInfo);

        vm.SelectModuleExecute(upgradeInfo.ShipUpgrades.Single(upgrade => upgrade.Name == UpgradedHullName));

        vm.GetModifiers().Should().BeEmpty("modules select which components are fitted instead of contributing modifiers");
    }

    [Test]
    public void Changes_ModuleSelected_Emits()
    {
        var upgradeInfo = CreateUpgradeInfoWithUpgradePath();
        var vm = new ShipModuleViewModel(upgradeInfo);
        int emissions = 0;
        using var subscription = vm.Changes.Subscribe(_ => emissions++);
        int afterSubscribe = emissions;

        vm.SelectModuleExecute(upgradeInfo.ShipUpgrades.Single(upgrade => upgrade.Name == UpgradedHullName));

        emissions.Should().BeGreaterThan(afterSubscribe);
    }

    private static UpgradeInfo CreateUpgradeInfoWithUpgradePath() => new()
    {
        ShipUpgrades = ImmutableList.Create<ShipUpgrade>(
            new()
            {
                Components = new Dictionary<ComponentType, ImmutableArray<string>> { { ComponentType.Hull, ImmutableArray.Create("A_Hull") } }.ToImmutableDictionary(),
                Name = StockHullName,
                UcType = ComponentType.Hull,
            },
            new()
            {
                Components = new Dictionary<ComponentType, ImmutableArray<string>> { { ComponentType.Hull, ImmutableArray.Create("B_Hull") } }.ToImmutableDictionary(),
                Name = UpgradedHullName,
                Prev = StockHullName,
                UcType = ComponentType.Hull,
            }),
    };
}
