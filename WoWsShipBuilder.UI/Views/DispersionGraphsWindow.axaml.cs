using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using WoWsShipBuilder.UI.ViewModels;

namespace WoWsShipBuilder.UI.Views
{
    public partial class DispersionGraphsWindow : Window
    {
        public DispersionGraphsWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            Opened += DispersionGraphsWindow_Opened;
            Closing += DispersionGraphsWindow_Closing;
        }

        private void DispersionGraphsWindow_Closing(object? sender, System.EventArgs e)
        {
            ((MainWindowViewModel)Owner.DataContext!).ChildrenWindows.Remove(this);
        }

        private void DispersionGraphsWindow_Opened(object? sender, System.EventArgs e)
        {
            ((MainWindowViewModel)Owner.DataContext!).ChildrenWindows.Add(this);
        }
    }
}
