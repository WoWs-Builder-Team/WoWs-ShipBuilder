using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NLog;
using Splat;
using Squirrel;
using WoWsShipBuilder.Desktop.Features.MessageBox;
using WoWsShipBuilder.Desktop.Features.SplashScreen;
using WoWsShipBuilder.Desktop.Infrastructure;
using WoWsShipBuilder.Features.Settings;
using WoWsShipBuilder.Infrastructure;
using WoWsShipBuilder.Infrastructure.Data;
using WoWsShipBuilder.Infrastructure.Localization;
using WoWsShipBuilder.Infrastructure.Localization.Resources;

namespace WoWsShipBuilder.Desktop
{
    [SuppressMessage("System.IO.Abstractions", "IO0003", Justification = "This class is never tested.")]
    [SuppressMessage("System.IO.Abstractions", "IO0006", Justification = "This class is never tested.")]
    public class App : Application
    {
        private readonly ILogger<App> logger = NullLogger<App>.Instance;
        private readonly IServiceProvider services = default!;

        public App()
        {
            ModeDetector.OverrideModeDetector(new CustomModeDetector());
        }

        public IServiceProvider Services
        {
            get => services;
            init
            {
                services = value;
                logger = services.GetRequiredService<ILogger<App>>();
            }
        }

        public static async Task<MessageBox.MessageBoxResult> ShowUpdateRestartDialog(Window? parent, ILocalizer localizer)
        {
            return await Dispatcher.UIThread.InvokeAsync(async () => await MessageBox.Show(
                parent,
                localizer.GetAppLocalization(nameof(Translation.UpdateMessageBox_Description)).Localization,
                localizer.GetAppLocalization(nameof(Translation.UpdateMessageBox_Title)).Localization,
                MessageBox.MessageBoxButtons.YesNo,
                MessageBox.MessageBoxIcon.Question));
        }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // container = SetupDependencyInjection(LogManager.Configuration);
                Logging.Initialize(Services.GetRequiredService<ILoggerFactory>());
                InitializeSettings();
                var settings = Services.GetRequiredService<AppSettings>();

                // AppSettingsHelper.LoadSettings(Services);
                LogManager.ReconfigExistingLoggers();

                desktop.Exit += OnExit;
                desktop.MainWindow = new SplashScreen(Services);
                logger.LogInformation("AutoUpdate Enabled: {SettingsAutoUpdateEnabled}", settings.AutoUpdateEnabled);

                if (settings.AutoUpdateEnabled)
                {
                    Task.Run(async () =>
                    {
                        if (OperatingSystem.IsWindows())
                        {
                            await UpdateCheck(Services.GetRequiredService<AppNotificationService>());
                            logger.LogInformation("Finished updatecheck");
                        }
                        else
                        {
                            logger.LogInformation("Skipped updatecheck");
                        }
                    });
                }
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void InitializeSettings()
        {
            var settingsAccessor = (DesktopSettingsAccessor)services.GetRequiredService<ISettingsAccessor>();
            var settings = settingsAccessor.LoadSettingsSync();
            settings ??= new();
            settings.WebAppSettings ??= new();

            logger.LogDebug("Updating app settings with settings read from file...");
            var appSettings = services.GetRequiredService<AppSettings>();
            appSettings.UpdateFromSettings(settings);
            AppData.IsInitialized = true;
            Thread.CurrentThread.CurrentCulture = appSettings.SelectedLanguage.CultureInfo;
            Thread.CurrentThread.CurrentUICulture = appSettings.SelectedLanguage.CultureInfo;
            logger.LogDebug("Settings initialization complete");
        }

        private void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            logger.LogInformation("Closing app, saving setting and builds");
            var settingsAccessor = (DesktopSettingsAccessor)Services.GetRequiredService<ISettingsAccessor>();
            settingsAccessor.SaveSettingsSync(Services.GetRequiredService<AppSettings>());
            Services.GetRequiredService<IUserDataService>().SaveBuilds();
            logger.LogInformation("Exiting...");
            logger.LogInformation("------------------------------");
        }

        [SupportedOSPlatform("windows")]
        private async Task UpdateCheck(AppNotificationService notificationService)
        {
            logger.LogInformation("Current version: {Version}", Assembly.GetExecutingAssembly().GetName().Version);

            using UpdateManager updateManager = new GithubUpdateManager("https://github.com/WoWs-Builder-Team/WoWs-ShipBuilder");
            if (!updateManager.IsInstalledApp)
            {
                logger.LogInformation("No update.exe found, aborting update check");
                return;
            }

            logger.LogInformation("Update manager initialized");
            try
            {
                // Can throw a null-reference-exception, no idea why.
                var updateInfo = await updateManager.CheckForUpdate();
                if (!updateInfo.ReleasesToApply.Any())
                {
                    logger.LogInformation("No app update found");
                    return;
                }

                await notificationService.NotifyAppUpdateStart();
                var release = await updateManager.UpdateApp();
                if (release == null)
                {
                    logger.LogInformation("No app update found");
                    return;
                }

                logger.LogInformation("App updated to version {ReleaseVersion}", release.Version);
                await notificationService.NotifyAppUpdateComplete();
                var result = await ShowUpdateRestartDialog((ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow, Services.GetRequiredService<ILocalizer>());
                if (result.Equals(MessageBox.MessageBoxResult.Yes))
                {
                    logger.LogInformation("User decided to restart after update");
                    if (OperatingSystem.IsWindows())
                    {
                        UpdateManager.RestartApp();
                    }
                }
            }
            catch (NullReferenceException)
            {
                logger.LogDebug("NullReferenceException during app update");
            }
            catch (Exception e)
            {
#if DEBUG
                logger.LogWarning(e, "Exception during app update");
#else
                logger.LogError(e, "Exception during app update");
#endif
                await notificationService.NotifyAppUpdateError(nameof(Translation.NotificationService_ErrorMessage));
            }
        }

        private sealed class CustomModeDetector : IModeDetector
        {
            public bool? InUnitTestRunner() => false;
        }
    }
}
