using System.Reactive;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using WoWsShipBuilder.UI.ViewModels;
using WoWsShipBuilder.ViewModels.Helper;

namespace WoWsShipBuilder.UI.Views
{
    public class ShipSelectionWindow : ReactiveWindow<ShipSelectionWindowViewModel>
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
            this.WhenActivated(disposables =>
            {
                if (ViewModel != null)
                {
                    disposables(ViewModel.CloseInteraction.RegisterHandler(interaction =>
                    {
                        interaction.SetOutput(Unit.Default);
                        Close(interaction.Input);
                    }));
                }
            });
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
                if (viewModel.ConfirmCommand.CanExecute(null))
                {
                    viewModel.ConfirmCommand.Execute(null);
                }
            }
        }
    }
}
