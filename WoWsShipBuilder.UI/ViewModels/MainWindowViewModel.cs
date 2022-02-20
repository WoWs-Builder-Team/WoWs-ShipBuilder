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
using WoWsShipBuilder.UI.Translations;

namespace WoWsShipBuilder.UI.ViewModels
{
    // needed for binding to be outside of the class
    public enum Account
    {
        Normal,
        WoWsPremium,
        WGPremium,
    }

    public class MainWindowViewModel : ViewModelBase
    {
        private readonly SemaphoreSlim semaphore = new(1, 1);

        private readonly CompositeDisposable disposables = new();

        private readonly INavigationService navigationService;

        private readonly IScreenshotRenderService screenshotRenderService;

        private readonly IClipboardService clipboardService;

        private readonly IAppDataService appDataService;

        private Account accountType = Account.Normal;

        private string baseXp = "0";

        private CaptainSkillSelectorViewModel? captainSkillSelectorViewModel;

        private string commanderXp = "0";

        private string commanderXpBonus = "0";

        private ConsumableViewModel consumableViewModel = null!;

        private string? currentShipIndex = "_default";

        private int? currentShipTier;

        private Ship effectiveShipData = null!;

        private string freeXp = "0";

        private string freeXpBonus = "0";

        private Dictionary<string, int>? nextShips = new();

        private string? previousShipIndex;

        private int? previousShipTier;

        private Ship rawShipData = null!;

        private ShipModuleViewModel shipModuleViewModel = null!;

        private ShipStatsControlViewModel? shipStatsControlViewModel;

        private SignalSelectorViewModel? signalSelectorViewModel;

        private CancellationTokenSource tokenSource;

        private UpgradePanelViewModel upgradePanelViewModel = null!;

        private string xp = "0";

        private string xpBonus = "0";

        private string? currentBuildName;

        public MainWindowViewModel(INavigationService navigationService, IScreenshotRenderService screenshotRenderService, IClipboardService clipboardService, IAppDataService appDataService, MainViewModelParams viewModelParams)
        {
            this.navigationService = navigationService;
            this.screenshotRenderService = screenshotRenderService;
            this.clipboardService = clipboardService;
            this.appDataService = appDataService;
            tokenSource = new();
            PreviousShipIndex = viewModelParams.ShipSummary.PrevShipIndex;

            LoadShipFromIndexCommand = ReactiveCommand.CreateFromTask<string>(LoadShipFromIndexExecute);

            InitializeData(viewModelParams.Ship, PreviousShipIndex, viewModelParams.ShipSummary.NextShipsIndex, viewModelParams.Build);
        }

        public MainWindowViewModel()
            : this(null!, null!, null!, DesktopAppDataService.PreviewInstance, DataHelper.GetPreviewViewModelParams(ShipClass.Destroyer, 9, Nation.Germany))
        {
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

        public ShipStatsControlViewModel? ShipStatsControlViewModel
        {
            get => shipStatsControlViewModel;
            set => this.RaiseAndSetIfChanged(ref shipStatsControlViewModel, value);
        }

        public CaptainSkillSelectorViewModel? CaptainSkillSelectorViewModel
        {
            get => captainSkillSelectorViewModel;
            set => this.RaiseAndSetIfChanged(ref captainSkillSelectorViewModel, value);
        }

        public string BaseXp
        {
            get => baseXp;
            set
            {
                this.RaiseAndSetIfChanged(ref baseXp, value);
                CalculateXPValues();
            }
        }

        public string XpBonus
        {
            get => xpBonus;
            set
            {
                this.RaiseAndSetIfChanged(ref xpBonus, value);
                CalculateXPValues();
            }
        }

        public string CommanderXpBonus
        {
            get => commanderXpBonus;
            set
            {
                this.RaiseAndSetIfChanged(ref commanderXpBonus, value);
                CalculateXPValues();
            }
        }

        public string FreeXpBonus
        {
            get => freeXpBonus;
            set
            {
                this.RaiseAndSetIfChanged(ref freeXpBonus, value);
                CalculateXPValues();
            }
        }

        public string Xp
        {
            get => xp;
            set => this.RaiseAndSetIfChanged(ref xp, value);
        }

        public string CommanderXp
        {
            get => commanderXp;
            set => this.RaiseAndSetIfChanged(ref commanderXp, value);
        }

        public string FreeXp
        {
            get => freeXp;
            set => this.RaiseAndSetIfChanged(ref freeXp, value);
        }

        public Account AccountType
        {
            get => accountType;
            set
            {
                this.RaiseAndSetIfChanged(ref accountType, value);
                CalculateXPValues();
            }
        }

        public ConsumableViewModel ConsumableViewModel
        {
            get => consumableViewModel;
            set => this.RaiseAndSetIfChanged(ref consumableViewModel, value);
        }

        public UpgradePanelViewModel UpgradePanelViewModel
        {
            get => upgradePanelViewModel;
            set => this.RaiseAndSetIfChanged(ref upgradePanelViewModel, value);
        }

        public Ship EffectiveShipData
        {
            get => effectiveShipData;
            set => this.RaiseAndSetIfChanged(ref effectiveShipData, value);
        }

        private Ship RawShipData
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

        public async void OpenSaveBuild()
        {
            Logging.Logger.Info("Saving build");
            var currentBuild = new Build(CurrentShipIndex!, RawShipData.ShipNation, ShipModuleViewModel.SaveBuild(), UpgradePanelViewModel.SaveBuild(), ConsumableViewModel.SaveBuild(), CaptainSkillSelectorViewModel!.GetCaptainIndex(), CaptainSkillSelectorViewModel!.GetSkillNumberList(), SignalSelectorViewModel!.GetFlagList());
            if (currentBuildName != null)
            {
                currentBuild.BuildName = currentBuildName;
            }

            string shipName = Localizer.Instance[CurrentShipIndex!].Localization;
            var dialogResult = await BuildCreationInteraction.Handle(new(currentBuild, shipName)) ?? BuildCreationResult.Canceled;
            if (!dialogResult.Save)
            {
                return;
            }

            AppData.Settings.IncludeSignalsForImageExport = dialogResult.IncludeSignals;
            currentBuildName = currentBuild.BuildName;
            await CreateBuildImage(currentBuild, dialogResult.IncludeSignals, dialogResult.CopyImageToClipboard);

            string infoBoxContent;
            if (dialogResult.CopyImageToClipboard)
            {
                infoBoxContent = Translation.BuildCreationWindow_SavedImageToClipboard;
            }
            else
            {
                await clipboardService.SetTextAsync(currentBuild.CreateStringFromBuild());
                infoBoxContent = Translation.BuildCreationWindow_SavedClipboard;
            }

            await BuildCreatedInteraction.Handle(infoBoxContent);
        }

        // Handle(true) closes this window too
        public Interaction<Unit, Unit> CloseChildrenInteraction { get; } = new();

        public Interaction<StartMenuViewModel, Unit> OpenStartMenuInteraction { get; } = new();

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

        private async Task CreateBuildImage(Build currentBuild, bool includeSignals, bool copyToClipboard)
        {
            await screenshotRenderService.CreateBuildImageAsync(currentBuild, RawShipData, includeSignals, copyToClipboard);
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
            Console.WriteLine("DATACACHE: " + (AppData.ShipDictionary?.Count ?? -1));
            Logging.Logger.Info("Loading data for ship {0}", ship.Index);
            Logging.Logger.Info("Build is null: {0}", build is null);

            ShipUI.ExpanderStateMapper.Clear();

            // Ship stats model
            RawShipData = ship;
            EffectiveShipData = RawShipData;

            Logging.Logger.Info("Initializing view models");

            // Viewmodel inits
            SignalSelectorViewModel = new SignalSelectorViewModel();
            CaptainSkillSelectorViewModel = new CaptainSkillSelectorViewModel(RawShipData.ShipClass, ship.ShipNation);
            ShipModuleViewModel = new ShipModuleViewModel(RawShipData.ShipUpgradeInfo);
            UpgradePanelViewModel = new UpgradePanelViewModel(RawShipData);
            ConsumableViewModel = new ConsumableViewModel(RawShipData);

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
                currentBuildName = build.BuildName;
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

        private void CalculateXPValues()
        {
            if (!string.IsNullOrEmpty(BaseXp) && !string.IsNullOrEmpty(XpBonus) && !string.IsNullOrEmpty(CommanderXpBonus) && !string.IsNullOrEmpty(FreeXpBonus))
            {
                var baseXp = Convert.ToInt32(BaseXp);
                var xpBonus = Convert.ToDouble(XpBonus);
                var commanderXpBonus = Convert.ToDouble(CommanderXpBonus);
                var freeXpBonus = Convert.ToDouble(FreeXpBonus);

                double accountMultiplier = 1;
                if (AccountType == Account.WGPremium)
                {
                    accountMultiplier = 1.5;
                }
                else if (AccountType == Account.WoWsPremium)
                {
                    accountMultiplier = 1.65;
                }

                // The 1 represent the account type modifier. For now, the math is done only for non premium account
                int finalXp = (int)(baseXp * accountMultiplier * (1 + (xpBonus / 100)));
                int commanderXp = (int)(finalXp + (baseXp * accountMultiplier * (commanderXpBonus / 100)));
                int freeXp = (int)(finalXp * 0.05 * (1 + (freeXpBonus / 100)));

                Xp = Convert.ToString(finalXp);
                CommanderXp = Convert.ToString(commanderXp);
                FreeXp = Convert.ToString(freeXp);
            }
        }

        private void UpdateStatsViewModel()
        {
            tokenSource.Cancel();
            tokenSource.Dispose();
            tokenSource = new();
            CancellationToken token = tokenSource.Token;
            currentBuildName = null;
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
