using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using DynamicData.Binding;
using ReactiveUI;
using WoWsShipBuilder.Core;
using WoWsShipBuilder.Core.BuildCreator;
using WoWsShipBuilder.Core.Data;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.DataUI;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.ViewModels.Base;
using WoWsShipBuilder.ViewModels.Helper;
using WoWsShipBuilder.ViewModels.Other;

namespace WoWsShipBuilder.ViewModels.ShipVm
{
    public abstract class MainWindowViewModelBase : ViewModelBase
    {
        private readonly SemaphoreSlim semaphore = new(1, 1);

        private readonly CompositeDisposable disposables = new();

        private readonly INavigationService navigationService;

        private readonly IAppDataService appDataService;

        private CaptainSkillSelectorViewModel? captainSkillSelectorViewModel;

        private ConsumableViewModel consumableViewModel = null!;

        private string? currentShipIndex = "_default";

        private int? currentShipTier;

        private Ship effectiveShipData = null!;

        private Dictionary<string, int>? nextShips = new();

        private string? previousShipIndex;

        private int? previousShipTier;

        private Ship rawShipData = null!;

        private ShipModuleViewModel shipModuleViewModel = null!;

        private ShipStatsControlViewModelBase? shipStatsControlViewModel;

        private SignalSelectorViewModel? signalSelectorViewModel;

        private CancellationTokenSource tokenSource;

        private UpgradePanelViewModelBase upgradePanelViewModel = null!;

        protected string? CurrentBuildName;

        protected MainWindowViewModelBase(INavigationService navigationService, IAppDataService appDataService, MainViewModelParams viewModelParams)
        {
            this.navigationService = navigationService;
            this.appDataService = appDataService;
            tokenSource = new();
            PreviousShipIndex = viewModelParams.ShipSummary.PrevShipIndex;

            LoadShipFromIndexCommand = ReactiveCommand.CreateFromTask<string>(LoadShipFromIndexExecute);

            InitializeData(viewModelParams.Ship, PreviousShipIndex, viewModelParams.ShipSummary.NextShipsIndex, viewModelParams.Build);
        }

        public string? CurrentShipIndex
        {
            get => currentShipIndex;
            set => this.RaiseAndSetIfChanged(ref currentShipIndex, value);
        }

        public int? CurrentShipTier
        {
            get => currentShipTier;
            set => this.RaiseAndSetIfChanged(ref currentShipTier, value);
        }

        public string? PreviousShipIndex
        {
            get => previousShipIndex;
            set => this.RaiseAndSetIfChanged(ref previousShipIndex, value);
        }

        public int? PreviousShipTier
        {
            get => previousShipTier;
            set => this.RaiseAndSetIfChanged(ref previousShipTier, value);
        }

        public Dictionary<string, int>? NextShips
        {
            get => nextShips;
            set => this.RaiseAndSetIfChanged(ref nextShips, value);
        }

        public ShipModuleViewModel ShipModuleViewModel
        {
            get => shipModuleViewModel;
            set => this.RaiseAndSetIfChanged(ref shipModuleViewModel, value);
        }

        public SignalSelectorViewModel? SignalSelectorViewModel
        {
            get => signalSelectorViewModel;
            set => this.RaiseAndSetIfChanged(ref signalSelectorViewModel, value);
        }

        public ShipStatsControlViewModelBase? ShipStatsControlViewModel
        {
            get => shipStatsControlViewModel;
            set => this.RaiseAndSetIfChanged(ref shipStatsControlViewModel, value);
        }

        public CaptainSkillSelectorViewModel? CaptainSkillSelectorViewModel
        {
            get => captainSkillSelectorViewModel;
            set => this.RaiseAndSetIfChanged(ref captainSkillSelectorViewModel, value);
        }

        public ConsumableViewModel ConsumableViewModel
        {
            get => consumableViewModel;
            set => this.RaiseAndSetIfChanged(ref consumableViewModel, value);
        }

        public UpgradePanelViewModelBase UpgradePanelViewModel
        {
            get => upgradePanelViewModel;
            set => this.RaiseAndSetIfChanged(ref upgradePanelViewModel, value);
        }

        public Ship EffectiveShipData
        {
            get => effectiveShipData;
            set => this.RaiseAndSetIfChanged(ref effectiveShipData, value);
        }

        protected Ship RawShipData
        {
            get => rawShipData;
            set => this.RaiseAndSetIfChanged(ref rawShipData, value);
        }

        public async void ResetBuild()
        {
            Logging.Logger.Info("Resetting build");
            await LoadNewShip(AppData.ShipSummaryList!.First(summary => summary.Index.Equals(CurrentShipIndex)));
        }

        public Interaction<BuildCreationWindowViewModel, BuildCreationResult?> BuildCreationInteraction { get; } = new();

        public Interaction<string, Unit> BuildCreatedInteraction { get; } = new();

        public abstract void OpenSaveBuild();

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

            var result = (await SelectNewShipInteraction.Handle(new(false)))?.FirstOrDefault();
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
            var ship = appDataService.GetShipFromSummary(summary);
            appDataService.LoadNationFiles(summary.Nation);

            InitializeData(ship!, summary.PrevShipIndex, summary.NextShipsIndex);
        }

        private void InitializeData(Ship ship, string? previousIndex, List<string>? nextShipsIndexes, Build? build = null)
        {
            Logging.Logger.Info("Loading data for ship {0}", ship.Index);
            Logging.Logger.Info("Build is null: {0}", build is null);

            ShipUI.ExpanderStateMapper.Clear();

            // Ship stats model
            RawShipData = ship;
            EffectiveShipData = RawShipData;

            Logging.Logger.Info("Initializing view models");

            // Viewmodel inits
            SignalSelectorViewModel = new();
            CaptainSkillSelectorViewModel = new(RawShipData.ShipClass, ship.ShipNation);
            ShipModuleViewModel = new(RawShipData.ShipUpgradeInfo);
            UpgradePanelViewModel = new(RawShipData);
            ConsumableViewModel = new(RawShipData);

            if (build != null)
            {
                Logging.Logger.Info("Loading build");
                SignalSelectorViewModel.LoadBuild(build.Signals);
                CaptainSkillSelectorViewModel.LoadBuild(build.Skills, build.Captain);
                ShipModuleViewModel.LoadBuild(build.Modules);
                UpgradePanelViewModel.LoadBuild(build.Upgrades);
                ConsumableViewModel.LoadBuild(build.Consumables);
            }

            ShipStatsControlViewModel = new(EffectiveShipData, ShipModuleViewModel.SelectedModules.ToList(), GenerateModifierList(), appDataService);

            CurrentShipIndex = ship.Index;
            CurrentShipTier = ship.Tier;
            PreviousShipIndex = previousIndex;
            if (previousIndex != null)
            {
                PreviousShipTier = AppData.ShipDictionary![previousIndex].Tier;
            }

            NextShips = nextShipsIndexes?.ToDictionary(x => x, x => AppData.ShipDictionary![x].Tier);
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

            CaptainSkillSelectorViewModel.WhenAnyValue(x => x.SkillActivationPopupOpen).Subscribe(HandleCaptainParamsChange).DisposeWith(disposables);
            CaptainSkillSelectorViewModel.WhenAnyValue(x => x.CaptainWithTalents).Subscribe(HandleCaptainParamsChange).DisposeWith(disposables);
            CaptainSkillSelectorViewModel.WhenAnyValue(x => x.CamoEnabled).Subscribe(_ => UpdateStatsViewModel()).DisposeWith(disposables);
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

                            ConsumableViewModel.UpdateShipConsumables(modifiers);
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
            return modifiers;
        }
    }
}