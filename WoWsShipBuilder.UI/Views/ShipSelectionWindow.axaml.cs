using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using WoWsShipBuilder.UI.ViewModels;

namespace WoWsShipBuilder.UI.Views
{
    public class ShipSelectionWindow : Window
    {
        public ShipSelectionWindow()
            : this(false)
        {
        }

        public ShipSelectionWindow(bool multiSelection)
        {
            InitializeComponent();
            if (multiSelection)
            {              
                var list = this.FindControl<ListBox>("SelectionList");
                list.SelectionMode = SelectionMode.Multiple;
            }
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void RunResearch(object sender, PointerReleasedEventArgs e)
        {
            if (e.Source is not TextBlock && e.Source is not ContentPresenter)
            {
                var box = sender as AutoCompleteBox;
                box!.IsDropDownOpen = true;
                e.Handled = true;
            }
        }

        // Workaround to set initial focus
        private void InputElement_OnGotFocus(object? sender, GotFocusEventArgs e)
        {
            this.FindControl<TextBox>("FilterBox").Focus();
            GotFocus -= InputElement_OnGotFocus;
        }

        private void ListItem_OnDoubleTapped(object? sender, RoutedEventArgs e)
        {
            if (DataContext is ShipSelectionWindowViewModel viewModel && sender is Panel panel)
            {
                // viewModel.SelectedShipList.Add((KeyValuePair<string, ShipSummary>)panel.DataContext!);
                if (viewModel.CanConfirm(null!))
                {
                    viewModel.Confirm(null!);
                }
            }
        }
    }
}
