using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using WoWsShipBuilder.Desktop.Features.BlazorWebView;
using WoWsShipBuilder.Desktop.UserControls;
using WoWsShipBuilder.Infrastructure;
using WoWsShipBuilder.Infrastructure.Localization.Resources;

namespace WoWsShipBuilder.Desktop.Views
{
    public class SplashScreen : ReactiveWindow<SplashScreenViewModel>
    {
        public SplashScreen(IServiceProvider services)
        {
            InitializeComponent(services.GetRequiredService<SplashScreenViewModel>());
#if DEBUG
            this.AttachDevTools();
#endif
            this.WhenActivated(_ =>
            {
                var vm = ViewModel ?? services.GetRequiredService<SplashScreenViewModel>();

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

                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        var blazorWindow = new BlazorWindow();
                        blazorWindow.Show();

                        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                        {
                            desktop.MainWindow.Close();
                            desktop.MainWindow = blazorWindow;
                        }
                    });
                });
            });
        }

        private void InitializeComponent(SplashScreenViewModel viewModel)
        {
            AvaloniaXamlLoader.Load(this);
            ViewModel = viewModel;
        }
    }
}
