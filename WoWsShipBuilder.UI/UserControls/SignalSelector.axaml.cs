using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using WoWsShipBuilder.UI.CustomControls;
using WoWsShipBuilder.UI.ViewModels;

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
