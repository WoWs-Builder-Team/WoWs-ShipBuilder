using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.DataUI;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.UI.ViewModels;
using WoWsShipBuilder.UI.ViewModels.DispersionPlot;
using WoWsShipBuilder.UI.Views;
using WoWsShipBuilder.ViewModels.ShipVm;
using static WoWsShipBuilder.UI.ViewModels.DispersionPlot.DispersionGraphViewModel;

namespace WoWsShipBuilder.UI.UserControls
{
    public class ShipStatsControl : UserControl
    {
        public ShipStatsControl()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void OpenDispersionGraphWindow(object sender, PointerReleasedEventArgs e)
        {
            OpenGraphsWindow(sender, e, Tabs.Dispersion);
        }

        public void OpenDispersionPlotWindow(object sender, PointerReleasedEventArgs e)
        {
            OpenGraphsWindow(sender, e, Tabs.Plot);
        }

        public void OpenBallisticGraphWindow(object sender, PointerReleasedEventArgs e)
        {
            OpenGraphsWindow(sender, e, Tabs.Ballistic);
        }

        public void OpenShellTrajectoryWindow(object sender, PointerReleasedEventArgs e)
        {
            OpenGraphsWindow(sender, e, Tabs.Trajectory);
        }

        public void OpenTurretAnglesWindow(object sender, PointerReleasedEventArgs e)
        {
            var dc = (ShipStatsControlViewModelBase)DataContext!;
            var win = new FiringAngleWindow
            {
                DataContext = new FiringAngleViewModelBase(dc.CurrentShipStats!.MainBatteryUI!.OriginalMainBatteryData),
            };
            win.Show((Window)this.GetVisualRoot());
            e.Handled = true;
        }

        private void OpenGraphsWindow(object sender, PointerReleasedEventArgs e, Tabs tab)
        {
            var dc = DataContext as ShipStatsControlViewModelBase;
            var mainBattery = dc!.CurrentShipStats!.MainBatteryUI!;
            var win = new DispersionGraphsWindow();
            var textBlock = (TextBlock)sender;
            var shellIndex = ((ShellUI)textBlock.DataContext!).Index;
            var shell = DesktopAppDataService.Instance.GetProjectile<ArtilleryShell>(shellIndex);
            win.DataContext = new DispersionGraphViewModel(win, mainBattery.DispersionData, (double)mainBattery.Range * 1000, dc.CurrentShipStats.Index, shell, tab, mainBattery.Sigma);
            win.Show((Window)this.GetVisualRoot());
            e.Handled = true;
        }
    }
}
