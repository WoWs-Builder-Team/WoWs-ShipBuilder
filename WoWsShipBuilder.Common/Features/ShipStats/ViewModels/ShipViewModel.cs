using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData.Binding;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.Features.Builds;
using WoWsShipBuilder.Infrastructure.ApplicationData;
using WoWsShipBuilder.Infrastructure.Utility;

namespace WoWsShipBuilder.Features.ShipStats.ViewModels;

public partial class ShipViewModel : ReactiveObject
{
    private readonly CompositeDisposable disposables = new();

    private readonly ILogger<ShipViewModel> logger;

    private readonly SemaphoreSlim semaphore = new(1, 1);

    [Observable]
    private CaptainSkillSelectorViewModel? captainSkillSelectorViewModel;

    [Observable]
    private ConsumableViewModel consumableViewModel = null!;

    [Observable]
    private ShipSummary currentShip = default!;

    [Observable]
    private string currentShipIndex = "_default";

    [Observable]
    private int? currentShipTier;

    [Observable]
    private Ship effectiveShipData = null!;

    [Observable]
    private List<ShipSummary>? nextShips = new();

    [Observable]
    private ShipSummary? previousShip;

    [Observable]
    private Ship rawShipData = null!;

    [Observable]
    private ShipModuleViewModel shipModuleViewModel = null!;

    [Observable]
    private ShipStatsControlViewModel? shipStatsControlViewModel;

    [Observable]
    private SignalSelectorViewModel? signalSelectorViewModel;

    private CancellationTokenSource tokenSource;

    [Observable]
    private UpgradePanelViewModelBase upgradePanelViewModel = null!;

    public ShipViewModel(ILogger<ShipViewModel> logger, ShipViewModelParams viewModelParams)
    {
        tokenSource = new();
        this.logger = logger;
        PreviousShip = viewModelParams.ShipSummary.PrevShipIndex is null ? null : AppData.ShipSummaryMapper[viewModelParams.ShipSummary.PrevShipIndex];
        CurrentShip = viewModelParams.ShipSummary;
    }

    public void ResetBuild()
    {
        logger.LogDebug("Resetting build");
        LoadNewShip(AppData.ShipSummaryMapper[CurrentShipIndex]);
    }

    public void InitializeData(ShipViewModelParams viewModelParams)
    {
        InitializeData(viewModelParams.Ship, viewModelParams.ShipSummary.PrevShipIndex, viewModelParams.ShipSummary.NextShipsIndex, viewModelParams.Build);
    }

    public Build CreateBuild(string buildName)
    {
        return new(buildName, CurrentShipIndex, RawShipData.ShipNation, ShipModuleViewModel.SaveBuild(), UpgradePanelViewModel.SaveBuild(), ConsumableViewModel.SaveBuild(), CaptainSkillSelectorViewModel!.GetCaptainIndex(), CaptainSkillSelectorViewModel!.GetSkillNumberList(), SignalSelectorViewModel!.GetFlagList());
    }

    private void LoadNewShip(ShipSummary summary)
    {
        disposables.Clear();
        var ship = AppData.FindShipFromSummary(summary);
        InitializeData(ship, summary.PrevShipIndex, summary.NextShipsIndex);
    }

    private void InitializeData(Ship ship, string? previousIndex, List<string>? nextShipsIndexes, Build? build = null)
    {
        logger.LogInformation("Loading data for ship {Index}", ship.Index);
        logger.LogDebug("Build is null: {BuildIsNull}", build is null);

        // Ship stats model
        RawShipData = ship;
        EffectiveShipData = RawShipData;

        logger.LogDebug("Initializing view models");

        // Viewmodel inits
        SignalSelectorViewModel = new();
        CaptainSkillSelectorViewModel = new(RawShipData.ShipClass, CaptainSkillSelectorViewModel.LoadParams(ship.ShipNation));
        ShipModuleViewModel = new(RawShipData.ShipUpgradeInfo);
        UpgradePanelViewModel = new(RawShipData, AppData.ModernizationCache);
        ConsumableViewModel = ConsumableViewModel.Create(RawShipData, new List<string>(), Logging.LoggerFactory);

        ShipStatsControlViewModel = new(EffectiveShipData);

        if (build != null)
        {
            logger.LogDebug("Loading build");
            SignalSelectorViewModel.LoadBuild(build.Signals);
            CaptainSkillSelectorViewModel.LoadBuild(build.Skills, build.Captain);
            ShipModuleViewModel.LoadBuild(build.Modules);
            UpgradePanelViewModel.LoadBuild(build.Upgrades);
            ConsumableViewModel.LoadBuild(build.Consumables);
        }

        CurrentShipIndex = ship.Index;
        CurrentShipTier = ship.Tier;
        CurrentShip = AppData.ShipSummaryMapper[ship.Index];
        PreviousShip = previousIndex is null ? null : AppData.ShipSummaryMapper[previousIndex];
        NextShips = nextShipsIndexes?.Select(index => AppData.ShipSummaryMapper[index]).ToList();

        AddChangeListeners();
        UpdateStatsViewModel(true);
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

    private void UpdateStatsViewModel(bool skipDelay = false)
    {
        tokenSource.Cancel();
        tokenSource.Dispose();
        tokenSource = new();
        CancellationToken token = tokenSource.Token;
        Task.Run(
            async () =>
            {
                try
                {
                    if (!skipDelay)
                    {
                        await Task.Delay(250, token);
                    }

                    if (!token.IsCancellationRequested)
                    {
                        await semaphore.WaitAsync(token);
                        var modifiers = GenerateModifierList();
                        if (ShipStatsControlViewModel != null)
                        {
                            logger.LogDebug("Updating ship stats");
                            await ShipStatsControlViewModel.UpdateShipStats(ShipModuleViewModel.SelectedModules.ToList(), modifiers);
                        }

                        ConsumableViewModel.UpdateConsumableData(modifiers, ShipStatsControlViewModel!.CurrentShipStats!.SurvivabilityDataContainer.HitPoints, RawShipData.ShipClass);
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
