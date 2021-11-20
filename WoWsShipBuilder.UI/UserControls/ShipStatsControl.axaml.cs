using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.DataUI;
using WoWsShipBuilder.UI.ViewModels;
using WoWsShipBuilder.UI.Views;
using WoWsShipBuilderDataStructures;

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
            var dc = DataContext as ShipStatsControlViewModel;
            var mainBattery = dc!.CurrentShipStats!.MainBatteryUI!;
            var win = new DispersionGraphsWindow();
            var apShellName = dc.CurrentShipStats.MainBatteryUI!.ShellData.First(x => x.Type.Equals("ap", System.StringComparison.InvariantCultureIgnoreCase)).Index;
            var apShell = (ArtilleryShell)AppDataHelper.Instance.GetProjectile(apShellName);
            win.DataContext = new DispersionGraphViewModel(win, mainBattery.DispersionData, (double)mainBattery.Range * 1000, dc.CurrentShipStats.Index, apShell, DispersionGraphViewModel.Tabs.Dispersion);
            win.Show((Window)this.GetVisualRoot());
            e.Handled = true;
        }

        public void OpenBallisticGraphWindow(object sender, PointerReleasedEventArgs e)
        {
            var dc = DataContext as ShipStatsControlViewModel;
            var mainBattery = dc!.CurrentShipStats!.MainBatteryUI!;
            var win = new DispersionGraphsWindow();
            var textBlock = (TextBlock)sender;
            var shellIndex = ((ShellUI)textBlock.DataContext!).Index;
            var shell = AppDataHelper.Instance.GetProjectile<ArtilleryShell>(shellIndex);
            win.DataContext = new DispersionGraphViewModel(win, mainBattery.DispersionData, (double)mainBattery.Range * 1000, dc.CurrentShipStats.Index, shell, DispersionGraphViewModel.Tabs.Ballistic);
            win.Show((Window)this.GetVisualRoot());
            e.Handled = true;
        }

        public void OpenTurretAnglesWindow(object sender, PointerReleasedEventArgs e)
        {
            var dc = (ShipStatsControlViewModel)DataContext!;
            var win = new FiringAngleWindow();
            win.DataContext = new FiringAngleViewModel(dc.CurrentShipStats!.MainBatteryUI!.OriginalMainBatteryData);
            win.Show((Window)this.GetVisualRoot());
            e.Handled = true;
        }
    }
}
