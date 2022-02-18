using System.Diagnostics;
using System.Reactive;
using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using WoWsShipBuilder.UI.UserControls;
using WoWsShipBuilder.UI.ViewModels;

namespace WoWsShipBuilder.UI.Views
{
    public partial class SettingsWindow : ReactiveWindow<SettingsWindowViewModel>
    {
        public SettingsWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            this.WhenActivated(disposables =>
            {
                ViewModel?.ShowWarningInteraction.RegisterHandler(async interaction =>
                {
                    var result = await MessageBox.Show(this, interaction.Input.text, interaction.Input.title, MessageBox.MessageBoxButtons.YesNo, MessageBox.MessageBoxIcon.Warning);
                    interaction.SetOutput(result);
                }).DisposeWith(disposables);

                ViewModel?.ShowErrorInteraction.RegisterHandler(async interaction =>
                {
                    await MessageBox.Show(this, interaction.Input.text, interaction.Input.title, MessageBox.MessageBoxButtons.Ok, MessageBox.MessageBoxIcon.Error);
                    interaction.SetOutput(Unit.Default);
                }).DisposeWith(disposables);

                ViewModel?.ShowDownloadWindowInteraction.RegisterHandler(async interaction =>
                {
                    await new DownloadWindow().ShowDialog(this);
                    interaction.SetOutput(Unit.Default);
                }).DisposeWith(disposables);

                ViewModel?.SelectFolderInteraction.RegisterHandler(async interaction =>
                {
                    var dialog = new OpenFolderDialog
                    {
                        Directory = interaction.Input,
                    };
                    interaction.SetOutput(await dialog.ShowAsync(this));
                }).DisposeWith(disposables);

                ViewModel?.ShutdownInteraction.RegisterHandler(interaction =>
                {
                    interaction.SetOutput(Unit.Default);
                    (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Shutdown();
                });

                ViewModel?.CloseInteraction.RegisterHandler(interaction =>
                {
                    Close();
                    interaction.SetOutput(Unit.Default);
                }).DisposeWith(disposables);
            });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OpenDiscord(object? sender, Avalonia.Input.PointerReleasedEventArgs e)
        {
            string url = "https://discord.gg/C8EaepZJDY";

            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true,
            });
        }
    }
}
