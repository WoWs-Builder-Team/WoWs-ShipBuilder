using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using WoWsShipBuilder.UI.ViewModels;

namespace WoWsShipBuilder.UI.Views
{
    public partial class StartingMenuWindow : Window
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
#if DEBUG
            var control = this.FindControl<Image>("Image");
            control.PointerReleased += Control_PointerReleased;
#endif
        }

        private void Control_PointerReleased(object? sender, Avalonia.Input.PointerReleasedEventArgs e)
        {
            var window = new TestWindow();
            window.DataContext = new TestWindowViewModel();
            window.Show();
        }

        private void LoadBuild(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var dc = (StartMenuViewModel)DataContext!;
            dc.LoadBuild(null!);
        }
    }
}
