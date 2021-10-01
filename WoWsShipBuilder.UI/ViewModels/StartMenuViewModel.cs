using System.Windows.Input;
using ReactiveUI;
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

        private void NewBuild()
        {
            MainWindow win = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
            win.Show();
            self.Close();
        }

        private void LoadBuild()
        {
            TestWindow win = new TestWindow
            {
                DataContext = new TestWindowViewModel(),
            };
            win.Show();
            self.Close();
        }

        private void Setting()
        {
            SettingsWindow win = new SettingsWindow();
            win.DataContext = new SettingsWindowViewModel(win);
            win.ShowDialog(self);
        }
    }
}
