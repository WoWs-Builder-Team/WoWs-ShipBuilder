using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using WoWsShipBuilder.Core.BuildCreator;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.UI.Translations;
using WoWsShipBuilder.UI.UserControls;
using WoWsShipBuilder.UI.Utilities;
using WoWsShipBuilder.UI.ViewModels;

namespace WoWsShipBuilder.UI.Views
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
                    var selectionWindow = new ShipSelectionWindow(false);
                    var result = await selectionWindow.ShowDialog<List<ShipSummary>>(this);
                    interaction.SetOutput(result);
                }).DisposeWith(disposables);

                ViewModel?.CloseInteraction.RegisterHandler(interaction =>
                {
                    Close();
                    interaction.SetOutput(Unit.Default);
                }).DisposeWith(disposables);

                ViewModel?.ShowSettingsInteraction.RegisterHandler(async interaction =>
                {
                    await new SettingsWindow
                    {
                        DataContext = interaction.Input,
                        ShowInTaskbar = false,
                    }.ShowDialog(this);
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
            window.DataContext = new TestWindowViewModel();
            window.Show();
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            this.HandleAndCheckScaling();
        }

        private void LoadBuild(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var dc = (StartMenuViewModel)DataContext!;
            dc.LoadBuild();
        }
    }
}
