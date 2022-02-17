using System;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Splat;
using Squirrel;
using WoWsShipBuilder.Core;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Settings;
using WoWsShipBuilder.UI.Settings;
using WoWsShipBuilder.UI.UserControls;
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
                Locator.CurrentMutable.RegisterConstant(new FileSystem(), typeof(IFileSystem));
                AppSettingsHelper.LoadSettings();
                Logging.InitializeLogging(ApplicationSettings.ApplicationOptions.SentryDsn, true);
                desktop.Exit += OnExit;
                desktop.MainWindow = new SplashScreen(versionDetails);
                Logging.Logger.Info($"AutoUpdate Enabled: {AppData.Settings.AutoUpdateEnabled}");

                if (AppData.Settings.AutoUpdateEnabled)
                {
#if !DEBUG || UPDATE_TEST
                    Task.Run(async () =>
                    {
                        if (OperatingSystem.IsWindows())
                        {
                            await UpdateCheck();
                            Logging.Logger.Info("Finished updatecheck");
                        }
                        else
                        {
                            Logging.Logger.Warn("Skipped updatecheck");
                        }
                    });
#endif
                }
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            Logging.Logger.Info("Closing app, saving setting and builds");
            AppSettingsHelper.SaveSettings();
            AppDataHelper.Instance.SaveBuilds();
            Logging.Logger.Info("Exiting...");
            Logging.Logger.Info(new string('-', 30));
        }

        [SupportedOSPlatform("windows")]
        private async Task UpdateCheck()
        {
            Logging.Logger.Info($"Current version: {Assembly.GetExecutingAssembly().GetName().Version}");

            using UpdateManager updateManager = new GithubUpdateManager("https://github.com/WoWs-Builder-Team/WoWs-ShipBuilder");
            Logging.Logger.Info("Update manager initialized.");
            try
            {
                // Can throw a null-reference-exception, no idea why.
                var release = await updateManager.UpdateApp();
                if (release == null)
                {
                    Logging.Logger.Info("No app update found.");
                    return;
                }

                Logging.Logger.Info($"App updated to version {release.Version}");
                var result = await Dispatcher.UIThread.InvokeAsync(async () => await MessageBox.Show(
                    null,
                    $"App was updated to version {release.Version}, do you want to restart to apply?",
                    "App Updated",
                    MessageBox.MessageBoxButtons.YesNo,
                    MessageBox.MessageBoxIcon.Question));
                if (result.Equals(MessageBox.MessageBoxResult.Yes))
                {
                    Logging.Logger.Info("User decided to restart after update.");
                    if (OperatingSystem.IsWindows())
                    {
                        UpdateManager.RestartApp();
                    }
                }
            }
            catch (NullReferenceException)
            {
                Logging.Logger.Debug("NullReferenceException during app update.");
            }
            catch (Exception e)
            {
#if DEBUG
                Logging.Logger.Warn(e);
#else
                Logging.Logger.Error(e);
#endif
            }
        }
    }
}
