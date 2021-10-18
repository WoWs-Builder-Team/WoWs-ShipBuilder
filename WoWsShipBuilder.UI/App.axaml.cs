using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Settings;
using WoWsShipBuilder.UI.Views;

namespace WoWsShipBuilder.UI
{
    [SuppressMessage("System.IO.Abstractions", "IO0003", Justification = "This class is never tested.")]
    [SuppressMessage("System.IO.Abstractions", "IO0006", Justification = "This class is never tested.")]
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {   
            Version versionDetails = Assembly.GetExecutingAssembly().GetName().Version!;
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                if (desktop.Args.Length > 0 && desktop.Args[0] == "-update")
                {
                    string programPath = desktop.Args[1];
                    string tempPath = Path.Combine(AppDataHelper.Instance.AppDataDirectory, "tempUpdate");
                    DirectoryCopy(tempPath, programPath, true);
                    string programExe = Path.Combine(programPath, "WoWsShipBuilder.exe");
                    Process.Start(programExe);
                    desktop.Shutdown();
                    return;
                }

                AppSettingsHelper.LoadSettings();
                desktop.Exit += OnExit;
                SplashScreen splashScreen = new(versionDetails);
                splashScreen.Show();
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            AppSettingsHelper.SaveSettings();
        }

        private void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
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
