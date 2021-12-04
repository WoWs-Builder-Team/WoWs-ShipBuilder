using System;
using System.Linq;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.UI.Extensions;
using WoWsShipBuilder.UI.ViewModels;

namespace WoWsShipBuilder.UI.Views
{
    public partial class MainWindow : ScalableWindow
    {
        private static readonly Regex Regex = new("\\D+");

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

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            this.HandleAndCheckScaling();
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
            var prevShipIndex = AppData.ShipSummaryList!.First(x => x.Index == shipIndex).PrevShipIndex;
            var nextShipIndex = AppData.ShipSummaryList!.First(x => x.Index == shipIndex).NextShipsIndex;
            DataContext = new MainWindowViewModel(ship, this, prevShipIndex, nextShipIndex, contentScaling: dc!.ContentScaling);
        }

        public void OnClickChangeShipPrevious(object sender, PointerReleasedEventArgs e)
        {
            var dc = DataContext as MainWindowViewModel;
            var ship = AppData.ShipDictionary![dc!.PreviousShipIndex!];
            var prevShipIndex = AppData.ShipSummaryList!.First(x => x.Index == dc.PreviousShipIndex!).PrevShipIndex;
            var nextShipIndex = AppData.ShipSummaryList!.First(x => x.Index == dc.PreviousShipIndex!).NextShipsIndex;
            DataContext = new MainWindowViewModel(ship, this, prevShipIndex, nextShipIndex, contentScaling: dc.ContentScaling);
        }
    }
}
