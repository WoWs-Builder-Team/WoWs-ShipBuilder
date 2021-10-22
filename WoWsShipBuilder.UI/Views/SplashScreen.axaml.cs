using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using NLog;
using WoWsShipBuilder.Core;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.UI.Updater;
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
                Logger.Debug("Checking application version...");
                bool isUpdating = await ApplicationVersionCheck();
                if (!isUpdating)
                {
                    Logger.Debug("Checking gamedata versions...");
                    await dataContext.VersionCheck();
                }

                Logger.Debug("Updating localization settings...");
                Localizer.Instance.UpdateLanguage(AppData.Settings.Locale);
                Logger.Debug("Startup tasks completed. Launching main window.");

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
            Logger.Info($"Last version check was at: {AppData.Settings.LastVersionUpdateCheck}");
            var now = DateTime.Now;
            if ((now - AppData.Settings.LastVersionUpdateCheck).TotalHours > 4 || AppData.IsDebug)
            {
                dataContext.DownloadInfo = "appVersionCheck";
                AppData.Settings.LastVersionUpdateCheck = DateTime.Now;
                Logger.Info($"Current application version: {currentVersion}");
                Logger.Info("Checking for updates...");
                GithubApiResponse? latestVersion = await ApplicationUpdater.GetLatestVersionNumber();
                bool versionParseSuccessful = Version.TryParse(Regex.Replace(latestVersion?.TagName ?? "0.0.0", "[^0-9.]", ""), out Version? newVersion);
                if (!versionParseSuccessful)
                {
                    Logger.Warn("Unable to parse the version of the new release.");
                }

                if (newVersion != null && newVersion > currentVersion)
                {
                    if (AppData.Settings!.AutoUpdateEnabled)
                    {
                        Logger.Info("New version available");

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
                        Logger.Info("New version available but auto-update is disabled.");

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
                    Logger.Info("No updates found.");
                }
            }

            return false;
        }
    }
}
