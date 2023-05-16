using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using Splat;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.Infrastructure;
using WoWsShipBuilder.UI.Extensions;
using WoWsShipBuilder.UI.Utilities;
using WoWsShipBuilder.UI.ViewModels.DispersionPlot;

namespace WoWsShipBuilder.UI.Views
{
    public class DispersionGraphsWindow : ScalableReactiveWindow<DispersionGraphViewModel>
    {
        public DispersionGraphsWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            this.WhenActivated(disposable =>
            {
                if (ViewModel != null)
                {
                    disposable(ViewModel.AddShipInteraction.RegisterHandler(async interaction =>
                    {
                        var selectionWindow = new ShipSelectionWindow(interaction.Input.MultiSelectionEnabled)
                        {
                            DataContext = interaction.Input,
                        };
                        var result = await selectionWindow.ShowDialog<List<ShipSummary>>(this);
                        interaction.SetOutput(result);
                    }));
                    disposable(ViewModel.ShellSelectionInteraction.RegisterHandler(async interaction =>
                    {
                        var valueSelectionWindow = new ValueSelectionWindow
                        {
                            DataContext = interaction.Input,
                        };
                        var result = await valueSelectionWindow.ShowDialog<string>(this);
                        interaction.SetOutput(result);
                    }));
                }
            });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            Opened += DispersionGraphsWindow_Opened;
            Closing += DispersionGraphsWindow_Closing;
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);

            var currentScreen = Screens.ScreenFromVisual(this);
            if (WindowScalingHelper.CheckScaling(currentScreen, this).Key)
            {
                WindowState = WindowState.Maximized;
            }
        }

        private void DispersionGraphsWindow_Closing(object? sender, EventArgs e)
        {
            if (Owner is MainWindow mainWindow)
            {
                mainWindow.ChildWindows.Remove(this);
            }
            else
            {
                Locator.Current.GetRequiredService<INavigationService>().OpenStartMenu();
            }
        }

        private void DispersionGraphsWindow_Opened(object? sender, EventArgs e)
        {
            if (Owner is MainWindow mainWindow)
            {
                mainWindow.ChildWindows.Add(this);
            }
        }
    }
}
