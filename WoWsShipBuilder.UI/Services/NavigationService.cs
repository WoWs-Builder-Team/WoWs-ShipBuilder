using System.Threading.Tasks;
using Autofac;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.Features.Builds;
using WoWsShipBuilder.Features.ShipStats;
using WoWsShipBuilder.Infrastructure;
using WoWsShipBuilder.UI.ViewModels;
using WoWsShipBuilder.UI.ViewModels.DispersionPlot;
using WoWsShipBuilder.UI.ViewModels.ShipVm;
using WoWsShipBuilder.UI.Views;

namespace WoWsShipBuilder.UI.Services;

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
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            if (closeMainWindow)
            {
                desktop.MainWindow.Close();
            }

            desktop.MainWindow = startWindow;
        }
    }

    public async Task OpenMainWindow(Ship ship, ShipSummary summary, Build? build = null, bool closeMainWindow = false)
    {
        await using var subScope = scope.BeginLifetimeScope();
        var vmParams = new ShipViewModelParams(ship, summary, build);
        var vm = subScope.Resolve<ShipWindowViewModel>(new TypedParameter(typeof(ShipViewModelParams), vmParams));
        vm.InitializeData(vmParams);

        MainWindow win = new()
        {
            DataContext = vm,
        };

        win.Show();
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
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

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            if (closeCurrentWindow)
            {
                desktop.MainWindow.Close();
            }

            desktop.MainWindow = window;
        }
    }
}
