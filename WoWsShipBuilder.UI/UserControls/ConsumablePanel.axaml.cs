using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using WoWsShipBuilder.Core.DataContainers;
using WoWsShipBuilder.ViewModels.ShipVm;

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
                var optionsPopup = parent?.LogicalChildren.FirstOrDefault(child => child is Popup) as Popup;
                if (optionsPopup?.DataContext is List<ConsumableDataContainer> { Count: > 1 })
                {
                    optionsPopup.IsOpen = true;
                }
            }
        }

        private void InputElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (sender is Image image)
            {
                if (DataContext is ConsumableViewModel viewModel && image.DataContext is ConsumableDataContainer consumableUi)
                {
                    var parent = image.Parent;
                    while (parent is not Popup && parent != null)
                    {
                        parent = parent.Parent;
                    }

                    ((Popup)parent!).IsOpen = false;
                    viewModel.OnConsumableSelected.Invoke(consumableUi);
                }
            }
        }
    }
}
