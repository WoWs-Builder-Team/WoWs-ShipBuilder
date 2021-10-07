using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

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
    }
}
