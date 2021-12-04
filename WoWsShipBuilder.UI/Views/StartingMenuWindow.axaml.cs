using System;
using Avalonia;
using Avalonia.Markup.Xaml;
using WoWsShipBuilder.UI.Extensions;
using WoWsShipBuilder.UI.Utilities;
using WoWsShipBuilder.UI.ViewModels;

namespace WoWsShipBuilder.UI.Views
{
    public partial class StartingMenuWindow : ScalableWindow
    {
        public StartingMenuWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            this.HandleAndCheckScaling();
        }

        private void LoadBuild(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var dc = (StartMenuViewModel)DataContext!;
            dc.LoadBuild(null!);
        }
    }
}
