using WoWsShipBuilder.Common.DataContainers;
using WoWsShipBuilder.Common.Infrastructure;
using WoWsShipBuilder.Common.Infrastructure.Data;
using WoWsShipBuilder.Common.ShipStats.ViewModels;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.ViewModels.Base;

namespace WoWsShipBuilder.Common.Builds.ViewModels;

/// <summary>
/// ViewModel for the build configuration dialog.
/// Based on the structure of the main ship viewmodel, this viewmodel is a simplified version that does not contain a viewmodel to automatically calculate ship stats.
/// Instead, it allows to manually calculate ship stats based on current modifications.
/// </summary>
public partial class ShipBuildViewModel : ViewModelBase
{
    [Observable]
    private string buildName = string.Empty;

    [Observable]
    private bool specialAbilityActive;

    private ShipBuildViewModel(Ship ship)
    {
        CurrentShip = ship;
    }

    public ShipModuleViewModel ShipModuleViewModel { get; private init; } = default!;

    public UpgradePanelViewModelBase UpgradePanelViewModel { get; private init; } = default!;

    public ConsumableViewModel ConsumableViewModel { get; private init; } = default!;

    public CaptainSkillSelectorViewModel CaptainSkillSelectorViewModel { get; private init; } = default!;

    public SignalSelectorViewModel SignalSelectorViewModel { get; private init; } = default!;

    public Ship CurrentShip { get; }

    public static ShipBuildViewModel Create(ShipBuildContainer shipBuildContainer)
    {
        var ship = shipBuildContainer.Ship;
        var vm = new ShipBuildViewModel(ship)
        {
            SignalSelectorViewModel = new(),
            CaptainSkillSelectorViewModel = new(ship.ShipClass, CaptainSkillSelectorViewModel.LoadParams(ship.ShipNation)),
            ShipModuleViewModel = new(ship.ShipUpgradeInfo),
            UpgradePanelViewModel = new(ship, AppData.ModernizationCache),
            ConsumableViewModel = ConsumableViewModel.Create(ship, new List<string>(), Logging.LoggerFactory),
            SpecialAbilityActive = shipBuildContainer.SpecialAbilityActive,
        };

        var build = shipBuildContainer.Build;
        if (build != null)
        {
            vm.SignalSelectorViewModel.LoadBuild(build.Signals);
            vm.CaptainSkillSelectorViewModel.LoadBuild(build.Skills, build.Captain);
            vm.ShipModuleViewModel.LoadBuild(build.Modules);
            vm.UpgradePanelViewModel.LoadBuild(build.Upgrades);
            vm.ConsumableViewModel.LoadBuild(build.Consumables);
            vm.BuildName = build.BuildName.Trim();
        }

        return vm;
    }

    private Build? DumpToBuild()
    {
        bool isCustomBuild = !string.IsNullOrWhiteSpace(BuildName) || ShipModuleViewModel.SelectedModules.Any(m => !string.IsNullOrEmpty(m.Prev)) ||
                             UpgradePanelViewModel.SelectedModernizationList.Any() || ConsumableViewModel.ActivatedSlots.Any() ||
                             CaptainSkillSelectorViewModel.SkillOrderList.Any() || SignalSelectorViewModel.SelectedSignals.Any();
        if (isCustomBuild)
        {
            return new(BuildName.Trim(), CurrentShip.Index, CurrentShip.ShipNation, ShipModuleViewModel.SaveBuild(), UpgradePanelViewModel.SaveBuild(), ConsumableViewModel.SaveBuild(), CaptainSkillSelectorViewModel.GetCaptainIndex(), CaptainSkillSelectorViewModel.GetSkillNumberList(), SignalSelectorViewModel.GetFlagList());
        }

        return null;
    }

    public ShipBuildContainer CreateShipBuildContainerAsync(ShipBuildContainer baseContainer)
    {
        var build = DumpToBuild();
        List<int>? activatedConsumables = ConsumableViewModel.ActivatedSlots.Any() ? ConsumableViewModel.ActivatedSlots.ToList() : null;
        List<(string, float)> modifiers = GenerateModifierList();
        return baseContainer with
        {
            Build = build,
            ActivatedConsumableSlots = activatedConsumables,
            SpecialAbilityActive = SpecialAbilityActive,
            ShipDataContainer = CreateDataContainerAsync(modifiers),
            Modifiers = modifiers,
        };
    }

    private ShipDataContainer CreateDataContainerAsync(List<(string, float)> modifiers)
    {
        return ShipDataContainer.CreateFromShip(CurrentShip, ShipModuleViewModel.SelectedModules.ToList(), modifiers);
    }

    private List<(string, float)> GenerateModifierList()
    {
        var modifiers = new List<(string, float)>();

        modifiers.AddRange(UpgradePanelViewModel.GetModifierList());
        modifiers.AddRange(SignalSelectorViewModel.GetModifierList());
        modifiers.AddRange(CaptainSkillSelectorViewModel.GetModifiersList());
        modifiers.AddRange(ConsumableViewModel.GetModifiersList());
        return modifiers;
    }
}
