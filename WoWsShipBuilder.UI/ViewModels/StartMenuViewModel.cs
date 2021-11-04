using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Avalonia.Collections;
using Avalonia.Metadata;
using ReactiveUI;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.UI.Translations;
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
            var list = AppData.Builds.Select(build => build.BuildName);
            BuildList.Add(Translation.StartMenu_ImportBuild);
            BuildList.AddRange(list!);
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

        private AvaloniaList<string> buildList = new();

        public AvaloniaList<string> BuildList
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
            if (SelectedBuild.Equals(0))
            {
                BuildImportWindow win = new();
                win.DataContext = new BuildImportViewModel(win);
                var build = await win.ShowDialog<string>(self);
                Debug.WriteLine(build);
            }
            else
            {
                // TODO normal build loading
            }
        }

        [DependsOn(nameof(SelectedBuild))]
        public bool CanLoadBuild(object parameter)
        {
            return SelectedBuild != null;
        }

        public void DeleteBuild()
        {
            AppData.Builds.RemoveAt(SelectedBuild!.Value);
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
