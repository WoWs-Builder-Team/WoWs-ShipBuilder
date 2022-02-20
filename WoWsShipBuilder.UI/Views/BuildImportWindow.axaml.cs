using System.Reactive;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Mixins;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.UI.UserControls;
using WoWsShipBuilder.UI.ViewModels;

namespace WoWsShipBuilder.UI.Views
{
    public partial class BuildImportWindow : ReactiveWindow<BuildImportViewModel>
    {
        public BuildImportWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            this.WhenActivated(disposable =>
            {
                ViewModel?.CloseInteraction.RegisterHandler(interaction =>
                {
                    Close(interaction.Input);
                    interaction.SetOutput(Unit.Default);
                }).DisposeWith(disposable);

                ViewModel?.FileDialogInteraction.RegisterHandler(async interaction =>
                {
                    var fileDialog = new OpenFileDialog
                    {
                        AllowMultiple = false,
                        Directory = AppData.Settings.LastImageImportPath ?? DesktopAppDataService.Instance.BuildImageOutputDirectory,
                        Filters = new()
                        {
                            new() { Name = "PNG Files", Extensions = new() { "png" } },
                        },
                    };

                    string[]? result = await fileDialog.ShowAsync(this);
                    interaction.SetOutput(result);
                }).DisposeWith(disposable);

                ViewModel?.MessageBoxInteraction.RegisterHandler(async interaction =>
                {
                    await MessageBox.Show(this, interaction.Input.text, interaction.Input.title, MessageBox.MessageBoxButtons.Ok, MessageBox.MessageBoxIcon.Error);
                    interaction.SetOutput(Unit.Default);
                }).DisposeWith(disposable);
            });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
