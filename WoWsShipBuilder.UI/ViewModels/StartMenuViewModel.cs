using System.Diagnostics;
using System.Windows.Input;
using ReactiveUI;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.UI.Views;

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
            //var result = await ShipSelectionWindow.ShowShipSelection(self);
            //if (result != null)
            //{
            //    AppDataHelper.Instance.GetShipFromSummary(result);
            //}

            MainWindow win = new MainWindow();
            win.DataContext = new MainWindowViewModel(win);
            win.Show();
            self.Close();
        }

        private void LoadBuild()
        {
            // Insert build loading
        }

        private void Setting()
        {
            SettingsWindow win = new SettingsWindow();
            win.DataContext = new SettingsWindowViewModel(win);
            win.ShowDialog(self);
        }
    }
}
