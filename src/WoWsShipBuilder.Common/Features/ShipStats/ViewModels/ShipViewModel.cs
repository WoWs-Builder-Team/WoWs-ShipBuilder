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
using WoWsShipBuilder.Features.DataContainers;
using WoWsShipBuilder.Features.Navigation;
using WoWsShipBuilder.Infrastructure.ApplicationData;
using WoWsShipBuilder.Infrastructure.Utility;

namespace WoWsShipBuilder.Features.ShipStats.ViewModels;

public sealed partial class ShipViewModel : ReactiveObject, IDisposable
{
    private readonly ImmutableArray<IBuildComponent> buildComponents;

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

    private bool disposed;

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

        this.buildComponents =
        [
            this.ShipModuleViewModel,
            this.UpgradePanelViewModel,
            this.SignalSelectorViewModel,
            this.CaptainSkillSelectorViewModel,
            this.ConsumableViewModel,
        ];

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
    /// Gets a value indicating whether the user changed any part of the build away from its default selection.
    /// </summary>
    public bool IsModified => this.buildComponents.Any(component => component.IsModified);

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

    /// <summary>
    /// Creates a new instance of the <see cref="ShipViewModel"/> for the ship with the specified index.
    /// </summary>
    /// <param name="shipIndex">The index of the ship to load.</param>
    /// <param name="build">The build to load, or <see langword="null"/> to start from the default configuration.</param>
    /// <returns>A new instance of the <see cref="ShipViewModel"/> with initialized data.</returns>
    public static ShipViewModel Create(string shipIndex, Build? build)
    {
        var shipSummary = AppData.ShipSummaryMapper[shipIndex];
        var viewModelParams = new ShipViewModelParams(AppData.FindShipFromSummary(shipSummary), shipSummary, build);
        return Create(viewModelParams, Logging.LoggerFactory.CreateLogger<ShipViewModel>());
    }

    public Build CreateBuild(string buildName)
    {
        return new(buildName, this.CurrentShipIndex, this.rawShipData.ShipNation, this.ShipModuleViewModel.SaveBuild(), this.UpgradePanelViewModel.SaveBuild(), this.ConsumableViewModel.SaveBuild(), this.CaptainSkillSelectorViewModel.GetCaptainIndex(), this.CaptainSkillSelectorViewModel.GetSkillNumberList(), this.SignalSelectorViewModel.GetFlagList());
    }

    /// <summary>
    /// Creates a new <see cref="ShipBuildContainer"/> describing the current state of this viewmodel,
    /// so it can be handed to another page.
    /// </summary>
    /// <param name="containerId">The id identifying the new container.</param>
    /// <param name="buildName">The name of the current build, empty if the user did not name it.</param>
    /// <param name="calculateShipDataContainer">Whether the ship stats should be calculated for the new container.</param>
    /// <returns>A new <see cref="ShipBuildContainer"/> for the current state of this viewmodel.</returns>
    public ShipBuildContainer CreateShipBuildContainer(Guid containerId, string buildName, bool calculateShipDataContainer)
    {
        var modifiers = this.GetModifiers();
        bool isCustomBuild = !string.IsNullOrWhiteSpace(buildName) || this.IsModified;
        var build = isCustomBuild ? this.CreateBuild(buildName) : null;
        var activatedConsumables = this.ConsumableViewModel.ActivatedSlots.Any() ? this.ConsumableViewModel.ActivatedSlots.ToImmutableArray() : ImmutableArray<int>.Empty;
        var shipDataContainer = calculateShipDataContainer ? this.CreateDataContainer(build, modifiers) : null;

        return new(this.rawShipData, build, containerId, activatedConsumables, false, shipDataContainer, modifiers);
    }

    /// <summary>
    /// Refreshes the modifiers and ship stats of an existing <see cref="ShipBuildContainer"/> from the state of this viewmodel,
    /// keeping everything else about the container unchanged.
    /// </summary>
    /// <param name="baseContainer">The container to refresh.</param>
    /// <param name="calculateShipDataContainer">Whether the ship stats should be recalculated.</param>
    /// <returns>The refreshed <see cref="ShipBuildContainer"/>.</returns>
    public ShipBuildContainer UpdateShipBuildContainer(ShipBuildContainer baseContainer, bool calculateShipDataContainer)
    {
        var modifiers = this.GetModifiers();

        return baseContainer with
        {
            Modifiers = modifiers,
            ShipDataContainer = calculateShipDataContainer ? this.CreateDataContainer(baseContainer.Build, modifiers) : baseContainer.ShipDataContainer,
        };
    }

    /// <summary>
    /// Gets the modifiers of all build components.
    /// The special ability is not part of a build and therefore not included, see <see cref="GenerateModifierList"/>
    /// for the list used to calculate the currently displayed ship stats.
    /// </summary>
    /// <returns>The combined modifiers of all build components.</returns>
    public ImmutableList<Modifier> GetModifiers() => [.. this.buildComponents.SelectMany(component => component.GetModifiers())];

    public void Dispose()
    {
        if (this.disposed)
        {
            return;
        }

        this.disposed = true;

        // Stop reacting to changes first, then cancel any update that is already in flight so it does not
        // touch the semaphore or the token source after they have been disposed.
        this.disposables.Dispose();
        this.tokenSource.Cancel();
        this.tokenSource.Dispose();
        this.semaphore.Dispose();
    }

    private ShipDataContainer CreateDataContainer(Build? build, ImmutableList<Modifier> modifiers)
    {
        return build is null
            ? DataContainerUtility.GetStockShipDataContainer(this.rawShipData)
            : DataContainerUtility.GetShipDataContainerFromBuild(this.rawShipData, build.Modules, this.rawShipData.ShipUpgradeInfo.ShipUpgrades, modifiers);
    }

    private void AddChangeListeners()
    {
        this.buildComponents.Select(component => component.Changes).Merge().Subscribe(_ => this.UpdateStatsViewModel()).DisposeWith(this.disposables);
        this.ShipStatsControlViewModel.WhenAnyValue(x => x.IsSpecialAbilityActive).Subscribe(_ => this.UpdateStatsViewModel()).DisposeWith(this.disposables);
    }

    /// <summary>
    /// Gets the modifiers used to calculate the currently displayed ship stats.
    /// Unlike <see cref="GetModifiers"/>, this includes the special ability, which is a display toggle rather than part of the build.
    /// </summary>
    /// <returns>The combined modifiers of all build components and of the special ability.</returns>
    private ImmutableList<Modifier> GenerateModifierList() =>
        [.. this.buildComponents.SelectMany(component => component.GetModifiers()), .. this.ShipStatsControlViewModel.GetSpecialAbilityModifiers()];

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
                catch (Exception e) when (e is OperationCanceledException or ObjectDisposedException)
                {
                    // The viewmodel was disposed while an update was in flight. Nothing left to update.
                }
            },
            token);
    }
}
