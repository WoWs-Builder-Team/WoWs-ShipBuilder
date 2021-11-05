using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Collections;
using Avalonia.Controls;
using ReactiveUI;
using WoWsShipBuilder.Core.BuildCreator;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.UI.Views;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.UI.ViewModels
{
    class MainWindowViewModel : ViewModelBase
    {
        // ReSharper disable once CollectionNeverQueried.Local
        private readonly List<IDisposable?> collectionChangeListeners = new();

        private readonly MainWindow? self;

        private readonly SemaphoreSlim semaphore = new(1, 1);

        private bool? accountState = false;

        private string accountType = "Normal Account";

        private string baseXp = "0";

        private CaptainSkillSelectorViewModel? captainSkillSelectorViewModel;

        private string commanderXp = "0";

        private string commanderXpBonus = "0";

        private ConsumableViewModel consumableViewModel = null!;

        private string? currentShipIndex = "_default";

        private Ship effectiveShipData = null!;

        private string freeXp = "0";

        private string freeXpBonus = "0";

        private List<string>? nextShipIndex = new();

        private string? previousShipIndex;

        private Ship rawShipData = null!;

        private ShipModuleViewModel shipModuleViewModel = null!;

        private ShipStatsControlViewModel? shipStatsControlViewModel;

        private SignalSelectorViewModel? signalSelectorViewModel;

        private CancellationTokenSource tokenSource;

        private UpgradePanelViewModel upgradePanelViewModel = null!;

        private string xp = "0";

        private string xpBonus = "0";

        public MainWindowViewModel(Ship ship, MainWindow? window, string? previousShipIndex, List<string>? nextShipsIndexes)
        {
            self = window;
            tokenSource = new CancellationTokenSource();

            // Signal selector model
            SignalSelectorViewModel = new SignalSelectorViewModel();

            // Ship stats model
            RawShipData = ship;
            EffectiveShipData = RawShipData;

            // Captain Skill model
            CaptainSkillSelectorViewModel = new CaptainSkillSelectorViewModel(RawShipData.ShipClass, ship.ShipNation);

            OpenSaveBuildCommand = ReactiveCommand.Create(() => OpenSaveBuild());
            BackToMenuCommand = ReactiveCommand.Create(() => BackToMenu());
            NewShipSelectionCommand = ReactiveCommand.Create(() => NewShipSelection());
            ShipModuleViewModel = new ShipModuleViewModel(RawShipData.ShipUpgradeInfo);
            UpgradePanelViewModel = new UpgradePanelViewModel(RawShipData);
            ConsumableViewModel = new ConsumableViewModel(RawShipData);

            ShipStatsControlViewModel = new ShipStatsControlViewModel(EffectiveShipData, ShipModuleViewModel.SelectedModules.ToList(), GenerateModifierList());

            CurrentShipIndex = ship.Index;
            PreviousShipIndex = previousShipIndex;
            NextShipIndex = nextShipsIndexes;

            collectionChangeListeners.Add(ShipModuleViewModel.SelectedModules.WeakSubscribe(_ => UpdateStatsViewModel()));
            collectionChangeListeners.Add(UpgradePanelViewModel.SelectedModernizationList.WeakSubscribe(_ => UpdateStatsViewModel()));
            collectionChangeListeners.Add(SignalSelectorViewModel.SelectedSignals.WeakSubscribe(_ => UpdateStatsViewModel()));
            collectionChangeListeners.Add(CaptainSkillSelectorViewModel.SkillOrderList.WeakSubscribe(_ => UpdateStatsViewModel()));
            UpdateStatsViewModel();
        }

        public MainWindowViewModel(Ship ship, MainWindow? window, string? previousShipIndex, List<string>? nextShipsIndexes, Build build)
        {
            self = window;
            tokenSource = new CancellationTokenSource();

            // Signal selector model
            SignalSelectorViewModel = new SignalSelectorViewModel(build.Signals);

            // Ship stats model
            RawShipData = ship;
            EffectiveShipData = RawShipData;

            // Captain Skill model
            CaptainSkillSelectorViewModel = new CaptainSkillSelectorViewModel(RawShipData.ShipClass, ship.ShipNation, build.Skills);

            OpenSaveBuildCommand = ReactiveCommand.Create(() => OpenSaveBuild());
            BackToMenuCommand = ReactiveCommand.Create(() => BackToMenu());
            NewShipSelectionCommand = ReactiveCommand.Create(() => NewShipSelection());
            ShipModuleViewModel = new ShipModuleViewModel(RawShipData.ShipUpgradeInfo);
            ShipModuleViewModel.LoadBuild(build.Modules);
            UpgradePanelViewModel = new UpgradePanelViewModel(RawShipData);
            UpgradePanelViewModel.LoadBuild(build.Upgrades);
            ConsumableViewModel = new ConsumableViewModel(RawShipData);
            ConsumableViewModel.LoadBuild(build.Consumables);

            ShipStatsControlViewModel = new ShipStatsControlViewModel(EffectiveShipData, ShipModuleViewModel.SelectedModules.ToList(), GenerateModifierList());

            CurrentShipIndex = ship.Index;
            PreviousShipIndex = previousShipIndex;
            NextShipIndex = nextShipsIndexes;

            collectionChangeListeners.Add(ShipModuleViewModel.SelectedModules.WeakSubscribe(_ => UpdateStatsViewModel()));
            collectionChangeListeners.Add(UpgradePanelViewModel.SelectedModernizationList.WeakSubscribe(_ => UpdateStatsViewModel()));
            collectionChangeListeners.Add(SignalSelectorViewModel.SelectedSignals.WeakSubscribe(_ => UpdateStatsViewModel()));
            collectionChangeListeners.Add(CaptainSkillSelectorViewModel.SkillOrderList.WeakSubscribe(_ => UpdateStatsViewModel()));
            UpdateStatsViewModel();
        }

        public MainWindowViewModel()
            : this(AppDataHelper.Instance.ReadLocalJsonData<Ship>(Nation.Germany, ServerType.Live)!["PGSD109"], null, null, null)
        {
        }

        public List<Window> ChildrenWindows { get; set; } = new List<Window>();

        public string? CurrentShipIndex
        {
            get => currentShipIndex;
            set => this.RaiseAndSetIfChanged(ref currentShipIndex, value);
        }

        public string? PreviousShipIndex
        {
            get => previousShipIndex;
            set => this.RaiseAndSetIfChanged(ref previousShipIndex, value);
        }

        public List<string>? NextShipIndex
        {
            get => nextShipIndex;
            set => this.RaiseAndSetIfChanged(ref nextShipIndex, value);
        }

        public ICommand OpenSaveBuildCommand { get; }

        public ICommand BackToMenuCommand { get; }

        public ICommand NewShipSelectionCommand { get; }

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

        public string AccountType
        {
            get => accountType;
            set
            {
                this.RaiseAndSetIfChanged(ref accountType, value);
                CalculateXPValues();
            }
        }

        public bool? AccountState
        {
            get => accountState;
            set
            {
                this.RaiseAndSetIfChanged(ref accountState, value);
                ChangeAccountType(value);
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

        private void OpenSaveBuild()
        {
            var currentBuild = new Build(CurrentShipIndex!, RawShipData.ShipNation, ShipModuleViewModel.SaveBuild(), UpgradePanelViewModel.SaveBuild(), ConsumableViewModel.SaveBuild(), CaptainSkillSelectorViewModel!.GetSkillNumberList(), SignalSelectorViewModel!.GetFlagList());
            var shipName = Localizer.Instance[CurrentShipIndex!].Localization;
            var win = new BuildCreationWindow();
            win.DataContext = new BuildCreationWindowViewModel(win, currentBuild, shipName);
            win.ShowInTaskbar = false;
            win.ShowDialog(self);
        }

        private void BackToMenu()
        {
            StartingMenuWindow win = new();
            StartMenuViewModel model = new(win);
            win.DataContext = model;
            win.Show();
            self!.Close();
        }

        private async void NewShipSelection()
        {
            var result = await ShipSelectionWindow.ShowShipSelection(self!);
            if (result != null)
            {
                foreach (var listener in collectionChangeListeners)
                {
                    listener!.Dispose();
                }

                var temp = ChildrenWindows.ToList();
                foreach (var window in temp)
                {
                    window.Close();
                }

                collectionChangeListeners.Clear();
                var ship = AppDataHelper.Instance.GetShipFromSummary(result);
                AppDataHelper.Instance.LoadNationFiles(result.Nation);
                RawShipData = ship!;
                EffectiveShipData = RawShipData;

                // Captain Skill model
                CaptainSkillSelectorViewModel = new CaptainSkillSelectorViewModel(RawShipData.ShipClass, RawShipData.ShipNation);

                ShipModuleViewModel = new ShipModuleViewModel(RawShipData.ShipUpgradeInfo);
                UpgradePanelViewModel = new UpgradePanelViewModel(RawShipData);
                ConsumableViewModel = new ConsumableViewModel(RawShipData);

                CurrentShipIndex = ship!.Index;
                PreviousShipIndex = result.PrevShipIndex;
                NextShipIndex = result.NextShipsIndex;
                ShipStatsControlViewModel =
                    new ShipStatsControlViewModel(EffectiveShipData, ShipModuleViewModel.SelectedModules.ToList(), GenerateModifierList());

                collectionChangeListeners.Add(ShipModuleViewModel.SelectedModules.WeakSubscribe(_ => UpdateStatsViewModel()));
                collectionChangeListeners.Add(UpgradePanelViewModel.SelectedModernizationList.WeakSubscribe(_ => UpdateStatsViewModel()));
                collectionChangeListeners.Add(SignalSelectorViewModel!.SelectedSignals.WeakSubscribe(_ => UpdateStatsViewModel()));
                collectionChangeListeners.Add(CaptainSkillSelectorViewModel.SkillOrderList.WeakSubscribe(_ => UpdateStatsViewModel()));
                UpdateStatsViewModel();
            }
        }

        private void CalculateXPValues()
        {
            if (!string.IsNullOrEmpty(BaseXp) && !string.IsNullOrEmpty(XpBonus) && !string.IsNullOrEmpty(CommanderXpBonus) &&
                !string.IsNullOrEmpty(FreeXpBonus))
            {
                var baseXp = Convert.ToInt32(BaseXp);
                var xpBonus = Convert.ToDouble(XpBonus);
                var commanderXpBonus = Convert.ToDouble(CommanderXpBonus);
                var freeXpBonus = Convert.ToDouble(FreeXpBonus);

                double accountMultiplier = 1;
                if (AccountType.Equals("WG Premium Account"))
                {
                    accountMultiplier = 1.5;
                }
                else if (AccountType.Equals("WoWs Premium Account"))
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

        private void ChangeAccountType(bool? accountState)
        {
            if (accountState.HasValue)
            {
                if (accountState.Value)
                {
                    AccountType = "WoWs Premium Account";
                }
                else
                {
                    AccountType = "Normal Account";
                }
            }
            else
            {
                AccountType = "WG Premium Account";
            }
        }

        private void UpdateStatsViewModel()
        {
            tokenSource.Cancel();
            tokenSource.Dispose();
            tokenSource = new CancellationTokenSource();
            CancellationToken token = tokenSource.Token;
            Task.Run(
                async () =>
                {
                    await Task.Delay(250, token);
                    if (!token.IsCancellationRequested)
                    {
                        await semaphore.WaitAsync(token);
                        var modifiers = GenerateModifierList();
                        if (ShipStatsControlViewModel != null)
                        {
                            await ShipStatsControlViewModel.UpdateShipStats(ShipModuleViewModel.SelectedModules.ToList(), modifiers);
                        }

                        ConsumableViewModel.UpdateShipConsumables(modifiers);
                        semaphore.Release();
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
