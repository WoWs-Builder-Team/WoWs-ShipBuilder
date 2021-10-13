using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.UI.UserControls
{
    public class ConsumablePanel : UserControl
    {
        public ConsumablePanel()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void DropDownButton_OnClick(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                var parent = button.Parent;
                var optionsPopup = parent.FindControl<Popup>("ConsumablePopup");
                if (optionsPopup?.DataContext is List<ShipConsumable> { Count: > 1 })
                {
                    optionsPopup.IsOpen = true;
                }
            }
        }
    }
}
