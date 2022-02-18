using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Mixins;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.UI.Translations;
using WoWsShipBuilder.UI.UserControls;
using WoWsShipBuilder.UI.Utilities;
using WoWsShipBuilder.UI.ViewModels;

namespace WoWsShipBuilder.UI.Views
{
    public partial class MainWindow : ScalableReactiveWindow<MainWindowViewModel>
    {
        private static readonly Regex Regex = new("\\D+");

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
                        DataContext = new ShipSelectionWindowViewModel(false),
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
                    if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
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
            var baseXp = this.FindControl<TextBox>("BaseXpInput");
            baseXp.AddHandler(TextInputEvent, AcceptOnlyNumber, RoutingStrategies.Tunnel);
            var xpInput = this.FindControl<TextBox>("XpInput");
            xpInput.AddHandler(TextInputEvent, AcceptOnlyNumber, RoutingStrategies.Tunnel);
            var commanderXpInput = this.FindControl<TextBox>("CommanderXpInput");
            commanderXpInput.AddHandler(TextInputEvent, AcceptOnlyNumber, RoutingStrategies.Tunnel);
            var freeXpInput = this.FindControl<TextBox>("FreeXpInput");
            freeXpInput.AddHandler(TextInputEvent, AcceptOnlyNumber, RoutingStrategies.Tunnel);
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            this.HandleAndCheckScaling();
        }

        public void AcceptOnlyNumber(object? sender, TextInputEventArgs e)
        {
            var text = e.Text!;
            if (Regex.IsMatch(text))
            {
                e.Handled = true;
            }
        }

        public void OnClickChangeShipNext(object sender, PointerReleasedEventArgs e)
        {
            var image = (Image)sender;
            string? shipIndex = image.Name;
            ViewModel?.LoadShipFromIndexCommand.Execute(shipIndex);
        }

        public void OnClickChangeShipPrevious(object sender, PointerReleasedEventArgs e)
        {
            var shipIndex = ViewModel?.PreviousShipIndex;
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
