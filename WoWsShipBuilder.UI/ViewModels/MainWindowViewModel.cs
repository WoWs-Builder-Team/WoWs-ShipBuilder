using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using Newtonsoft.Json;
using ReactiveUI;
using WoWsShipBuilder.Core;
using WoWsShipBuilder.Core.BuildCreator;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.DataUI;
using WoWsShipBuilder.UI.Utilities;
using WoWsShipBuilder.UI.Views;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.UI.ViewModels
{
    // needed for binding to be outside of the class
    public enum Account
    {
        Normal,
        WoWsPremium,
        WGPremium,
    }

    class MainWindowViewModel : ViewModelBase, IScalableViewModel
    {
        // ReSharper disable once CollectionNeverQueried.Local
        private readonly List<IDisposable?> collectionChangeListeners = new();

        private readonly MainWindow? self;

        private readonly SemaphoreSlim semaphore = new(1, 1);

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

        public MainWindowViewModel(Ship ship, MainWindow? window, string? previousShipIndex, List<string>? nextShipsIndexes, Build? build = null, double contentScaling = 1)
        {
            self = window;
            tokenSource = new();
            ContentScaling = contentScaling;

            InitializeData(ship, previousShipIndex, nextShipsIndexes, build);
        }

        public MainWindowViewModel()
            : this(AppDataHelper.Instance.ReadLocalJsonData<Ship>(Nation.Germany, ServerType.Live)!["PGSD109"], null, null, null)
        {
        }

        public List<Window> ChildrenWindows { get; set; } = new();

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

        private double contentScaling = 1;

        public double ContentScaling
        {
            get => contentScaling;
            set => this.RaiseAndSetIfChanged(ref contentScaling, value);
        }

        public void ResetBuild()
        {
            Logging.Logger.Info("Resetting build");
            LoadNewShip(AppData.ShipSummaryList!.First(summary => summary.Index.Equals(CurrentShipIndex)));
        }

        public async void OpenSaveBuild()
        {
            Logging.Logger.Info("Saving build");
            var currentBuild = new Build(CurrentShipIndex!, RawShipData.ShipNation, ShipModuleViewModel.SaveBuild(), UpgradePanelViewModel.SaveBuild(), ConsumableViewModel.SaveBuild(), CaptainSkillSelectorViewModel!.GetCaptainIndex(), CaptainSkillSelectorViewModel!.GetSkillNumberList(), SignalSelectorViewModel!.GetFlagList());
            if (currentBuildName != null)
            {
                currentBuild.BuildName = currentBuildName;
            }

            var shipName = Localizer.Instance[CurrentShipIndex!].Localization;
            var win = new BuildCreationWindow();
            win.DataContext = new BuildCreationWindowViewModel(win, currentBuild, shipName);
            win.ShowInTaskbar = false;
            var dialogResult = await win.ShowDialog<bool>(self);
            if (!dialogResult)
            {
                return;
            }

            currentBuildName = currentBuild.BuildName;
            CreateBuildImage(currentBuild);
        }

        public void BackToMenu()
        {
            StartingMenuWindow win = new();
            win.DataContext = new StartMenuViewModel(win);
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = win;
            }

            win.Show();
            self?.Close();
        }

        public async void NewShipSelection()
        {
            Logging.Logger.Info("Selecting new ship");
            var selectionWin = new ShipSelectionWindow();
            selectionWin.DataContext = new ShipSelectionWindowViewModel(selectionWin, false);
            var result = await selectionWin.ShowDialog<ShipSummary>(self);
            if (result != null)
            {
                Logging.Logger.Info("New ship selected: {0}", result.Index);
                LoadNewShip(result);
            }
        }

        private async void CreateBuildImage(Build currentBuild)
        {
            var screenshotWindow = new ScreenshotWindow
            {
                DataContext = new ScreenshotContainerViewModel(currentBuild, RawShipData),
            };
            screenshotWindow.Show();

            string outputPath = AppDataHelper.Instance.GetImageOutputPath(currentBuild.BuildName);
            await using var bitmapData = new MemoryStream();
            using var bitmap = ScreenshotContainerViewModel.RenderScreenshot(screenshotWindow);
            bitmap.Save(bitmapData);
            bitmapData.Seek(0, SeekOrigin.Begin);
            BuildImageProcessor.AddTextToBitmap(bitmapData, JsonConvert.SerializeObject(currentBuild), outputPath);
            if (OperatingSystem.IsWindows())
            {
                using var savedBitmap = new Bitmap(outputPath);
                await ClipboardHelper.SetBitmapAsync(savedBitmap);
            }

            screenshotWindow.Close();
            if (AppData.Settings.OpenExplorerAfterImageSave)
            {
                Process.Start("explorer.exe", $"/select, \"{outputPath}\"");
            }
        }

        private void LoadNewShip(ShipSummary summary)
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
            var ship = AppDataHelper.Instance.GetShipFromSummary(summary);
            AppDataHelper.Instance.LoadNationFiles(summary.Nation);

            InitializeData(ship!, summary.PrevShipIndex, summary.NextShipsIndex, null);
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

            ShipStatsControlViewModel = new ShipStatsControlViewModel(EffectiveShipData, ShipModuleViewModel.SelectedModules.ToList(), GenerateModifierList());

            CurrentShipIndex = ship.Index;
            CurrentShipTier = ship.Tier;
            PreviousShipIndex = previousIndex;
            if (previousIndex != null)
            {
                PreviousShipTier = AppData.ShipDictionary![previousIndex].Tier;
            }

            NextShips = nextShipsIndexes!.ToDictionary(x => x, x => AppData.ShipDictionary![x].Tier);
            AddChangeListeners();
            UpdateStatsViewModel();
            if (build != null)
            {
                currentBuildName = build.BuildName;
            }
        }

        private void AddChangeListeners()
        {
            collectionChangeListeners.Add(ShipModuleViewModel.SelectedModules.WeakSubscribe(_ => UpdateStatsViewModel()));
            collectionChangeListeners.Add(UpgradePanelViewModel.SelectedModernizationList.WeakSubscribe(_ => UpdateStatsViewModel()));
            collectionChangeListeners.Add(SignalSelectorViewModel!.SelectedSignals.WeakSubscribe(_ => UpdateStatsViewModel()));
            collectionChangeListeners.Add(CaptainSkillSelectorViewModel!.SkillOrderList.WeakSubscribe(_ => UpdateStatsViewModel()));
            collectionChangeListeners.Add(CaptainSkillSelectorViewModel.WhenAnyValue(x => x.CamoEnabled).Subscribe(_ => UpdateStatsViewModel()));
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
            tokenSource = new CancellationTokenSource();
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
