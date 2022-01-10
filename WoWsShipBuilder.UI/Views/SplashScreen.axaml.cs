using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using NLog;
using WoWsShipBuilder.Core;
using WoWsShipBuilder.UI.ViewModels;

namespace WoWsShipBuilder.UI.Views
{
    public class SplashScreen : Window
    {
        private static readonly Logger Logger = Logging.GetLogger("SplashScreen");

        private readonly SplashScreenViewModel dataContext;

        private readonly Version currentVersion;

        public SplashScreen()
            : this(new Version(0, 0, 1))
        {
        }

        public SplashScreen(Version currentVersion)
        {
            this.currentVersion = currentVersion;
            dataContext = new SplashScreenViewModel();
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            DataContext = dataContext;
        }

        protected override void OnInitialized()
        {
            if (Design.IsDesignMode)
            {
                return;
            }

            Task.Run(async () =>
            {
                Task minimumRuntime = Task.Delay(1500);
                Logger.Debug("Checking gamedata versions...");
                try
                {
                    await dataContext.VersionCheck();
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Encountered unexpected exception during version check.");
                }

                Logger.Debug("Startup tasks completed. Launching main window.");

                await minimumRuntime;

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    var startWindow = new StartingMenuWindow();
                    startWindow.DataContext = new StartMenuViewModel(startWindow);
                    if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                    {
                        desktop.MainWindow = startWindow;
                    }

                    startWindow.Show();
                    Close();
                });
            });
        }
    }
}
