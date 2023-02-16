using FluentAssertions;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.ViewModels.ShipVm;

namespace WoWsShipBuilder.ViewModels.Test.ShipModuleViewModelTests;

[TestFixture]
public class LoadFromBuild
{
    private const string FirstUpgradeName = "PTUA702_TEST_A";

    private const string SecondUpgradeIndex = "PTUA703";

    private const string SecondUpgradeName = $"{SecondUpgradeIndex}_TEST_B";

    [Test]
    public void LoadFromBuild_LoadExportedData_Success()
    {
        var upgradeInfo = CreateBasicUpgradeInfo();
        var vm = new ShipModuleViewModel(upgradeInfo);
        vm.SelectedModules.Single().Name.Should().Be(FirstUpgradeName);
        vm.SelectModuleExecute(upgradeInfo.ShipUpgrades[1]);

        var exportResult = vm.SaveBuild();
        exportResult.Single().Should().Be(SecondUpgradeIndex);

        var vm2 = new ShipModuleViewModel(upgradeInfo);
        vm2.SelectedModules.Single().Name.Should().Be(FirstUpgradeName);
        vm2.LoadBuild(exportResult);
        vm2.SelectedModules.Single().Name.Should().Be(SecondUpgradeName);
    }

    private static UpgradeInfo CreateBasicUpgradeInfo() => new()
    {
        ShipUpgrades = new()
        {
            new()
            {
                Components = new() { { ComponentType.Artillery, new[] { "A_Artillery" } } },
                Name = FirstUpgradeName,
                UcType = ComponentType.Artillery,
            },
            new()
            {
                Components = new() { { ComponentType.Artillery, new[] { "B_Artillery" } } },
                Name = SecondUpgradeName,
                UcType = ComponentType.Artillery,
            },
        },
    };
}
