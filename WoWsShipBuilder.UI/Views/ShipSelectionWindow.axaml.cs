using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace WoWsShipBuilder.UI.Views
{
    public partial class ShipSelectionWindow : Window
    {
        public ShipSelectionWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
