using System.IO.Abstractions;
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
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.UI.Translations;
using WoWsShipBuilder.UI.UserControls;
using WoWsShipBuilder.UI.Utilities;
using WoWsShipBuilder.UI.Views;

namespace WoWsShipBuilder.UI.ViewModels
{
    class StartMenuViewModel : ViewModelBase, IScalableViewModel
    {
        private readonly StartingMenuWindow? self;
        private readonly IFileSystem fileSystem;

        public StartMenuViewModel()
            : this(null, new FileSystem())
        {
        }

        public StartMenuViewModel(StartingMenuWindow? window, IFileSystem fileSystem)
        {
            self = window;
            this.fileSystem = fileSystem;
            if (!AppData.Builds.Any())
            {
                AppDataHelper.Instance.LoadBuilds();
            }

            BuildList.CollectionChanged += BuildList_CollectionChanged;
            BuildList.Add(new(Translation.StartMenu_ImportBuild));
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
            set
            {
                this.RaiseAndSetIfChanged(ref selectedBuild, value);
                this.RaisePropertyChanged(nameof(LoadBuildButtonText));
            }
        }

        private AvaloniaList<Build> buildList = new();

        public AvaloniaList<Build> BuildList
        {
            get => buildList;
            set => this.RaiseAndSetIfChanged(ref buildList, value);
        }

        private double contentScaling = 1;

        public double ContentScaling
        {
            get => contentScaling;
            set => this.RaiseAndSetIfChanged(ref contentScaling, value);
        }

        public string LoadBuildButtonText => SelectedBuild is > 0 ? Translation.StartMenu_LoadBuild : Translation.StartMenu_ImportBuild;

        public async void NewBuild()
        {
            if (Design.IsDesignMode)
            {
                return;
            }

            var selectionWin = new ShipSelectionWindow();
            selectionWin.DataContext = new ShipSelectionWindowViewModel(selectionWin, false);
            var result = await selectionWin.ShowDialog<ShipSummary>(self);
            if (result != null)
            {
                Logging.Logger.Info($"Selected ship with index {result.Index}");
                try
                {
                    var ship = AppDataHelper.Instance.GetShipFromSummary(result);
                    AppDataHelper.Instance.LoadNationFiles(result.Nation);
                    MainWindow win = new();
                    win.DataContext = new MainWindowViewModel(fileSystem, ship!, win, result.PrevShipIndex, result.NextShipsIndex);
                    if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                    {
                        desktop.MainWindow = win;
                    }

                    win.Show();
                    self?.Close();
                }
                catch (System.Exception e)
                {
                    Logging.Logger.Error(e, $"Error during the loading of the local json files");
                    await MessageBox.Show(self, Translation.MessageBox_LoadingError, Translation.MessageBox_Error, MessageBox.MessageBoxButtons.Ok, MessageBox.MessageBoxIcon.Error, 500, sizeToContent: SizeToContent.Height);
                }
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

        public async void LoadBuild()
        {
            Build? build;
            if (SelectedBuild is > 0)
            {
                build = BuildList.ElementAt(SelectedBuild.Value);
            }
            else
            {
                BuildImportWindow importWin = new();
                importWin.DataContext = new BuildImportViewModel(importWin, fileSystem);
                build = await importWin.ShowDialog<Build?>(self);
                if (build is null)
                {
                    return;
                }
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
            win.DataContext = new MainWindowViewModel(fileSystem, ship!, win, summary.PrevShipIndex, summary.NextShipsIndex, build);
            win.Show();
            self?.Close();
        }

        public void DeleteBuild()
        {
            AppData.Builds.RemoveAt(SelectedBuild!.Value - 1);
            BuildList.RemoveAt(SelectedBuild!.Value);
        }

        [DependsOn(nameof(SelectedBuild))]
        public bool CanDeleteBuild(object parameter) => SelectedBuild is > 0;

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
