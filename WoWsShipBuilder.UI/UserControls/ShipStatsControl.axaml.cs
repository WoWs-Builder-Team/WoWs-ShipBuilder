using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.UI.ViewModels;
using WoWsShipBuilder.UI.Views;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.UI.UserControls
{
    public partial class ShipStatsControl : UserControl
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
            // var dc = DataContext as ShipStatsControlViewModel;
            // var mainBattery = dc!.CurrentShipStats!.MainBatteryModuleList.Values.First();
            // var win = new DispersionGraphsWindow();
            // win.DataContext = new DispersionGraphViewModel(win, mainBattery.DispersionValues, (double)mainBattery.MaxRange, dc.CurrentShipStats.Index);
            // win.Show();
            // e.Handled = true;
        }

        public void OpenTurretAnglesWindow(object sender, PointerReleasedEventArgs e)
        {
            var mainBattery = AppDataHelper.Instance.ReadLocalJsonData<Ship>(Nation.Germany, ServerType.Live)!["PGSD109"].MainBatteryModuleList.First().Value;
            var win = new FiringAngleWindow();
            win.DataContext = new FiringAngleViewModel(mainBattery);
            win.Show();
            e.Handled = true;
        }
    }
}
