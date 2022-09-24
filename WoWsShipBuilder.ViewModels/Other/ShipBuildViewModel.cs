using ReactiveUI;
using WoWsShipBuilder.Core.Builds;
using WoWsShipBuilder.Core.Data;
using WoWsShipBuilder.Core.DataContainers;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.Core.Settings;
using WoWsShipBuilder.ViewModels.Base;
using WoWsShipBuilder.ViewModels.ShipVm;

namespace WoWsShipBuilder.ViewModels.Other;

/// <summary>
/// ViewModel for the build configuration dialog.
/// Based on the structure of the main ship viewmodel, this viewmodel is a simplified version that does not contain a viewmodel to automatically calculate ship stats.
/// Instead, it allows to manually calculate ship stats based on current modifications.
/// </summary>
public class ShipBuildViewModel : ViewModelBase
{
    private readonly IAppDataService appDataService;

    private string buildName = string.Empty;

    private bool specialAbilityActive;

    private ShipBuildViewModel(Ship ship, IAppDataService appDataService)
    {
        this.appDataService = appDataService;
        CurrentShip = ship;
    }

    public ShipModuleViewModel ShipModuleViewModel { get; private init; } = default!;

    public UpgradePanelViewModelBase UpgradePanelViewModel { get; private init; } = default!;

    public ConsumableViewModel ConsumableViewModel { get; private init; } = default!;

    public CaptainSkillSelectorViewModel CaptainSkillSelectorViewModel { get; private init; } = default!;

    public SignalSelectorViewModel SignalSelectorViewModel { get; private init; } = default!;

    public Ship CurrentShip { get; }

    public bool SpecialAbilityActive
    {
        get => specialAbilityActive;
        set => this.RaiseAndSetIfChanged(ref specialAbilityActive, value);
    }

    public string BuildName
    {
        get => buildName.Trim();
        set => this.RaiseAndSetIfChanged(ref buildName, value);
    }

    public static async Task<ShipBuildViewModel> CreateAsync(ShipBuildContainer shipBuildContainer, IAppDataService appDataService, AppSettings appSettings)
    {
        var ship = shipBuildContainer.Ship;
        var vm = new ShipBuildViewModel(ship, appDataService)
        {
            SignalSelectorViewModel = new(await SignalSelectorViewModel.LoadSignalList(appDataService, appSettings)),
            CaptainSkillSelectorViewModel = new(ship.ShipClass, await CaptainSkillSelectorViewModel.LoadParamsAsync(appDataService, appSettings, ship.ShipNation)),
            ShipModuleViewModel = new(ship.ShipUpgradeInfo),
            UpgradePanelViewModel = new(ship, AppData.ModernizationCache ?? new Dictionary<string, Modernization>()),
            ConsumableViewModel = await ConsumableViewModel.CreateAsync(appDataService, ship, new List<string>()),
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
            vm.BuildName = build.BuildName;
        }

        return vm;
    }

    public Build? DumpToBuild()
    {
        bool isCustomBuild = !string.IsNullOrWhiteSpace(BuildName) || ShipModuleViewModel.SelectedModules.Any(m => !string.IsNullOrEmpty(m.Prev)) ||
                             UpgradePanelViewModel.SelectedModernizationList.Any() || ConsumableViewModel.ActivatedSlots.Any() ||
                             CaptainSkillSelectorViewModel.SkillOrderList.Any() || SignalSelectorViewModel.SelectedSignals.Any();
        if (isCustomBuild)
        {
            return new(BuildName, CurrentShip.Index, CurrentShip.ShipNation, ShipModuleViewModel.SaveBuild(), UpgradePanelViewModel.SaveBuild(), ConsumableViewModel.SaveBuild(), CaptainSkillSelectorViewModel.GetCaptainIndex(), CaptainSkillSelectorViewModel.GetSkillNumberList(), SignalSelectorViewModel.GetFlagList());
        }

        return null;
    }

    public async Task<ShipBuildContainer> CreateShipBuildContainerAsync(ShipBuildContainer baseContainer)
    {
        var build = DumpToBuild();
        List<int>? activatedConsumables = ConsumableViewModel.ActivatedSlots.Any() ? ConsumableViewModel.ActivatedSlots.ToList() : null;
        List<(string, float)> modifiers = GenerateModifierList();
        return baseContainer with
        {
            Build = build,
            ActivatedConsumableSlots = activatedConsumables,
            SpecialAbilityActive = SpecialAbilityActive,
            ShipDataContainer = await CreateDataContainerAsync(modifiers),
            Modifiers = modifiers,
        };
    }

    private async Task<ShipDataContainer> CreateDataContainerAsync(List<(string, float)> modifiers)
    {
        return await ShipDataContainer.FromShipAsync(CurrentShip, ShipModuleViewModel.SelectedModules.ToList(), modifiers, appDataService);
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
