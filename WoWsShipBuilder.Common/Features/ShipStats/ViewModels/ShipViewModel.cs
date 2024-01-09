using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData.Binding;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Modifiers;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.Features.Builds;
using WoWsShipBuilder.Infrastructure.ApplicationData;
using WoWsShipBuilder.Infrastructure.Utility;

namespace WoWsShipBuilder.Features.ShipStats.ViewModels;

public sealed partial class ShipViewModel : ReactiveObject, IDisposable
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

    public ShipViewModel(ShipViewModelParams viewModelParams, ILogger<ShipViewModel> logger)
    {
        this.tokenSource = new();
        this.logger = logger;
        this.PreviousShip = viewModelParams.ShipSummary.PrevShipIndex is null ? null : AppData.ShipSummaryMapper[viewModelParams.ShipSummary.PrevShipIndex];
        this.CurrentShip = viewModelParams.ShipSummary;
    }

    public void ResetBuild()
    {
        this.logger.LogDebug("Resetting build");
        this.LoadNewShip(AppData.ShipSummaryMapper[this.CurrentShipIndex]);
    }

    public void InitializeData(ShipViewModelParams viewModelParams)
    {
        this.InitializeData(viewModelParams.Ship, viewModelParams.ShipSummary.PrevShipIndex, viewModelParams.ShipSummary.NextShipIndexes, viewModelParams.Build);
    }

    public Build CreateBuild(string buildName)
    {
        return new(buildName, this.CurrentShipIndex, this.RawShipData.ShipNation, this.ShipModuleViewModel.SaveBuild(), this.UpgradePanelViewModel.SaveBuild(), this.ConsumableViewModel.SaveBuild(), this.CaptainSkillSelectorViewModel!.GetCaptainIndex(), this.CaptainSkillSelectorViewModel!.GetSkillNumberList(), this.SignalSelectorViewModel!.GetFlagList());
    }

    private void LoadNewShip(ShipSummary summary)
    {
        this.disposables.Clear();
        var ship = AppData.FindShipFromSummary(summary);
        this.InitializeData(ship, summary.PrevShipIndex, summary.NextShipIndexes);
    }

    private void InitializeData(Ship ship, string? previousIndex, IEnumerable<string>? nextShipsIndexes, Build? build = null)
    {
        this.logger.LogInformation("Loading data for ship {Index}", ship.Index);
        this.logger.LogDebug("Build is null: {BuildIsNull}", build is null);

        // Ship stats model
        this.RawShipData = ship;
        this.EffectiveShipData = this.RawShipData;

        this.logger.LogDebug("Initializing view models");

        // Viewmodel inits
        this.SignalSelectorViewModel = new();
        this.CaptainSkillSelectorViewModel = new(this.RawShipData.ShipClass, CaptainSkillSelectorViewModel.LoadParams(ship.ShipNation));
        this.ShipModuleViewModel = new(this.RawShipData.ShipUpgradeInfo);
        this.UpgradePanelViewModel = new(this.RawShipData, AppData.ModernizationCache);
        this.ConsumableViewModel = ConsumableViewModel.Create(this.RawShipData, new List<string>(), Logging.LoggerFactory);

        this.ShipStatsControlViewModel = new(this.EffectiveShipData);

        if (build != null)
        {
            this.logger.LogDebug("Loading build");
            this.SignalSelectorViewModel.LoadBuild(build.Signals);
            this.CaptainSkillSelectorViewModel.LoadBuild(build.Skills, build.Captain);
            this.ShipModuleViewModel.LoadBuild(build.Modules);
            this.UpgradePanelViewModel.LoadBuild(build.Upgrades);
            this.ConsumableViewModel.LoadBuild(build.Consumables);
        }

        this.CurrentShipIndex = ship.Index;
        this.CurrentShipTier = ship.Tier;
        this.CurrentShip = AppData.ShipSummaryMapper[ship.Index];
        this.PreviousShip = previousIndex is null ? null : AppData.ShipSummaryMapper[previousIndex];
        this.NextShips = nextShipsIndexes?.Select(index => AppData.ShipSummaryMapper[index]).ToList();

        this.AddChangeListeners();
        this.UpdateStatsViewModel(true);
    }

    private void AddChangeListeners()
    {
        this.ShipModuleViewModel.SelectedModules.ToObservableChangeSet().Do(_ => this.UpdateStatsViewModel()).Subscribe().DisposeWith(this.disposables);
        this.UpgradePanelViewModel.SelectedModernizationList.ToObservableChangeSet().Do(_ => this.UpdateStatsViewModel()).Subscribe().DisposeWith(this.disposables);
        this.SignalSelectorViewModel?.SelectedSignals.ToObservableChangeSet().Do(_ => this.UpdateStatsViewModel()).Subscribe().DisposeWith(this.disposables);
        this.CaptainSkillSelectorViewModel?.SkillOrderList.ToObservableChangeSet().Do(_ => this.UpdateStatsViewModel()).Subscribe().DisposeWith(this.disposables);
        this.ConsumableViewModel.ActivatedSlots.ToObservableChangeSet().Do(_ => this.UpdateStatsViewModel()).Subscribe().DisposeWith(this.disposables);
        this.ShipStatsControlViewModel.WhenAnyValue(x => x.IsSpecialAbilityActive).Subscribe(_ => this.UpdateStatsViewModel()).DisposeWith(this.disposables);

        this.CaptainSkillSelectorViewModel.WhenAnyValue(x => x.SkillActivationPopupOpen).Subscribe(this.HandleCaptainParamsChange).DisposeWith(this.disposables);
        this.CaptainSkillSelectorViewModel.WhenAnyValue(x => x.CaptainWithTalents).Subscribe(this.HandleCaptainParamsChange).DisposeWith(this.disposables);
    }

    private void HandleCaptainParamsChange(bool newValue)
    {
        if (!newValue)
        {
            this.UpdateStatsViewModel();
        }
    }

    private void UpdateStatsViewModel(bool skipDelay = false)
    {
        this.tokenSource.Cancel();
        this.tokenSource.Dispose();
        this.tokenSource = new();
        var token = this.tokenSource.Token;
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
                        await this.semaphore.WaitAsync(token);
                        try
                        {
                            List<Modifier> modifiers = this.GenerateModifierList();
                            if (this.ShipStatsControlViewModel != null)
                            {
                                this.logger.LogDebug("Updating ship stats");
                                await this.ShipStatsControlViewModel.UpdateShipStats(this.ShipModuleViewModel.SelectedModules.ToList(), modifiers);
                            }

                            this.ConsumableViewModel.UpdateConsumableData(modifiers, this.ShipStatsControlViewModel!.CurrentShipStats!.SurvivabilityDataContainer.HitPoints, this.RawShipData.ShipClass);
                        }
                        finally
                        {
                            this.semaphore.Release();
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // ignored
                }
            },
            token);
    }

    private List<Modifier> GenerateModifierList()
    {
        var modifiers = new List<Modifier>();

        modifiers.AddRange(this.UpgradePanelViewModel.GetModifierList());
        modifiers.AddRange(this.SignalSelectorViewModel!.GetModifierList());
        modifiers.AddRange(this.CaptainSkillSelectorViewModel!.GetModifiersList());
        modifiers.AddRange(this.ConsumableViewModel.GetModifiersList());
        modifiers.AddRange(this.ShipStatsControlViewModel!.GetSpecialAbilityModifiers());
        return modifiers;
    }

    public void Dispose()
    {
        this.disposables.Dispose();
        this.semaphore.Dispose();
        this.tokenSource.Dispose();
    }
}
