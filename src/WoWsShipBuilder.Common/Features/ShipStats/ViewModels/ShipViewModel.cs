using System.Collections.Immutable;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
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

    private readonly Ship rawShipData;

    [Observable]
    private ShipSummary currentShip;

    [Observable]
    private string currentShipIndex;

    [Observable]
    private int? currentShipTier;

    [Observable]
    private List<ShipSummary>? nextShips;

    [Observable]
    private ShipSummary? previousShip;

    private CancellationTokenSource tokenSource;

    private ShipViewModel(ShipViewModelParams viewModelParams, ILogger<ShipViewModel> logger)
    {
        var ship = viewModelParams.Ship;
        var build = viewModelParams.Build;

        this.tokenSource = new();
        this.logger = logger;
        this.logger.LogInformation("Loading data for ship {Index}", ship.Index);
        this.logger.LogDebug("Build is null: {BuildIsNull}", build is null);

        // Ship stats model
        this.rawShipData = ship;

        this.logger.LogDebug("Initializing view models");

        // Viewmodel inits
        this.SignalSelectorViewModel = new();
        this.CaptainSkillSelectorViewModel = new(ship.ShipClass, CaptainSkillSelectorViewModel.LoadParams(ship.ShipNation));
        this.ShipModuleViewModel = new(ship.ShipUpgradeInfo);
        this.UpgradePanelViewModel = new(ship, AppData.ModernizationCache);
        this.ConsumableViewModel = ConsumableViewModel.Create(ship, new List<string>(), Logging.LoggerFactory);

        this.ShipStatsControlViewModel = new(ship);

        if (build != null)
        {
            this.logger.LogDebug("Loading build");
            this.SignalSelectorViewModel.LoadBuild(build.Signals);
            this.CaptainSkillSelectorViewModel.LoadBuild(build.Skills, build.Captain);
            this.ShipModuleViewModel.LoadBuild(build.Modules);
            this.UpgradePanelViewModel.LoadBuild(build.Upgrades);
            this.ConsumableViewModel.LoadBuild(build.Consumables);
        }

        var shipSummary = viewModelParams.ShipSummary;
        this.currentShipIndex = ship.Index;
        this.currentShipTier = ship.Tier;
        this.currentShip = AppData.ShipSummaryMapper[ship.Index];
        this.previousShip = shipSummary.PrevShipIndex is null ? null : AppData.ShipSummaryMapper[shipSummary.PrevShipIndex];
        this.nextShips = shipSummary.NextShipIndexes?.Select(index => AppData.ShipSummaryMapper[index]).ToList();
    }

    public ShipModuleViewModel ShipModuleViewModel { get; }

    public UpgradePanelViewModelBase UpgradePanelViewModel { get; }

    public ConsumableViewModel ConsumableViewModel { get; }

    public CaptainSkillSelectorViewModel CaptainSkillSelectorViewModel { get; }

    public SignalSelectorViewModel SignalSelectorViewModel { get; }

    public ShipStatsControlViewModel ShipStatsControlViewModel { get; }

    /// <summary>
    /// Creates a new instance of the <see cref="ShipViewModel"/>, fully initialized and ready to use.
    /// The returned viewmodel is bound to a single ship for its entire lifetime.
    /// To display a different ship, dispose the current instance and create a new one.
    /// </summary>
    /// <param name="viewModelParams">The parameters describing the ship and the optional build to load.</param>
    /// <param name="logger">The logger used by the viewmodel.</param>
    /// <returns>A new instance of the <see cref="ShipViewModel"/> with initialized data.</returns>
    public static ShipViewModel Create(ShipViewModelParams viewModelParams, ILogger<ShipViewModel> logger)
    {
        var vm = new ShipViewModel(viewModelParams, logger);
        vm.AddChangeListeners();
        vm.UpdateStatsViewModel(true);
        return vm;
    }

    public Build CreateBuild(string buildName)
    {
        return new(buildName, this.CurrentShipIndex, this.rawShipData.ShipNation, this.ShipModuleViewModel.SaveBuild(), this.UpgradePanelViewModel.SaveBuild(), this.ConsumableViewModel.SaveBuild(), this.CaptainSkillSelectorViewModel.GetCaptainIndex(), this.CaptainSkillSelectorViewModel.GetSkillNumberList(), this.SignalSelectorViewModel.GetFlagList());
    }

    public void Dispose()
    {
        this.disposables.Dispose();
        this.semaphore.Dispose();
        this.tokenSource.Dispose();
    }

    private void AddChangeListeners()
    {
        this.ShipModuleViewModel.SelectedModules.ToObservableChangeSet().Do(_ => this.UpdateStatsViewModel()).Subscribe().DisposeWith(this.disposables);
        this.UpgradePanelViewModel.SelectedModernizationList.ToObservableChangeSet().Do(_ => this.UpdateStatsViewModel()).Subscribe().DisposeWith(this.disposables);
        this.SignalSelectorViewModel.SelectedSignals.ToObservableChangeSet().Do(_ => this.UpdateStatsViewModel()).Subscribe().DisposeWith(this.disposables);
        this.CaptainSkillSelectorViewModel.SkillOrderList.ToObservableChangeSet().Do(_ => this.UpdateStatsViewModel()).Subscribe().DisposeWith(this.disposables);
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
                            ImmutableList<Modifier> modifiers = this.GenerateModifierList();
                            this.logger.LogDebug("Updating ship stats");
                            await this.ShipStatsControlViewModel.UpdateShipStats(this.ShipModuleViewModel.SelectedModules.ToImmutableList(), modifiers);

                            this.ConsumableViewModel.UpdateConsumableData(modifiers, this.ShipStatsControlViewModel.CurrentShipStats!.SurvivabilityDataContainer.HitPoints, this.rawShipData.ShipClass);
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

    private ImmutableList<Modifier> GenerateModifierList()
    {
        var modifiers = new List<Modifier>();

        modifiers.AddRange(this.UpgradePanelViewModel.GetModifierList());
        modifiers.AddRange(this.SignalSelectorViewModel.GetModifierList());
        modifiers.AddRange(this.CaptainSkillSelectorViewModel.GetModifiersList());
        modifiers.AddRange(this.ConsumableViewModel.GetModifiersList());
        modifiers.AddRange(this.ShipStatsControlViewModel.GetSpecialAbilityModifiers());
        return modifiers.ToImmutableList();
    }
}
