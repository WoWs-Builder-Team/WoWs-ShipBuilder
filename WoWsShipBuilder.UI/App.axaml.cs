using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using WoWsShipBuilder.UI.ViewModels;
using WoWsShipBuilder.UI.Views;

namespace WoWsShipBuilder.UI
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                StartingMenuWindow win = new StartingMenuWindow();
                win.DataContext = new StartMenuViewModel(win);
                desktop.MainWindow = win;
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
