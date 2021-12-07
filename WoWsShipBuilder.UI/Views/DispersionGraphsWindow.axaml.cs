using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using WoWsShipBuilder.UI.ViewModels;

namespace WoWsShipBuilder.UI.Views
{
    public class DispersionGraphsWindow : Window
    {
        public DispersionGraphsWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            Opened += DispersionGraphsWindow_Opened;
            Closing += DispersionGraphsWindow_Closing;
        }

        private void DispersionGraphsWindow_Closing(object? sender, System.EventArgs e)
        {
            if (Owner?.DataContext is MainWindowViewModel mainWindowViewModel)
            {
                mainWindowViewModel.ChildrenWindows.Remove(this);
            }
            else
            {
                var startWindow = new StartingMenuWindow();
                var startViewModel = new StartMenuViewModel(startWindow);
                startWindow.DataContext = startViewModel;
                startWindow.Show();
                if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    desktop.MainWindow = startWindow;
                }
            }
        }

        private void DispersionGraphsWindow_Opened(object? sender, System.EventArgs e)
        {
            if (Owner?.DataContext is MainWindowViewModel mainWindowViewModel)
            {
                mainWindowViewModel.ChildrenWindows.Add(this);
            }
        }
    }
}
