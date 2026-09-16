using System.Collections.Immutable;
using ReactiveUI;
using WoWsShipBuilder.DataStructures.Modifiers;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.Features.DataContainers;
using WoWsShipBuilder.Features.Navigation;
using WoWsShipBuilder.Features.ShipStats;
using WoWsShipBuilder.Features.ShipStats.ViewModels;
using WoWsShipBuilder.Infrastructure.ApplicationData;
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

    private ImmutableArray<IBuildComponent> BuildComponents =>
        [this.ShipModuleViewModel, this.UpgradePanelViewModel, this.SignalSelectorViewModel, this.CaptainSkillSelectorViewModel, this.ConsumableViewModel];

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
        bool isCustomBuild = !string.IsNullOrWhiteSpace(this.BuildName) || this.BuildComponents.Any(component => component.IsModified);
        if (isCustomBuild)
        {
            return new(this.BuildName.Trim(), this.CurrentShip.Index, this.CurrentShip.ShipNation, this.ShipModuleViewModel.SaveBuild(), this.UpgradePanelViewModel.SaveBuild(), this.ConsumableViewModel.SaveBuild(), this.CaptainSkillSelectorViewModel.GetCaptainIndex(), this.CaptainSkillSelectorViewModel.GetSkillNumberList(), this.SignalSelectorViewModel.GetFlagList());
        }

        return null;
    }

    public ShipBuildContainer CreateShipBuildContainer(ShipBuildContainer baseContainer)
    {
        var build = this.DumpToBuild();
        var activatedConsumables = this.ConsumableViewModel.ActivatedSlots.Any() ? this.ConsumableViewModel.ActivatedSlots.ToImmutableArray() : ImmutableArray<int>.Empty;
        ImmutableList<Modifier> modifiers = this.GenerateModifierList();
        return baseContainer with
        {
            Build = build,
            ActivatedConsumableSlots = activatedConsumables,
            SpecialAbilityActive = this.SpecialAbilityActive,
            ShipDataContainer = this.CreateDataContainer(modifiers),
            Modifiers = modifiers,
        };
    }

    private ShipDataContainer CreateDataContainer(ImmutableList<Modifier> modifiers)
    {
        return ShipDataContainer.CreateFromShip(this.CurrentShip, this.ShipModuleViewModel.SelectedModules.ToImmutableList(), modifiers);
    }

    private ImmutableList<Modifier> GenerateModifierList()
    {
        var modifiers = this.BuildComponents.SelectMany(component => component.GetModifiers()).ToList();
        if (!this.SpecialAbilityActive)
        {
            return [.. modifiers];
        }

        if (this.CurrentShip.SpecialAbility != null)
        {
            modifiers.AddRange(this.CurrentShip.SpecialAbility.Modifiers);
            return [.. modifiers];
        }

        // when the special ability is enabled, we want to properly handle its effects
        var burstModeAbility = this.CurrentShip.MainBatteryModuleList.Values.FirstOrDefault(x => x.BurstModeAbility is { AlternateShells.IsEmpty: false })?.BurstModeAbility;
        if (burstModeAbility != null)
        {
            modifiers.AddRange(burstModeAbility.Modifiers);
        }

        return [.. modifiers];
    }
}
