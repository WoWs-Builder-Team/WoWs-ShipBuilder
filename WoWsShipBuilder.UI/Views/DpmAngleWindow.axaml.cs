using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using WoWsShipBuilder.UI.ViewModels;

namespace WoWsShipBuilder.UI.Views
{
    public partial class DpmAngleWindow : Window
    {
        public DpmAngleWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            Opened += DpmAngleWindow_Opened;
            Closing += DpmAngleWindow_Closing;
        }

        private void DpmAngleWindow_Closing(object? sender, System.EventArgs e)
        {
            if (Owner is MainWindow mainWindow)
            {
                mainWindow.ChildWindows.Remove(this);
            }
        }

        private void DpmAngleWindow_Opened(object? sender, System.EventArgs e)
        {
            ((MainWindow)Owner).ChildWindows.Add(this);
        }
    }
}
