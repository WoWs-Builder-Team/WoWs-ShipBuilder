using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using WoWsShipBuilder.Common.DataContainers;
using WoWsShipBuilder.Common.Features.ShipStats.ViewModels;

namespace WoWsShipBuilder.UI.UserControls;

public partial class ConsumablePanelItem : UserControl
{
    public ConsumablePanelItem()
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
            if (optionsPopup?.DataContext is ConsumableSlotViewModel { ConsumableData.Count: > 1 })
            {
                optionsPopup.IsOpen = true;
            }
        }
    }

    private void InputElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is Image image)
        {
            if (DataContext is ConsumableSlotViewModel viewModel && image.DataContext is ConsumableDataContainer consumableData)
            {
                var parent = image.Parent;
                while (parent is not Popup && parent != null)
                {
                    parent = parent.Parent;
                }

                ((Popup)parent!).IsOpen = false;
                viewModel.SelectedIndex = viewModel.ConsumableData.IndexOf(consumableData);
            }
        }
    }

    private void ContentPanel_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (e.InitialPressMouseButton == MouseButton.Right && DataContext is ConsumableSlotViewModel viewModel)
        {
            viewModel.ConsumableActivated = !viewModel.ConsumableActivated;
        }
    }
}
