using System.Collections.Generic;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using WoWsShipBuilder.UI.CustomControls;
using WoWsShipBuilder.UI.ViewModels;
using WoWsShipBuilderDataStructures;

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

        private void UpdateVisual(object? sender, SelectionChangedEventArgs e)
        {
            var ic = this.FindControl<ItemsControl>("SkillContainer");
            ic.InvalidateVisual();
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
