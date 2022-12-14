using System;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Autofac;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using Splat;
using Splat.Autofac;
using Squirrel;
using WoWsShipBuilder.Core;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.DataProvider.Updater;
using WoWsShipBuilder.Core.HttpClients;
using WoWsShipBuilder.Core.Localization;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.Core.Settings;
using WoWsShipBuilder.UI.Extensions;
using WoWsShipBuilder.UI.Services;
using WoWsShipBuilder.UI.Settings;
using WoWsShipBuilder.UI.UserControls;
using WoWsShipBuilder.UI.Utilities;
using WoWsShipBuilder.UI.ViewModels;
using WoWsShipBuilder.UI.ViewModels.DispersionPlot;
using WoWsShipBuilder.UI.ViewModels.ShipVm;
using WoWsShipBuilder.UI.Views;
using WoWsShipBuilder.ViewModels.Other;
using Localizer = WoWsShipBuilder.Core.Localization.Localizer;

namespace WoWsShipBuilder.UI
{
    [SuppressMessage("System.IO.Abstractions", "IO0003", Justification = "This class is never tested.")]
    [SuppressMessage("System.IO.Abstractions", "IO0006", Justification = "This class is never tested.")]
    public class App : Application
    {
        private IContainer container = null!;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                container = SetupDependencyInjection();

                AppSettingsHelper.LoadSettings();
                LoggingSetup.InitializeLogging(ApplicationSettings.ApplicationOptions.SentryDsn, AppSettingsHelper.Settings, true);
                desktop.Exit += OnExit;
                desktop.MainWindow = new SplashScreen();
                Logging.Logger.Info($"AutoUpdate Enabled: {AppSettingsHelper.Settings.AutoUpdateEnabled}");

                if (AppSettingsHelper.Settings.AutoUpdateEnabled)
                {
                    Task.Run(async () =>
                    {
                        if (OperatingSystem.IsWindows())
                        {
                            await UpdateCheck(container.Resolve<AppNotificationService>());
                            Logging.Logger.Info("Finished updatecheck");
                        }
                        else
                        {
                            Logging.Logger.Warn("Skipped updatecheck");
                        }
                    });
                }
            }

            base.OnFrameworkInitializationCompleted();
        }

        private static IContainer SetupDependencyInjection()
        {
            var builder = new ContainerBuilder();

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
            Logging.Logger.Info("Closing app, saving setting and builds");
            AppSettingsHelper.SaveSettings();
            scope.Resolve<IUserDataService>().SaveBuilds();
            Logging.Logger.Info("Exiting...");
            Logging.Logger.Info(new string('-', 30));
        }

        [SupportedOSPlatform("windows")]
        private async Task UpdateCheck(AppNotificationService notificationService)
        {
            Logging.Logger.Info($"Current version: {Assembly.GetExecutingAssembly().GetName().Version}");

            using UpdateManager updateManager = new GithubUpdateManager("https://github.com/WoWs-Builder-Team/WoWs-ShipBuilder");
            if (!updateManager.IsInstalledApp)
            {
                Logging.Logger.Info("No update.exe found, aborting update check.");
                return;
            }

            Logging.Logger.Info("Update manager initialized.");
            try
            {
                // Can throw a null-reference-exception, no idea why.
                var updateInfo = await updateManager.CheckForUpdate();
                if (!updateInfo.ReleasesToApply.Any())
                {
                    Logging.Logger.Info("No app update found.");
                    return;
                }

                await notificationService.NotifyAppUpdateStart();
                var release = await updateManager.UpdateApp();
                if (release == null)
                {
                    Logging.Logger.Info("No app update found.");
                    return;
                }

                Logging.Logger.Info($"App updated to version {release.Version}");
                await notificationService.NotifyAppUpdateComplete();
                var result = await UpdateHelpers.ShowUpdateRestartDialog((ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow);
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
                await notificationService.NotifyAppUpdateError(nameof(Translation.NotificationService_ErrorMessage));
            }
        }
    }
}
