using System.Reactive;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Mixins;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using WoWsShipBuilder.UI.ViewModels;
using WoWsShipBuilder.ViewModels.Helper;
using BuildCreationWindowViewModel = WoWsShipBuilder.UI.ViewModels.Helper.BuildCreationWindowViewModel;

namespace WoWsShipBuilder.UI.Views
{
    public partial class BuildCreationWindow : ReactiveWindow<BuildCreationWindowViewModel>
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
