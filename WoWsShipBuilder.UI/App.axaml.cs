using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Squirrel;
using WoWsShipBuilder.Core;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Settings;
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
            base.OnFrameworkInitializationCompleted();
            Version versionDetails = Assembly.GetExecutingAssembly().GetName().Version!;
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                AppSettingsHelper.LoadSettings();
                desktop.Exit += OnExit;
                SplashScreen splashScreen = new(versionDetails);
                splashScreen.Show();

                Logging.Logger.Info($"AutoUpdate Enabled: {AppData.Settings.AutoUpdateEnabled}");

                if (AppData.Settings.AutoUpdateEnabled)
                {
                    Task.Run(async () =>
                    {
                        await UpdateCheck();
                        Logging.Logger.Info("After updatecheck");
                    });
                }
            }
        }

        private void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            AppSettingsHelper.SaveSettings();
            AppDataHelper.Instance.SaveBuilds();
        }

        private async Task UpdateCheck()
        {
            Logging.Logger.Info($"Current version: {Assembly.GetExecutingAssembly().GetName().Version}");
            using UpdateManager updateManager = await UpdateManager.GitHubUpdateManager("https://github.com/WoWs-Builder-Team/WoWs-ShipBuilder");
            try
            {
                var release = await updateManager.UpdateApp();
                Logging.Logger.Info($"App updated to version {release.Version}");
                var result = await Dispatcher.UIThread.InvokeAsync(async () => await MessageBox.Show(null, "App was updated, do you want to restart to apply?", "App Updated", MessageBox.MessageBoxButtons.YesNo, MessageBox.MessageBoxIcon.Question));
                if (result.Equals(MessageBox.MessageBoxResult.Yes))
                {
                    Logging.Logger.Info("User decided to restart after update.");
                    UpdateManager.RestartApp();
                }
            }
            catch (Exception e)
            {
                Logging.Logger.Error(e);
                throw;
            }
        }
    }
}
