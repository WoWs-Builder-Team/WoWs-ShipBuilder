using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Squirrel;
using WoWsShipBuilder.Core;
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
            base.OnFrameworkInitializationCompleted();
            Version versionDetails = Assembly.GetExecutingAssembly().GetName().Version!;
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                AppSettingsHelper.LoadSettings();
                desktop.Exit += OnExit;
                SplashScreen splashScreen = new(versionDetails);
                splashScreen.Show();

                Logging.Logger.Info("Before updatecheck");

                // SquirrelAwareApp.HandleEvents(onAppUpdate: OnAppUpdate);
                Task.Run(async () =>
                {
                    await UpdateCheck();
                    Logging.Logger.Info("After updatecheck");
                });
            }
        }

        private void OnAppUpdate(Version obj)
        {
            Logging.Logger.Info("On App update triggered.");
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
                await updateManager.UpdateApp();
            }
            catch (Exception e)
            {
                Logging.Logger.Error(e);
                throw;
            }
        }
    }
}
