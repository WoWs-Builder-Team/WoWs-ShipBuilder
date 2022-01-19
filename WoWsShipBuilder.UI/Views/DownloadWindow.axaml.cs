using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using WoWsShipBuilder.Core;
using WoWsShipBuilder.UI.ViewModels;

namespace WoWsShipBuilder.UI.Views
{
    public class DownloadWindow : Window
    {
        public DownloadWindow()
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

        protected override void OnInitialized()
        {
            base.OnInitialized();
            if (Design.IsDesignMode)
            {
                return;
            }

            Task.Run(async () =>
            {
                var viewmodel = new SplashScreenViewModel();
                try
                {
                    await viewmodel.VersionCheck(true);
                }
                catch (Exception e)
                {
                    Logging.Logger.Error(e, "Encountered unexpected error during data download.");
                }

                await Dispatcher.UIThread.InvokeAsync(Close);
            });
        }
    }
}
