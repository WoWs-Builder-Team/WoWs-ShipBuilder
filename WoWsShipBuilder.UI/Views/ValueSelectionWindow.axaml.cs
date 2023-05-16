using System.Reactive;
using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using WoWsShipBuilder.UI.ViewModels;
using WoWsShipBuilder.ViewModels.Helper;
using ValueSelectionViewModel = WoWsShipBuilder.UI.ViewModels.Helper.ValueSelectionViewModel;

namespace WoWsShipBuilder.UI.Views
{
    public partial class ValueSelectionWindow : ReactiveWindow<ValueSelectionViewModel>
    {
        public ValueSelectionWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            this.WhenActivated(disposable =>
            {
                if (ViewModel != null)
                {
                    disposable(ViewModel.ConfirmationInteraction.RegisterHandler(selection =>
                    {
                        Close(selection.Input);
                        selection.SetOutput(Unit.Default);
                    }));
                }
            });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
