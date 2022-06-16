using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Mixins;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Translations;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.UI.Settings;
using WoWsShipBuilder.UI.UserControls;
using WoWsShipBuilder.UI.Utilities;
using WoWsShipBuilder.UI.ViewModels.ShipVm;
using WoWsShipBuilder.ViewModels.Helper;

namespace WoWsShipBuilder.UI.Views
{
    public partial class MainWindow : ScalableReactiveWindow<MainWindowViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            this.WhenActivated(disposables =>
            {
                ViewModel?.BuildCreationInteraction.RegisterHandler(async interaction =>
                {
                    var result = await new BuildCreationWindow
                    {
                        ShowInTaskbar = false,
                        DataContext = interaction.Input,
                    }.ShowDialog<BuildCreationResult?>(this);
                    interaction.SetOutput(result);
                }).DisposeWith(disposables);

                ViewModel?.CloseChildrenInteraction.RegisterHandler(interaction =>
                {
                    foreach (var childWindow in ChildWindows)
                    {
                        childWindow.Close();
                    }

                    interaction.SetOutput(Unit.Default);
                }).DisposeWith(disposables);

                ViewModel?.SelectNewShipInteraction.RegisterHandler(async interaction =>
                {
                    var result = await new ShipSelectionWindow
                    {
                        DataContext = new ShipSelectionWindowViewModel(false, await ShipSelectionWindowViewModel.LoadParamsAsync(DesktopAppDataService.Instance, AppSettingsHelper.Settings, AppSettingsHelper.LocalizerInstance)),
                    }.ShowDialog<List<ShipSummary>?>(this);
                    interaction.SetOutput(result);
                }).DisposeWith(disposables);

                ViewModel?.BuildCreatedInteraction.RegisterHandler(async interaction =>
                {
                    await MessageBox.Show(this, interaction.Input, Translation.BuildCreationWindow_BuildSaved, MessageBox.MessageBoxButtons.Ok, MessageBox.MessageBoxIcon.Info);
                    interaction.SetOutput(Unit.Default);
                }).DisposeWith(disposables);

                ViewModel?.OpenStartMenuInteraction.RegisterHandler(interaction =>
                {
                    StartMenuWindow win = new()
                    {
                        DataContext = interaction.Input,
                    };
                    if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                    {
                        desktop.MainWindow = win;
                    }

                    win.Show();
                    interaction.SetOutput(Unit.Default);
                }).DisposeWith(disposables);
            });
        }

        public List<Window> ChildWindows { get; } = new();

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            this.HandleAndCheckScaling();
        }

        public void OnClickChangeShipNext(object sender, PointerReleasedEventArgs e)
        {
            var image = (Image)sender;
            string? shipIndex = image.Name;
            ViewModel?.LoadShipFromIndexCommand.Execute(shipIndex);
        }

        public void OnClickChangeShipPrevious(object sender, PointerReleasedEventArgs e)
        {
            string? shipIndex = ViewModel?.PreviousShip?.Index;
            ViewModel?.LoadShipFromIndexCommand.Execute(shipIndex);
        }

        private void MainWindow_OnClosed(object? sender, EventArgs e)
        {
            foreach (var childWindow in ChildWindows.ToList())
            {
                childWindow.Close();
                ChildWindows.Remove(childWindow);
            }
        }
    }
}
