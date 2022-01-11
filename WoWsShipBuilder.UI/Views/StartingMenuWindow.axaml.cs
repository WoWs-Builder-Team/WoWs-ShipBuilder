using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
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
            var control = this.FindControl<Image>("Image");
#if DEBUG
            control.PointerReleased += Control_PointerReleased;
#else
            control.IsHitTestVisible = false;
#endif
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
