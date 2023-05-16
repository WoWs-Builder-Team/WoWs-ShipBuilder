using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using Splat;
using WoWsShipBuilder.Infrastructure;
using WoWsShipBuilder.Infrastructure.Localization.Resources;
using WoWsShipBuilder.UI.Extensions;
using WoWsShipBuilder.UI.UserControls;

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
                var viewmodel = Locator.Current.GetRequiredService<SplashScreenViewModel>();
                try
                {
                    await viewmodel.VersionCheck(true, true);
                }
                catch (Exception e)
                {
                    Logging.Logger.LogError(e, "Encountered unexpected error during data download");
                    await Dispatcher.UIThread.InvokeAsync(async () =>
                        await MessageBox.Show(
                            this,
                            Translation.DataUpdate_ErrorDescription,
                            Translation.DataUpdate_ErrorTitle,
                            MessageBox.MessageBoxButtons.Ok,
                            MessageBox.MessageBoxIcon.Error,
                            width: 500,
                            sizeToContent: SizeToContent.Height));
                }

                await Dispatcher.UIThread.InvokeAsync(Close);
            });
        }
    }
}
