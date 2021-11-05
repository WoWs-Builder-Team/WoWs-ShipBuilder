using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Avalonia.Collections;
using Avalonia.Metadata;
using ReactiveUI;
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
        private StartingMenuWindow self;

        public StartMenuViewModel(StartingMenuWindow window)
        {
            self = window;
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
            var result = await ShipSelectionWindow.ShowShipSelection(self);
            if (result != null)
            {
                var ship = AppDataHelper.Instance.GetShipFromSummary(result);
                AppDataHelper.Instance.LoadNationFiles(result.Nation);
                MainWindow win = new MainWindow();
                win.DataContext = new MainWindowViewModel(ship!, win, result.PrevShipIndex, result.NextShipsIndex);
                win.Show();
                self.Close();
            }
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

                Debug.WriteLine(build.BuildName);
            }
            else
            {
                build = BuildList.ElementAt(SelectedBuild!.Value); 
            }

            if (AppData.ShipSummaryList == null)
            {
                AppData.ShipSummaryList = AppDataHelper.Instance.GetShipSummaryList(AppData.Settings.SelectedServerType);
            }

            var summary = AppData.ShipSummaryList!.SingleOrDefault(ship => ship.Index.Equals(build.ShipIndex));
            if (summary is null)
            {
                await MessageBox.Show(self, Translation.StartMenu_BuildLoadingError, Translation.MessageBox_Error, MessageBox.MessageBoxButtons.Ok, MessageBox.MessageBoxIcon.Error);
                return;
            }

            var ship = AppDataHelper.Instance.GetShipFromSummary(summary);
            AppDataHelper.Instance.LoadNationFiles(summary.Nation);
            MainWindow win = new MainWindow();
            win.DataContext = new MainWindowViewModel(ship!, win, summary.PrevShipIndex, summary.NextShipsIndex, build);
            win.Show();
            self.Close();
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
            SettingsWindow win = new()
            {
                ShowInTaskbar = false,
            };
            win.DataContext = new SettingsWindowViewModel(win);
            win.ShowDialog(self);
        }
    }
}
