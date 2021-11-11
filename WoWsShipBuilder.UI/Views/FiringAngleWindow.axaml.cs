using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using WoWsShipBuilder.UI.ViewModels;

namespace WoWsShipBuilder.UI.Views
{
    public partial class FiringAngleWindow : Window
    {
        public FiringAngleWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            Opened += FiringAngleWindow_Opened;
            Closing += FiringAngleWindow_Closing;
        }

        private void FiringAngleWindow_Closing(object? sender, System.EventArgs e)
        {
            ((MainWindowViewModel)Owner.DataContext!).ChildrenWindows.Remove(this);
        }

        private void FiringAngleWindow_Opened(object? sender, System.EventArgs e)
        {
            ((MainWindowViewModel)Owner.DataContext!).ChildrenWindows.Add(this);
        }

        private void OpenDiscord(object? sender, Avalonia.Input.PointerReleasedEventArgs e)
        {
            string url = "https://discord.gg/C8EaepZJDY";

            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true,
            });
        }

        private void OpenGithub(object? sender, Avalonia.Input.PointerReleasedEventArgs e)
        {
            string url = "https://github.com/WoWs-Builder-Team/WoWs-ShipBuilder/issues";

            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true,
            });
        }
    }
}
