﻿using WoWsShipBuilder.Core.BuildCreator;
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

    public ShipModuleViewModel ShipModuleViewModel { get; private init; } = default!;

    public UpgradePanelViewModelBase UpgradePanelViewModel { get; private init; } = default!;

    public ConsumableViewModel ConsumableViewModel { get; private init; } = default!;

    public CaptainSkillSelectorViewModel CaptainSkillSelectorViewModel { get; private init; } = default!;

    public SignalSelectorViewModel SignalSelectorViewModel { get; private init; } = default!;

    public Ship CurrentShip { get; }

    private ShipBuildViewModel(Ship ship, IAppDataService appDataService)
    {
        this.appDataService = appDataService;
        CurrentShip = ship;
    }

    public static async Task<ShipBuildViewModel> CreateAsync(Ship ship, Build? build, IAppDataService appDataService, AppSettings appSettings)
    {
        var vm = new ShipBuildViewModel(ship, appDataService)
        {
            SignalSelectorViewModel = new(await SignalSelectorViewModel.LoadSignalList(appDataService, appSettings)),
            CaptainSkillSelectorViewModel = new(ship.ShipClass, await CaptainSkillSelectorViewModel.LoadParamsAsync(appDataService, appSettings, ship.ShipNation)),
            ShipModuleViewModel = new(ship.ShipUpgradeInfo),
            UpgradePanelViewModel = new(ship, AppData.ModernizationCache ?? new Dictionary<string, Modernization>()),
            ConsumableViewModel = await ConsumableViewModel.CreateAsync(appDataService, ship, new List<string>()),
        };

        if (build != null)
        {
            vm.SignalSelectorViewModel.LoadBuild(build.Signals);
            vm.CaptainSkillSelectorViewModel.LoadBuild(build.Skills, build.Captain);
            vm.ShipModuleViewModel.LoadBuild(build.Modules);
            vm.UpgradePanelViewModel.LoadBuild(build.Upgrades);
            vm.ConsumableViewModel.LoadBuild(build.Consumables);
        }

        return vm;
    }

    public Build DumpToBuild(string? buildName)
    {
        return new(CurrentShip.Index, CurrentShip.ShipNation, ShipModuleViewModel.SaveBuild(), UpgradePanelViewModel.SaveBuild(), ConsumableViewModel.SaveBuild(), CaptainSkillSelectorViewModel.GetCaptainIndex(), CaptainSkillSelectorViewModel.GetSkillNumberList(), SignalSelectorViewModel.GetFlagList())
        {
            BuildName = buildName ?? string.Empty,
        };
    }

    public async Task<ShipDataContainer> CreateDataContainerAsync()
    {
        return await ShipDataContainer.FromShipAsync(CurrentShip, ShipModuleViewModel.SelectedModules.ToList(), GenerateModifierList(), appDataService);
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
