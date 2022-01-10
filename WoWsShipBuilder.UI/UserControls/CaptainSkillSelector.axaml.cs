using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using WoWsShipBuilder.UI.ViewModels;

namespace WoWsShipBuilder.UI.UserControls
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1005:Single line comments should begin with single space", Justification = "Future use")]
    public partial class CaptainSkillSelector : UserControl
    {
        public CaptainSkillSelector()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OnSkillActiation_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                var parent = button.Parent;
                var skillActivationPopup = parent?.LogicalChildren.FirstOrDefault(child => child is Popup) as Popup;
                var dc = DataContext as CaptainSkillSelectorViewModel;
                if (dc!.ConditionalModifiersList.Count > 0 || dc!.ShowArHpSelection)
                {
                    skillActivationPopup!.IsOpen = true;
                }
            }
        }

        // For future feature: right click to set alternative skills
        //private void RightClickOnly(object sender, PointerReleasedEventArgs e)
        //{

        //    if (e.InitialPressMouseButton == MouseButton.Right)
        //    {
        //        CaptainSkillSelectorViewModel dc = DataContext as CaptainSkillSelectorViewModel;
        //        var button = sender as NumberedButton;
        //        var buttonDC = button.DataContext as KeyValuePair<string, Skill>?;
        //        button.ToggleNew();
        //        dc.AddSkill(buttonDC.Value.Value);

        //    }
        //    else
        //    {
        //        Debug.WriteLine("Test 2");
        //    }
        //}
    }
}
