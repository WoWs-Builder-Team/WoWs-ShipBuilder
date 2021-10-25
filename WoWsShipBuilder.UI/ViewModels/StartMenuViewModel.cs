using System.Linq;
using System.Windows.Input;
using ReactiveUI;
using WoWsShipBuilder.Core.DataProvider;
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
            NewBuildCommand = ReactiveCommand.Create(() => NewBuild());
            LoadBuildCommand = ReactiveCommand.Create(() => LoadBuild());
            SettingCommand = ReactiveCommand.Create(() => Setting());
        }

        private int selectedBuild;

        public int SelectedBuild
        {
            get => selectedBuild;
            set => this.RaiseAndSetIfChanged(ref selectedBuild, value);
        }

        public ICommand NewBuildCommand { get; }

        public ICommand LoadBuildCommand { get; }

        public ICommand SettingCommand { get; }

        private async void NewBuild()
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

        private void LoadBuild()
        {
            // TODO
        }

        private void Setting()
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
