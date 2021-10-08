using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace WoWsShipBuilder.UI.UserControls
{
    public class DropDownPanel : UserControl
    {
        public DropDownPanel()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

