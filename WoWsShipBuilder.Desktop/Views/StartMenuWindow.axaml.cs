using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using Squirrel;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.Desktop.UserControls;
using WoWsShipBuilder.Desktop.Utilities;
using WoWsShipBuilder.Desktop.ViewModels;
using WoWsShipBuilder.Features.Builds;
using WoWsShipBuilder.Infrastructure;

namespace WoWsShipBuilder.Desktop.Views
{
    public partial class StartMenuWindow : ScalableReactiveWindow<StartMenuViewModel>
    {
        public StartMenuWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            var control = this.FindControl<Image>("Image");
#if DEBUG
            control.PointerReleased += Control_PointerReleased;
#else
            control.IsHitTestVisible = false;
#endif
            this.WhenActivated(disposables =>
            {
                ViewModel?.SelectShipInteraction.RegisterHandler(async interaction =>
                {
                    var result = await new ShipSelectionWindow(false) { DataContext = interaction.Input }.ShowDialog<List<ShipSummary>>(this);
                    interaction.SetOutput(result);
                }).DisposeWith(disposables);

                ViewModel?.ShowSettingsInteraction.RegisterHandler(async interaction =>
                {
                    await new SettingsWindow
                    {
                        DataContext = interaction.Input,
                        ShowInTaskbar = false,
                    }.ShowDialog(this);
                    interaction.SetOutput(Unit.Default);
                }).DisposeWith(disposables);

                ViewModel?.BuildImportInteraction.RegisterHandler(async interaction =>
                {
                    var result = await new BuildImportWindow { DataContext = interaction.Input }.ShowDialog<Build?>(this);
                    interaction.SetOutput(result);
                }).DisposeWith(disposables);

                ViewModel?.MessageBoxInteraction.RegisterHandler(async interaction =>
                {
                    if (interaction.Input.autoSize)
                    {
                        await MessageBox.Show(this, interaction.Input.text, interaction.Input.title, MessageBox.MessageBoxButtons.Ok, MessageBox.MessageBoxIcon.Error, 500, sizeToContent: SizeToContent.Height);
                    }
                    else
                    {
                        await MessageBox.Show(this, interaction.Input.text, interaction.Input.title, MessageBox.MessageBoxButtons.Ok, MessageBox.MessageBoxIcon.Error);
                    }

                    interaction.SetOutput(Unit.Default);
                }).DisposeWith(disposables);
            });
        }

        private void Control_PointerReleased(object? sender, Avalonia.Input.PointerReleasedEventArgs e)
        {
            var window = new TestWindow();

            // window.DataContext = new TestWindowViewModel();
            window.Show();
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            this.HandleAndCheckScaling();
        }

        private void LoadBuild(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            ViewModel?.LoadBuild();
        }

        private async void OnUpdateCompletedIconClicked(object? sender, PointerPressedEventArgs e)
        {
            var result = await UpdateHelpers.ShowUpdateRestartDialog(this);
            if (result == MessageBox.MessageBoxResult.Yes)
            {
                Logging.Logger.LogInformation("User decided to restart after update");
                if (OperatingSystem.IsWindows())
                {
                    UpdateManager.RestartApp();
                }
            }
        }
    }
}
