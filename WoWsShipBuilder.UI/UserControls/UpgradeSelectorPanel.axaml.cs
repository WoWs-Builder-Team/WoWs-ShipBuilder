using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using WoWsShipBuilder.UI.ViewModels;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.UI.UserControls
{
    public class UpgradeSelectorPanel : UserControl
    {
        public UpgradeSelectorPanel()
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
                if (optionsPopup?.DataContext is List<Modernization> { Count: > 1 })
                {
                    optionsPopup.IsOpen = true;
                }
            }
        }

        private void InputElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (sender is Image image)
            {
                if (DataContext is UpgradePanelViewModel viewModel && image.DataContext is Modernization modernization)
                {
                    var parent = image.Parent;
                    List<Modernization>? itemList = null;
                    while (parent is not Popup && parent != null)
                    {
                        if (parent is ListBox listBox)
                        {
                            itemList = listBox.Items as List<Modernization>;
                        }

                        parent = parent.Parent;
                    }

                    ((Popup)parent!).IsOpen = false;
                    viewModel.OnModernizationSelected.Invoke(modernization, itemList!);
                }
            }
        }
    }
}
