using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Windows.Input;
using DynamicData.Binding;
using ReactiveUI;
using WoWsShipBuilder.Core;
using WoWsShipBuilder.Core.Builds;
using WoWsShipBuilder.Core.Data;
using WoWsShipBuilder.Core.DataContainers;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Localization;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.ViewModels.Base;
using WoWsShipBuilder.ViewModels.Helper;
using WoWsShipBuilder.ViewModels.Other;

namespace WoWsShipBuilder.ViewModels.ShipVm;

public partial class ShipViewModelBase : ViewModelBase
{
    private readonly SemaphoreSlim semaphore = new(1, 1);

    private readonly CompositeDisposable disposables = new();

    private readonly INavigationService navigationService;

    private readonly ILocalizer localizer;

    private CancellationTokenSource tokenSource;

    [Observable]
    private string currentShipIndex = "_default";

    [Observable]
    private int? currentShipTier;

    [Observable]
    private Ship rawShipData = null!;

    [Observable]
    private Ship effectiveShipData = null!;

    [Observable]
    private ShipSummary currentShip = default!;

    [Observable]
    private ShipSummary? previousShip;

    [Observable]
    private List<ShipSummary>? nextShips = new();

    [Observable]
    private CaptainSkillSelectorViewModel? captainSkillSelectorViewModel;

    [Observable]
    private ConsumableViewModel consumableViewModel = null!;

    [Observable]
    private ShipModuleViewModel shipModuleViewModel = null!;

    [Observable]
    private ShipStatsControlViewModel? shipStatsControlViewModel;

    [Observable]
    private SignalSelectorViewModel? signalSelectorViewModel;

    [Observable]
    private UpgradePanelViewModelBase upgradePanelViewModel = null!;

    protected string? CurrentBuildName;

    protected ShipViewModelBase(INavigationService navigationService, ILocalizer localizer, ShipViewModelParams viewModelParams)
    {
        this.navigationService = navigationService;
        this.localizer = localizer;
        tokenSource = new();
        PreviousShip = viewModelParams.ShipSummary.PrevShipIndex is null ? null : AppData.ShipSummaryList!.First(sum => sum.Index == viewModelParams.ShipSummary.PrevShipIndex);
        CurrentShip = viewModelParams.ShipSummary;

        LoadShipFromIndexCommand = ReactiveCommand.CreateFromTask<string>(LoadShipFromIndexExecute);
    }

    public async void ResetBuild()
    {
        Logging.Logger.Info("Resetting build");
        await LoadNewShip(AppData.ShipSummaryList!.First(summary => summary.Index.Equals(CurrentShipIndex)));
    }

    public Interaction<BuildCreationWindowViewModel, BuildCreationResult?> BuildCreationInteraction { get; } = new();

    public Interaction<string, Unit> BuildCreatedInteraction { get; } = new();

    // Handle(true) closes this window too
    public Interaction<Unit, Unit> CloseChildrenInteraction { get; } = new();

    public Interaction<StartMenuViewModelBase, Unit> OpenStartMenuInteraction { get; } = new();

    public ICommand LoadShipFromIndexCommand { get; }

    public void BackToMenu()
    {
        navigationService.OpenStartMenu(true);
    }

    public Interaction<ShipSelectionWindowViewModel, List<ShipSummary>?> SelectNewShipInteraction { get; } = new();

    public async void NewShipSelection()
    {
        Logging.Logger.Info("Selecting new ship");

        var result = (await SelectNewShipInteraction.Handle(new(false, ShipSelectionWindowViewModel.LoadParams(localizer))))?.FirstOrDefault();
        if (result != null)
        {
            Logging.Logger.Info("New ship selected: {0}", result.Index);
            await LoadNewShip(result);
        }
    }

    private async Task LoadShipFromIndexExecute(string shipIndex)
    {
        var shipSummary = AppData.ShipSummaryList!.First(summary => summary.Index == shipIndex);
        await LoadNewShip(shipSummary);
    }

    private async Task LoadNewShip(ShipSummary summary)
    {
        // only close child windows
        await CloseChildrenInteraction.Handle(Unit.Default);

        disposables.Clear();
        var ship = AppData.FindShipFromSummary(summary);

        await InitializeData(ship, summary.PrevShipIndex, summary.NextShipsIndex);
    }

    public async Task InitializeData(ShipViewModelParams viewModelParams)
    {
        await InitializeData(viewModelParams.Ship, viewModelParams.ShipSummary.PrevShipIndex, viewModelParams.ShipSummary.NextShipsIndex, viewModelParams.Build);
    }

    public Build CreateBuild(string buildName)
    {
        return new(buildName, CurrentShipIndex, RawShipData.ShipNation, ShipModuleViewModel.SaveBuild(), UpgradePanelViewModel.SaveBuild(), ConsumableViewModel.SaveBuild(), CaptainSkillSelectorViewModel!.GetCaptainIndex(), CaptainSkillSelectorViewModel!.GetSkillNumberList(), SignalSelectorViewModel!.GetFlagList());
    }

    private async Task InitializeData(Ship ship, string? previousIndex, List<string>? nextShipsIndexes, Build? build = null)
    {
        Logging.Logger.Info("Loading data for ship {0}", ship.Index);
        Logging.Logger.Info("Build is null: {0}", build is null);

        ShipDataContainer.ExpanderStateMapper.Clear();

        // Ship stats model
        RawShipData = ship;
        EffectiveShipData = RawShipData;

        Logging.Logger.Info("Initializing view models");

        // Viewmodel inits
        SignalSelectorViewModel = new();
        CaptainSkillSelectorViewModel = new(RawShipData.ShipClass, CaptainSkillSelectorViewModel.LoadParams(ship.ShipNation));
        ShipModuleViewModel = new(RawShipData.ShipUpgradeInfo);
        UpgradePanelViewModel = new(RawShipData, AppData.ModernizationCache);
        ConsumableViewModel = ConsumableViewModel.Create(RawShipData, new List<string>());

        ShipStatsControlViewModel = new(EffectiveShipData);
        await ShipStatsControlViewModel.UpdateShipStats(ShipModuleViewModel.SelectedModules.ToList(), GenerateModifierList());

        if (build != null)
        {
            Logging.Logger.Info("Loading build");
            SignalSelectorViewModel.LoadBuild(build.Signals);
            CaptainSkillSelectorViewModel.LoadBuild(build.Skills, build.Captain);
            ShipModuleViewModel.LoadBuild(build.Modules);
            UpgradePanelViewModel.LoadBuild(build.Upgrades);
            ConsumableViewModel.LoadBuild(build.Consumables);
        }


        CurrentShipIndex = ship.Index;
        CurrentShipTier = ship.Tier;
        CurrentShip = AppData.ShipSummaryList.First(sum => sum.Index == ship.Index);
        PreviousShip = previousIndex is null ? null : AppData.ShipSummaryList.First(sum => sum.Index == previousIndex);
        NextShips = nextShipsIndexes?.Select(index => AppData.ShipSummaryList.First(sum => sum.Index == index)).ToList();

        AddChangeListeners();
        UpdateStatsViewModel();
        if (build != null)
        {
            CurrentBuildName = build.BuildName;
        }
    }

    private void AddChangeListeners()
    {
        ShipModuleViewModel.SelectedModules.ToObservableChangeSet().Do(_ => UpdateStatsViewModel()).Subscribe().DisposeWith(disposables);
        UpgradePanelViewModel.SelectedModernizationList.ToObservableChangeSet().Do(_ => UpdateStatsViewModel()).Subscribe().DisposeWith(disposables);
        SignalSelectorViewModel?.SelectedSignals.ToObservableChangeSet().Do(_ => UpdateStatsViewModel()).Subscribe().DisposeWith(disposables);
        CaptainSkillSelectorViewModel?.SkillOrderList.ToObservableChangeSet().Do(_ => UpdateStatsViewModel()).Subscribe().DisposeWith(disposables);
        ConsumableViewModel.ActivatedSlots.ToObservableChangeSet().Do(_ => UpdateStatsViewModel()).Subscribe().DisposeWith(disposables);

        CaptainSkillSelectorViewModel.WhenAnyValue(x => x.SkillActivationPopupOpen).Subscribe(HandleCaptainParamsChange).DisposeWith(disposables);
        CaptainSkillSelectorViewModel.WhenAnyValue(x => x.CaptainWithTalents).Subscribe(HandleCaptainParamsChange).DisposeWith(disposables);
    }

    private void HandleCaptainParamsChange(bool newValue)
    {
        if (!newValue)
        {
            UpdateStatsViewModel();
        }
    }

    private void UpdateStatsViewModel()
    {
        tokenSource.Cancel();
        tokenSource.Dispose();
        tokenSource = new();
        CancellationToken token = tokenSource.Token;
        CurrentBuildName = null;
        Task.Run(
            async () =>
            {
                try
                {
                    await Task.Delay(250, token);
                    if (!token.IsCancellationRequested)
                    {
                        await semaphore.WaitAsync(token);
                        var modifiers = GenerateModifierList();
                        if (ShipStatsControlViewModel != null)
                        {
                            Logging.Logger.Info("Updating ship stats");
                            await ShipStatsControlViewModel.UpdateShipStats(ShipModuleViewModel.SelectedModules.ToList(), modifiers);
                        }
                        var hp = ShipStatsControlViewModel!.CurrentShipStats!.SurvivabilityDataContainer.HitPoints;
                        ConsumableViewModel.UpdateConsumableData(modifiers, hp);
                        semaphore.Release();
                    }
                }
                catch (OperationCanceledException)
                {
                    // ignored
                }
            },
            token);
    }

    private List<(string, float)> GenerateModifierList()
    {
        var modifiers = new List<(string, float)>();

        modifiers.AddRange(UpgradePanelViewModel.GetModifierList());
        modifiers.AddRange(SignalSelectorViewModel!.GetModifierList());
        modifiers.AddRange(CaptainSkillSelectorViewModel!.GetModifiersList());
        modifiers.AddRange(ConsumableViewModel.GetModifiersList());
        return modifiers;
    }
}
