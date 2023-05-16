using System.Reactive;
using Avalonia;
using Avalonia.Controls.Mixins;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace WoWsShipBuilder.Desktop.Views
{
    public partial class BuildCreationWindow : ReactiveWindow<ViewModels.Helper.BuildCreationWindowViewModel>
    {
        public BuildCreationWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            this.WhenActivated(disposables =>
            {
                ViewModel?.CloseInteraction.RegisterHandler(interaction =>
                {
                    Close(interaction.Input);
                    interaction.SetOutput(Unit.Default);
                }).DisposeWith(disposables);
            });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
