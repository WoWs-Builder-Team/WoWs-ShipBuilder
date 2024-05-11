using System;
using System.Diagnostics.CodeAnalysis;
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
using Velopack;
using Velopack.Sources;
using WoWsShipBuilder.Desktop.Features.MessageBox;
using WoWsShipBuilder.Desktop.Features.SplashScreen;
using WoWsShipBuilder.Desktop.Infrastructure;
using WoWsShipBuilder.Features.Settings;
using WoWsShipBuilder.Infrastructure.ApplicationData;
using WoWsShipBuilder.Infrastructure.Localization;
using WoWsShipBuilder.Infrastructure.Localization.Resources;
using WoWsShipBuilder.Infrastructure.Utility;

namespace WoWsShipBuilder.Desktop;

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
        get => this.services;
        init
        {
            this.services = value;
            this.logger = this.services.GetRequiredService<ILogger<App>>();
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
        if (this.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            Logging.Initialize(this.Services.GetRequiredService<ILoggerFactory>());
            this.InitializeSettings();
            var settings = this.Services.GetRequiredService<AppSettings>();

            LogManager.ReconfigExistingLoggers();

            desktop.Exit += this.OnExit;
            desktop.MainWindow = new SplashScreen(this.Services);
            this.logger.LogInformation("AutoUpdate Enabled: {SettingsAutoUpdateEnabled}", settings.AutoUpdateEnabled);

            if (settings.AutoUpdateEnabled)
            {
                Task.Run(async () =>
                {
                    if (OperatingSystem.IsWindows())
                    {
                        await this.UpdateCheck(this.Services.GetRequiredService<AppNotificationService>());
                        this.logger.LogInformation("Finished updatecheck");
                    }
                    else
                    {
                        this.logger.LogInformation("Skipped updatecheck");
                    }
                });
            }
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void InitializeSettings()
    {
        var settingsAccessor = (DesktopSettingsAccessor)this.services.GetRequiredService<ISettingsAccessor>();
        var settings = settingsAccessor.LoadSettingsSync();
        settings ??= new();

        this.logger.LogDebug("Updating app settings with settings read from file...");
        var appSettings = this.services.GetRequiredService<AppSettings>();
        appSettings.UpdateFromSettings(settings);
        AppData.IsInitialized = true;
        Thread.CurrentThread.CurrentCulture = appSettings.SelectedLanguage.CultureInfo;
        Thread.CurrentThread.CurrentUICulture = appSettings.SelectedLanguage.CultureInfo;
        this.logger.LogDebug("Settings initialization complete");
    }

    private void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        this.logger.LogInformation("Closing app, saving setting and builds");
        var settingsAccessor = (DesktopSettingsAccessor)this.Services.GetRequiredService<ISettingsAccessor>();
        settingsAccessor.SaveSettingsSync(this.Services.GetRequiredService<AppSettings>());
        this.logger.LogInformation("Exiting...");
        this.logger.LogInformation("------------------------------");
    }

    [SupportedOSPlatform("windows")]
    private async Task UpdateCheck(AppNotificationService notificationService)
    {
        this.logger.LogInformation("Current version: {Version}", Assembly.GetExecutingAssembly().GetName().Version);

        var updateManager = new UpdateManager(new GithubSource("https://github.com/WoWs-Builder-Team/WoWs-ShipBuilder", null, false));
        if (!updateManager.IsInstalled)
        {
            this.logger.LogInformation("No update.exe found, aborting update check");
            return;
        }

        this.logger.LogInformation("Update manager initialized");
        try
        {
            // Can throw a null-reference-exception, no idea why.
            var newVersion = await updateManager.CheckForUpdatesAsync();
            if (newVersion is null)
            {
                this.logger.LogInformation("No app update found");
                return;
            }

            await notificationService.NotifyAppUpdateStart();
            await updateManager.DownloadUpdatesAsync(newVersion);

            this.logger.LogInformation("App updated to version {ReleaseVersion}", newVersion.TargetFullRelease.Version);
            await notificationService.NotifyAppUpdateComplete();
            var result = await ShowUpdateRestartDialog((this.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow, this.Services.GetRequiredService<ILocalizer>());
            if (result.Equals(MessageBox.MessageBoxResult.Yes))
            {
                this.logger.LogInformation("User decided to restart after update");
                if (OperatingSystem.IsWindows())
                {
                    updateManager.ApplyUpdatesAndRestart(newVersion);
                }
            }
        }
        catch (Exception e)
        {
#if DEBUG
            this.logger.LogWarning(e, "Exception during app update");
#else
            this.logger.LogError(e, "Exception during app update");
#endif
            await notificationService.NotifyAppUpdateError(nameof(Translation.NotificationService_ErrorMessage));
        }
    }

    private sealed class CustomModeDetector : IModeDetector
    {
        public bool? InUnitTestRunner() => false;
    }
}
