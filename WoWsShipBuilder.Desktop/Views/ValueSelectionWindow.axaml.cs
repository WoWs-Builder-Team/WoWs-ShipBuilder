using System.Reactive;
using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace WoWsShipBuilder.Desktop.Views
{
    public partial class ValueSelectionWindow : ReactiveWindow<ViewModels.Helper.ValueSelectionViewModel>
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
