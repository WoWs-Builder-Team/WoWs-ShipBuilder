using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace WoWsShipBuilder.Desktop.UserControls
{
    public partial class FlagToolTip : UserControl
    {
        public FlagToolTip()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
