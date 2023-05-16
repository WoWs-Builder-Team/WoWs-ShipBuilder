using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace WoWsShipBuilder.Desktop.UserControls
{
    public partial class SkillToolTip : UserControl
    {
        public SkillToolTip()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
