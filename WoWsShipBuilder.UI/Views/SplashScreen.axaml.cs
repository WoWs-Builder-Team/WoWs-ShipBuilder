using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using ReactiveUI;
using Splat;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.UI.Extensions;
using WoWsShipBuilder.UI.ViewModels;
using WoWsShipBuilder.ViewModels.Other;

namespace WoWsShipBuilder.UI.Views
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
                var vm = ViewModel;
                if (vm != null)
                {
                    Task.Run(async () =>
                    {
                        var minimumRuntime = Task.Delay(1500);
                        await vm.VersionCheck();
                        await minimumRuntime;
                        var navigationService = Locator.Current.GetServiceSafe<INavigationService>();

                        await Dispatcher.UIThread.InvokeAsync(() => { navigationService.OpenStartMenu(true); });
                    });
                }
            });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            DataContext = Locator.Current.GetServiceSafe<SplashScreenViewModel>();
        }

        protected override void OnInitialized()
        {
            if (Design.IsDesignMode)
            {
                return;
            }
        }
    }
}
