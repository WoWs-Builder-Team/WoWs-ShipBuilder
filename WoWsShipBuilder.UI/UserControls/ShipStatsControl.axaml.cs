using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using WoWsShipBuilder.UI.ViewModels;
using WoWsShipBuilder.UI.Views;

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
    }
}
