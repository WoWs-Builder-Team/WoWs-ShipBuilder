using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using Splat;
using WoWsShipBuilder.Desktop.Extensions;
using WoWsShipBuilder.Desktop.UserControls;
using WoWsShipBuilder.Infrastructure;
using WoWsShipBuilder.Infrastructure.Localization.Resources;

namespace WoWsShipBuilder.Desktop.Views
{
    public class SplashScreen : ReactiveWindow<SplashScreenViewModel>
    {
        public SplashScreen()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            this.WhenActivated(_ =>
            {
                var vm = ViewModel ?? Locator.Current.GetRequiredService<SplashScreenViewModel>();

                Task.Run(async () =>
                {
                    var minimumRuntime = Task.Delay(1500);
                    try
                    {
                        await vm.VersionCheck(throwOnException: true);
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

                    await minimumRuntime;
                    var navigationService = Locator.Current.GetRequiredService<INavigationService>();

                    await Dispatcher.UIThread.InvokeAsync(() => { navigationService.OpenStartMenu(true); });
                });
            });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            ViewModel = Locator.Current.GetRequiredService<SplashScreenViewModel>();
        }
    }
}
