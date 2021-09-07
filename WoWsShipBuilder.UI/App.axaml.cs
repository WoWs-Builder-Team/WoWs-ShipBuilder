using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using WoWsShipBuilder.UI.Settings;
using WoWsShipBuilder.UI.Updater;
using WoWsShipBuilder.UI.ViewModels;
using WoWsShipBuilder.UI.Views;

namespace WoWsShipBuilder.UI
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            string version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0-alpha";
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                if (desktop.Args.Length > 0 && desktop.Args[0] == "-update")
                {
                    string programPath = desktop.Args[1];
                    string tempPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WoWsShipBuilder", "tempUpdate");
                    DirectoryCopy(tempPath, programPath, true);
                    string programExe = Path.Combine(programPath, "WoWsShipBuilder.exe");
                    Process.Start(programExe);
                    desktop.Shutdown();
                    return;
                }

                StartingMenuWindow win = new();
                win.DataContext = new StartMenuViewModel(win);
                desktop.MainWindow = win;

                Dispatcher.UIThread.InvokeAsync(RunUpdateCheck(version), DispatcherPriority.Background);

                base.OnFrameworkInitializationCompleted();
            }
        }

        private static Action RunUpdateCheck(string version)
        {
            return async () =>
            {
                var logger = Logging.GetLogger("UpdateCheck");
                logger.Info($"Current application version: {version}");
                logger.Info("Checking for updates...");
                GithubApiResponse? latestVersion = await ApplicationUpdater.GetLatestVersionNumber();
                if (latestVersion != null && latestVersion.TagName != version)
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
            };
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subDirectory in dirs)
                {
                    string tempPath = Path.Combine(destDirName, subDirectory.Name);
                    DirectoryCopy(subDirectory.FullName, tempPath, copySubDirs);
                }
            }
        }
    }
}
