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
        }

        private void LoadBuild(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var dc = (StartMenuViewModel)DataContext!;
            dc.LoadBuild(null!);
        }
    }
}
