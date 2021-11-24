using System.Linq;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Metadata;
using Newtonsoft.Json;
using ReactiveUI;
using WoWsShipBuilder.Core;
using WoWsShipBuilder.Core.BuildCreator;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.UI.Translations;
using WoWsShipBuilder.UI.UserControls;
using WoWsShipBuilder.UI.Views;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.UI.ViewModels
{
    class StartMenuViewModel : ViewModelBase
    {
        private readonly StartingMenuWindow? self;

        public StartMenuViewModel()
            : this(null)
        {
        }

        public StartMenuViewModel(StartingMenuWindow? window)
        {
            self = window;
            if (!AppData.Builds.Any())
            {
                AppDataHelper.Instance.LoadBuilds();
            }

            BuildList.CollectionChanged += BuildList_CollectionChanged;
            BuildList.Add(new Build(Translation.StartMenu_ImportBuild));
            BuildList.AddRange(AppData.Builds);
        }

        private void BuildList_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.RaisePropertyChanged(nameof(BuildList));
        }

        private int? selectedBuild;

        public int? SelectedBuild
        {
            get => selectedBuild;
            set => this.RaiseAndSetIfChanged(ref selectedBuild, value);
        }

        private AvaloniaList<Build> buildList = new();

        public AvaloniaList<Build> BuildList
        {
            get => buildList;
            set => this.RaiseAndSetIfChanged(ref buildList, value);
        }

        public async void NewBuild()
        {
            if (Design.IsDesignMode)
            {
                return;
            }

            var selectionWin = new ShipSelectionWindow();
            selectionWin.DataContext = new ShipSelectionWindowViewModel(selectionWin);
            var result = await selectionWin.ShowDialog<ShipSummary>(self);
            if (result != null)
            {
                Logging.Logger.Info($"Selected ship with index {result.Index}");
                var ship = AppDataHelper.Instance.GetShipFromSummary(result);
                AppDataHelper.Instance.LoadNationFiles(result.Nation);
                MainWindow win = new();
                win.DataContext = new MainWindowViewModel(ship!, win, result.PrevShipIndex, result.NextShipsIndex);
                if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    desktop.MainWindow = win;
                }

                win.Show();
                self?.Close();
            }
        }

        public void OpenDispersionGraphWindow()
        {
            if (Design.IsDesignMode)
            {
                return;
            }

            var window = new DispersionGraphsWindow();
            var viewModel = new DispersionGraphViewModel(window);
            window.DataContext = viewModel;
            window.Show();

            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = window;
            }

            self?.Close();
        }

        public async void LoadBuild(object parameter)
        {
            Build build;
            if (SelectedBuild.Equals(0))
            {
                BuildImportWindow importWin = new();
                importWin.DataContext = new BuildImportViewModel(importWin);
                build = await importWin.ShowDialog<Build>(self);
                if (build is null)
                {
                    return;
                }
            }
            else
            {
                build = BuildList.ElementAt(SelectedBuild!.Value);
            }

            Logging.Logger.Info("Loading build {0}", JsonConvert.SerializeObject(build));

            if (AppData.ShipSummaryList == null)
            {
                Logging.Logger.Info("Ship summary is null, loading it.");
                AppData.ShipSummaryList = AppDataHelper.Instance.GetShipSummaryList(AppData.Settings.SelectedServerType);
            }

            var summary = AppData.ShipSummaryList.SingleOrDefault(ship => ship.Index.Equals(build.ShipIndex));
            if (summary is null)
            {
                await MessageBox.Show(self, Translation.StartMenu_BuildLoadingError, Translation.MessageBox_Error, MessageBox.MessageBoxButtons.Ok, MessageBox.MessageBoxIcon.Error);
                return;
            }

            var ship = AppDataHelper.Instance.GetShipFromSummary(summary);
            AppDataHelper.Instance.LoadNationFiles(summary.Nation);
            MainWindow win = new();
            win.DataContext = new MainWindowViewModel(ship!, win, summary.PrevShipIndex, summary.NextShipsIndex, build);
            win.Show();
            self?.Close();
        }

        [DependsOn(nameof(SelectedBuild))]
        public bool CanLoadBuild(object parameter)
        {
            return SelectedBuild != null && SelectedBuild >= 0;
        }

        public void DeleteBuild()
        {
            AppData.Builds.RemoveAt(SelectedBuild!.Value - 1);
            BuildList.RemoveAt(SelectedBuild!.Value);
        }

        [DependsOn(nameof(SelectedBuild))]
        public bool CanDeleteBuild(object parameter)
        {
            if (SelectedBuild != null && SelectedBuild > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Setting()
        {
            if (Design.IsDesignMode)
            {
                return;
            }

            SettingsWindow win = new()
            {
                ShowInTaskbar = false,
            };
            win.DataContext = new SettingsWindowViewModel(win);
            win.ShowDialog(self);
        }
    }
}
