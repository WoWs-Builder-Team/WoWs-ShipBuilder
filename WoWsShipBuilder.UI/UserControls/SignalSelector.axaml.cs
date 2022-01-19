using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace WoWsShipBuilder.UI.UserControls
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
