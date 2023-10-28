using ReactiveUI;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.Features.DataContainers;
using WoWsShipBuilder.Features.ShipStats.ViewModels;
using WoWsShipBuilder.Infrastructure.ApplicationData;
using WoWsShipBuilder.Infrastructure.DataTransfer;
using WoWsShipBuilder.Infrastructure.Utility;

namespace WoWsShipBuilder.Features.Builds;

/// <summary>
/// ViewModel for the build configuration dialog.
/// Based on the structure of the main ship viewmodel, this viewmodel is a simplified version that does not contain a viewmodel to automatically calculate ship stats.
/// Instead, it allows to manually calculate ship stats based on current modifications.
/// </summary>
public partial class ShipBuildViewModel : ReactiveObject
{
    [Observable]
    private string buildName = string.Empty;

    [Observable]
    private bool specialAbilityActive;

    private ShipBuildViewModel(Ship ship)
    {
        this.CurrentShip = ship;
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
        bool isCustomBuild = !string.IsNullOrWhiteSpace(this.BuildName) || this.ShipModuleViewModel.SelectedModules.Any(m => !string.IsNullOrEmpty(m.Prev)) || this.UpgradePanelViewModel.SelectedModernizationList.Any() || this.ConsumableViewModel.ActivatedSlots.Any() || this.CaptainSkillSelectorViewModel.SkillOrderList.Any() || this.SignalSelectorViewModel.SelectedSignals.Any();
        if (isCustomBuild)
        {
            return new(this.BuildName.Trim(), this.CurrentShip.Index, this.CurrentShip.ShipNation, this.ShipModuleViewModel.SaveBuild(), this.UpgradePanelViewModel.SaveBuild(), this.ConsumableViewModel.SaveBuild(), this.CaptainSkillSelectorViewModel.GetCaptainIndex(), this.CaptainSkillSelectorViewModel.GetSkillNumberList(), this.SignalSelectorViewModel.GetFlagList());
        }

        return null;
    }

    public ShipBuildContainer CreateShipBuildContainerAsync(ShipBuildContainer baseContainer)
    {
        var build = this.DumpToBuild();
        List<int>? activatedConsumables = this.ConsumableViewModel.ActivatedSlots.Any() ? this.ConsumableViewModel.ActivatedSlots.ToList() : null;
        List<(string, float)> modifiers = this.GenerateModifierList();
        return baseContainer with
        {
            Build = build,
            ActivatedConsumableSlots = activatedConsumables,
            SpecialAbilityActive = this.SpecialAbilityActive,
            ShipDataContainer = this.CreateDataContainerAsync(modifiers),
            Modifiers = modifiers,
        };
    }

    private ShipDataContainer CreateDataContainerAsync(List<(string, float)> modifiers)
    {
        return ShipDataContainer.CreateFromShip(this.CurrentShip, this.ShipModuleViewModel.SelectedModules.ToList(), modifiers);
    }

    private List<(string, float)> GenerateModifierList()
    {
        var modifiers = new List<(string, float)>();

        modifiers.AddRange(this.UpgradePanelViewModel.GetModifierList());
        modifiers.AddRange(this.SignalSelectorViewModel.GetModifierList());
        modifiers.AddRange(this.CaptainSkillSelectorViewModel.GetModifiersList());
        modifiers.AddRange(this.ConsumableViewModel.GetModifiersList());
        return modifiers;
    }
}
