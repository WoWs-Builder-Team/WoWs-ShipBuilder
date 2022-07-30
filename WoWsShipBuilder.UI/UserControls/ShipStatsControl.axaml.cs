using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using WoWsShipBuilder.Core.DataContainers;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.DataStructures;
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

        public async void OpenDispersionGraphWindow(object sender, PointerReleasedEventArgs e)
        {
            await OpenGraphsWindow(sender, e, Tabs.Dispersion);
        }

        public async void OpenDispersionPlotWindow(object sender, PointerReleasedEventArgs e)
        {
            await OpenGraphsWindow(sender, e, Tabs.Plot);
        }

        public async void OpenBallisticGraphWindow(object sender, PointerReleasedEventArgs e)
        {
            await OpenGraphsWindow(sender, e, Tabs.Ballistic);
        }

        public async void OpenShellTrajectoryWindow(object sender, PointerReleasedEventArgs e)
        {
            await OpenGraphsWindow(sender, e, Tabs.Trajectory);
        }

        public void OpenTurretAnglesWindow(object sender, PointerReleasedEventArgs e)
        {
            var dc = (ShipStatsControlViewModel)DataContext!;
            var win = new FiringAngleWindow
            {
                DataContext = new FiringAngleViewModelBase(dc.CurrentShipStats!.MainBatteryDataContainer!.OriginalMainBatteryData.Guns),
            };
            win.Show((Window)this.GetVisualRoot());
            e.Handled = true;
        }

        private async Task OpenGraphsWindow(object sender, PointerReleasedEventArgs e, Tabs tab)
        {
            var dc = DataContext as ShipStatsControlViewModel;
            var mainBattery = dc!.CurrentShipStats!.MainBatteryDataContainer!;
            var win = new DispersionGraphsWindow();
            var textBlock = (TextBlock)sender;
            string shellIndex = ((ShellDataContainer)textBlock.DataContext!).Name;
            var shell = await DesktopAppDataService.Instance.GetProjectile<ArtilleryShell>(shellIndex);
            win.DataContext = new DispersionGraphViewModel(win, mainBattery.DispersionData, (double)mainBattery.Range * 1000, dc.CurrentShipStats.Index, shell, tab, mainBattery.Sigma);
            win.Show((Window)this.GetVisualRoot());
            e.Handled = true;
        }

        public void OpenDepthChargeDamageDistributionChart(object sender, PointerReleasedEventArgs e)
        {
            if (DataContext is not ShipStatsControlViewModel vm)
            {
                e.Handled = true;
                return;
            }

            var dataContext = vm.CurrentShipStats?.DepthChargeLauncherDataContainer?.DepthCharge as DepthChargeDataContainer ?? vm.CurrentShipStats?.AswAirstrikeDataContainer?.Weapon as DepthChargeDataContainer;
            if (dataContext != null)
            {
                var win = new DepthChargeDamageDistributionChartWindow
                {
                    DataContext = new DepthChargeDamageDistributionChartRecord(dataContext),
                };
                win.Show((Window)this.GetVisualRoot());
            }

            e.Handled = true;
        }

        private void ShowTorpedoAngles(object? sender, PointerReleasedEventArgs e)
        {
            if (DataContext is ShipStatsControlViewModel viewModel)
            {
                var win = new FiringAngleWindow
                {
                    DataContext = new FiringAngleViewModelBase(viewModel.CurrentShipStats!.TorpedoArmamentDataContainer!.TorpedoLaunchers),
                };
                win.Show((Window)this.GetVisualRoot());
                e.Handled = true;
            }
        }
    }
}
