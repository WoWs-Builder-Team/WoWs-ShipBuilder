using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using WoWsShipBuilder.Core;
using WoWsShipBuilder.UI.Settings;
using WoWsShipBuilder.UI.Updater;
using WoWsShipBuilder.UI.ViewModels;

namespace WoWsShipBuilder.UI.Views
{
    public class SplashScreen : Window
    {
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
                bool isUpdating = await ApplicationVersionCheck();
                if (!isUpdating)
                {
                    await dataContext.VersionCheck();
                }

                await minimumRuntime;

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    var startWindow = new StartingMenuWindow();
                    startWindow.DataContext = new StartMenuViewModel(startWindow);
                    if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                    {
                        desktop.MainWindow = startWindow;
                    }

                    startWindow.Show();
                    Close();
                });
            });
        }

        private async Task<bool> ApplicationVersionCheck()
        {
            dataContext.DownloadInfo = "appVersionCheck";
            var logger = Logging.GetLogger("UpdateCheck");
            logger.Info($"Current application version: {currentVersion}");
            logger.Info("Checking for updates...");
            GithubApiResponse? latestVersion = await ApplicationUpdater.GetLatestVersionNumber();
            bool versionParseSuccessful = Version.TryParse(Regex.Replace(latestVersion?.TagName ?? "0.0.0", "[^0-9.]", ""), out Version? newVersion);
            if (!versionParseSuccessful)
            {
                logger.Warn("Unable to parse the version of the new release.");
            }

            if (newVersion != null && newVersion > currentVersion)
            {
                if (ApplicationSettings.AutoUpdateEnabled)
                {
                    logger.Info("New version avaialble");

                    // UpdateWindow updateWin = new UpdateWindow("New Update found: " + latestVersion.TagName, latestVersion.Body,
                    //     "Do you want to update now?", true);
                    // updateWin.Owner = this;
                    // var result = (bool)updateWin.ShowDialog();
                    //
                    // //var result = MessageBox.Show(latestVersion.Body + "\nDo you want to update now?", "New Update found: " + latestVersion.TagName, MessageBoxButton.YesNo, MessageBoxImage.Question);
                    // if (result)
                    // {
                    //     string fileUrl = latestVersion.Assets[0].BrowserDownloadUrl;
                    //     DownloadingWindow win = new DownloadingWindow(fileUrl);
                    //     win.Owner = this;
                    //     win.ShowDialog();
                    // }
                }
                else
                {
                    logger.Info("New version available but auto-update is disabled.");

                    // UpdateWindow updateWin = new UpdateWindow("New Update found: " + latestVersion.TagName, latestVersion.Body,
                    //     "Do you want to open the page for the download?", true);
                    // updateWin.Owner = this;
                    // var result = (bool)updateWin.ShowDialog();
                    //
                    // //var result = MessageBox.Show(latestVersion.Body + "\nDo you want to open the page for the download?", "New Update found: " + latestVersion.TagName, MessageBoxButton.YesNo, MessageBoxImage.Question);
                    // if (result)
                    // {
                    //     Process.Start(latestVersion.HtmlUrl);
                    // }
                }
            }
            else
            {
                logger.Info("No updates found.");
            }

            return false;
        }
    }
}
