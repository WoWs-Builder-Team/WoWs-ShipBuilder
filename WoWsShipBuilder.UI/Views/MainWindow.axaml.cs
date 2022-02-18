using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Reactive;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Mixins;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using Splat;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.UI.Extensions;
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

                ViewModel?.CloseInteraction.RegisterHandler(interaction =>
                {
                    foreach (var childWindow in ChildWindows)
                    {
                        childWindow.Close();
                    }

                    if (interaction.Input)
                    {
                        Close();
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
            var dc = DataContext as MainWindowViewModel;
            var image = sender as Image;
            var shipIndex = image!.Name;
            var ship = AppData.ShipDictionary![shipIndex!];
            var prevShipIndex = AppData.ShipSummaryList!.First(x => x.Index == shipIndex).PrevShipIndex;
            var nextShipIndex = AppData.ShipSummaryList!.First(x => x.Index == shipIndex).NextShipsIndex;
            DataContext = new MainWindowViewModel(Locator.Current.GetServiceSafe<IFileSystem>(), ship, prevShipIndex, nextShipIndex, contentScaling: dc!.ContentScaling);
        }

        public void OnClickChangeShipPrevious(object sender, PointerReleasedEventArgs e)
        {
            var dc = DataContext as MainWindowViewModel;
            var ship = AppData.ShipDictionary![dc!.PreviousShipIndex!];
            var prevShipIndex = AppData.ShipSummaryList!.First(x => x.Index == dc.PreviousShipIndex!).PrevShipIndex;
            var nextShipIndex = AppData.ShipSummaryList!.First(x => x.Index == dc.PreviousShipIndex!).NextShipsIndex;
            DataContext = new MainWindowViewModel(Locator.Current.GetServiceSafe<IFileSystem>(), ship, prevShipIndex, nextShipIndex, contentScaling: dc.ContentScaling);
        }
    }
}
