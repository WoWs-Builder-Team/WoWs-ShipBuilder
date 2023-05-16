using System;
using System.Runtime.Versioning;
using System.Threading;
using Avalonia;
using NLog;
using Sentry;
using Squirrel;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.Infrastructure;
using WoWsShipBuilder.UI.Extensions;
using WoWsShipBuilder.UI.Settings;
using WoWsShipBuilder.UI.Utilities;

namespace WoWsShipBuilder.UI;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    public static void Main(string[] args)
    {
        var loggerConfig = LoggingSetup.CreateLoggingConfiguration();
        LogManager.Configuration = loggerConfig;
        SentrySdk.Init(ApplicationSettings.ApplicationOptions.SentryDsn);

        var logger = LogManager.GetCurrentClassLogger();
        if (OperatingSystem.IsWindows())
        {
            logger.Debug("Operating system is windows.");
            SquirrelAwareApp.HandleEvents(onInitialInstall: OnInitialInstall, onAppUninstall: OnAppUninstall, onEveryRun: OnEveryRun);
        }

        logger.Info("------------------------------");
        logger.Info("Starting application...");
        var culture = AppConstants.DefaultCultureDetails.CultureInfo;
        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;

        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception e)
        {
            logger.Fatal(e, "Encountered a critical error that will end the application.");
            throw;
        }

        logger.Info("Application is shutting down.");
        logger.Info("------------------------------\n");
    }

    private static void OnEveryRun(SemanticVersion version, IAppTools tools, bool firstRun)
    {
        tools.SetProcessAppUserModelId();
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .UseSkia()
            .UseUpdatedReactiveUI();

    [SupportedOSPlatform("windows")]
    private static void OnAppUninstall(SemanticVersion version, IAppTools tools)
    {
        LogManager.GetCurrentClassLogger().Info("App is uninstalling...");
        tools.RemoveShortcutForThisExe();
    }

    [SupportedOSPlatform("windows")]
    private static void OnInitialInstall(SemanticVersion version, IAppTools tools)
    {
        var logger = LogManager.GetCurrentClassLogger();
        logger.Info("App has been installed, creating shortcuts...");
        try
        {
            tools.CreateShortcutForThisExe();
            logger.Info("Shortcuts have been created.");
        }
        catch (Exception e)
        {
            logger.Error(e);
            throw;
        }
    }
}
