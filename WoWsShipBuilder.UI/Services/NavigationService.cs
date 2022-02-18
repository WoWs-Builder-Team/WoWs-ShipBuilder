using Autofac;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using WoWsShipBuilder.Core.BuildCreator;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.UI.ViewModels;
using WoWsShipBuilder.UI.Views;

namespace WoWsShipBuilder.UI.Services
{
    public class NavigationService : INavigationService
    {
        private readonly ILifetimeScope scope;

        public NavigationService(ILifetimeScope scope)
        {
            this.scope = scope;
        }

        internal NavigationService()
            : this(null!)
        {
        }

        public void OpenStartMenu(bool closeMainWindow = false)
        {
            using var subScope = scope.BeginLifetimeScope();
            var startWindow = new StartMenuWindow
            {
                DataContext = subScope.Resolve<StartMenuViewModel>(),
            };

            startWindow.Show();
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                if (closeMainWindow)
                {
                    desktop.MainWindow.Close();
                }

                desktop.MainWindow = startWindow;
            }
        }

        public void OpenMainWindow(Ship ship, ShipSummary summary, Build? build = null, bool closeMainWindow = false)
        {
            using var subScope = scope.BeginLifetimeScope();
            MainWindow win = new()
            {
                DataContext = subScope.Resolve<MainWindowViewModel>(new NamedParameter("ship", ship), new NamedParameter("shipSummary", summary), new TypedParameter(typeof(Build), build)),
            };

            win.Show();
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                if (closeMainWindow)
                {
                    desktop.MainWindow.Close();
                }

                desktop.MainWindow = win;
            }
        }

        public void OpenDispersionPlotWindow(bool closeCurrentWindow = false)
        {
            using var subScope = scope.BeginLifetimeScope();
            var window = new DispersionGraphsWindow();
            var viewModel = subScope.Resolve<DispersionGraphViewModel>(new NamedParameter("window", window));
            window.DataContext = viewModel;
            window.Show();

            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                if (closeCurrentWindow)
                {
                    desktop.MainWindow.Close();
                }

                desktop.MainWindow = window;
            }
        }
    }
}
