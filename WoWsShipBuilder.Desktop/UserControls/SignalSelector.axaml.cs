using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace WoWsShipBuilder.Desktop.UserControls
{
    public partial class SignalSelector : UserControl
    {
        public SignalSelector()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
