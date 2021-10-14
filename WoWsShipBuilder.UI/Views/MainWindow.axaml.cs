using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.UI.ViewModels;

namespace WoWsShipBuilder.UI.Views
{
    public partial class MainWindow : Window
    {
        private static readonly Regex Regex = new Regex("[^0-9]+");

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            var baseXp = this.FindControl<TextBox>("BaseXpInput");
            baseXp.AddHandler(TextInputEvent, AcceptOnlyNumber, RoutingStrategies.Tunnel);
            var xpInput = this.FindControl<TextBox>("XpInput");
            xpInput.AddHandler(TextInputEvent, AcceptOnlyNumber, RoutingStrategies.Tunnel);
            var commanderXpInput = this.FindControl<TextBox>("CommanderXpInput");
            commanderXpInput.AddHandler(TextInputEvent, AcceptOnlyNumber, RoutingStrategies.Tunnel);
            var freeXpInput = this.FindControl<TextBox>("FreeXpInput");
            freeXpInput.AddHandler(TextInputEvent, AcceptOnlyNumber, RoutingStrategies.Tunnel);
        }

        public void AcceptOnlyNumber(object? sender, TextInputEventArgs e)
        {
            var text = e.Text!;
            if (Regex.IsMatch(text))
            {
                e.Handled = true;
            }
        }

        public void OnClickChangeShipNext(object sender, PointerReleasedEventArgs e)
        {
            var dc = DataContext as MainWindowViewModel;
            var image = sender as Image;
            var shipIndex = image!.Name;
            var ship = AppData.ShipDictionary![shipIndex!];
            var prevShipIndex = AppData.ShipSummaryList!.Where(x => x.Index == shipIndex).First().PrevShipIndex;
            var nextShipIndex = AppData.ShipSummaryList!.Where(x => x.Index == shipIndex).First().NextShipsIndex;
            DataContext = new MainWindowViewModel(ship, this, prevShipIndex, nextShipIndex);
        }

        public void OnClickChangeShipPrevious(object sender, PointerReleasedEventArgs e)
        {
            var dc = DataContext as MainWindowViewModel;
            var image = sender as Image;
            var ship = AppData.ShipDictionary![dc!.PreviousShipIndex!];
            var prevShipIndex = AppData.ShipSummaryList!.Where(x => x.Index == dc.PreviousShipIndex!).First().PrevShipIndex;
            var nextShipIndex = AppData.ShipSummaryList!.Where(x => x.Index == dc.PreviousShipIndex!).First().NextShipsIndex;
            DataContext = new MainWindowViewModel(ship, this, prevShipIndex, nextShipIndex);
        }
    }
}
