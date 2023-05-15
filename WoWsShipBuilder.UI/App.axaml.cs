using System;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Config;
using NLog.Extensions.Logging;
using ReactiveUI;
using Splat;
using Splat.Autofac;
using Squirrel;
using WoWsShipBuilder.Common.Infrastructure;
using WoWsShipBuilder.Common.Infrastructure.Data;
using WoWsShipBuilder.Common.Infrastructure.HttpClients;
using WoWsShipBuilder.Common.Infrastructure.Localization;
using WoWsShipBuilder.Common.Infrastructure.Navigation;
using WoWsShipBuilder.Common.Settings;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.DataProvider.Updater;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.UI.Extensions;
using WoWsShipBuilder.UI.Services;
using WoWsShipBuilder.UI.Settings;
using WoWsShipBuilder.UI.Updater;
using WoWsShipBuilder.UI.UserControls;
using WoWsShipBuilder.UI.Utilities;
using WoWsShipBuilder.UI.ViewModels;
using WoWsShipBuilder.UI.ViewModels.DispersionPlot;
using WoWsShipBuilder.UI.ViewModels.ShipVm;
using WoWsShipBuilder.UI.Views;
using Localizer = WoWsShipBuilder.Common.Infrastructure.Localization.Localizer;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace WoWsShipBuilder.UI
{
    [SuppressMessage("System.IO.Abstractions", "IO0003", Justification = "This class is never tested.")]
    [SuppressMessage("System.IO.Abstractions", "IO0006", Justification = "This class is never tested.")]
    public class App : Application
    {
        private IContainer container = null!;

        private ILogger<App> logger = default!;

        public App()
        {
            ModeDetector.OverrideModeDetector(new CustomModeDetector());
        }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                container = SetupDependencyInjection(LogManager.Configuration);
                Logging.Initialize(container.Resolve<ILoggerFactory>());
                AppSettingsHelper.LoadSettings();
                LogManager.ReconfigExistingLoggers();
                logger = container.Resolve<ILogger<App>>();

                desktop.Exit += OnExit;
                desktop.MainWindow = new SplashScreen();
                logger.LogInformation("AutoUpdate Enabled: {SettingsAutoUpdateEnabled}", AppSettingsHelper.Settings.AutoUpdateEnabled);

                if (AppSettingsHelper.Settings.AutoUpdateEnabled)
                {
                    Task.Run(async () =>
                    {
                        if (OperatingSystem.IsWindows())
                        {
                            await UpdateCheck(container.Resolve<AppNotificationService>());
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

        private static IContainer SetupDependencyInjection(LoggingConfiguration configuration)
        {
            var services = new ServiceCollection();
            services.AddLogging(options =>
            {
                options.ClearProviders();
                options.SetMinimumLevel(LogLevel.Trace);
                options.AddNLog(configuration, new() { ParseMessageTemplates = true });
            });
            var builder = new ContainerBuilder();
            builder.Populate(services);

            builder.RegisterType<AppSettings>().SingleInstance();
            builder.RegisterInstance(new FileSystem()).As<IFileSystem>().SingleInstance();
            builder.RegisterType<DesktopDataService>().As<IDataService>().SingleInstance();
            builder.RegisterType<DesktopAppDataService>().As<IAppDataService>().As<DesktopAppDataService>().As<IUserDataService>().SingleInstance();
            builder.RegisterType<LocalizationProvider>().As<ILocalizationProvider>().SingleInstance();
            builder.RegisterType<Localizer>().AsSelf().As<ILocalizer>().SingleInstance();
            builder.RegisterType<AwsClient>().As<IAwsClient>().SingleInstance();
            builder.RegisterType<NavigationService>().As<INavigationService>().SingleInstance();
            builder.RegisterType<AvaloniaClipboardService>().As<IClipboardService>();
            builder.RegisterType<LocalDataUpdater>().As<ILocalDataUpdater>();
            builder.RegisterType<AppNotificationService>().SingleInstance();

            builder.RegisterType<StartMenuViewModel>();
            builder.RegisterType<ShipWindowViewModel>();
            builder.RegisterType<DispersionGraphViewModel>();
            builder.RegisterType<SplashScreenViewModel>();

            var resolver = builder.UseAutofacDependencyResolver();
            Locator.SetLocator(resolver);

            PlatformRegistrationManager.SetRegistrationNamespaces(RegistrationNamespace.Avalonia);
            resolver.InitializeSplat();
            resolver.InitializeReactiveUI(RegistrationNamespace.Avalonia);
            resolver.InitializeAvalonia();

            var diContainer = builder.Build();
            resolver.SetLifetimeScope(diContainer);

            return diContainer;
        }

        private void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            using var scope = container.BeginLifetimeScope();
            logger.LogInformation("Closing app, saving setting and builds");
            AppSettingsHelper.SaveSettings();
            scope.Resolve<IUserDataService>().SaveBuilds();
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
                var result = await UpdateHelpers.ShowUpdateRestartDialog((ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow);
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
