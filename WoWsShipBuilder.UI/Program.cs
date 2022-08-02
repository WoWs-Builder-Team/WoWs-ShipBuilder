using System;
using System.Runtime.Versioning;
using System.Threading;
using Avalonia;
using Squirrel;
using WoWsShipBuilder.Core;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.UI.Extensions;
using WoWsShipBuilder.UI.Settings;
using WoWsShipBuilder.UI.Utilities;

namespace WoWsShipBuilder.UI
{
    class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static void Main(string[] args)
        {
            LoggingSetup.InitializeLogging(ApplicationSettings.ApplicationOptions.SentryDsn, new());
            if (OperatingSystem.IsWindows())
            {
                Logging.Logger.Debug("Operating system is windows.");
                SquirrelAwareApp.HandleEvents(onInitialInstall: OnInitialInstall, onAppUninstall: OnAppUninstall, onEveryRun: OnEveryRun);
            }

            Logging.Logger.Info("------------------------------");
            Logging.Logger.Info("Starting application...");
            var culture = AppConstants.DefaultCultureDetails.CultureInfo;
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            try
            {
                BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            }
            catch (Exception e)
            {
                Logging.Logger.Fatal(e, "Encountered a critical error that will end the application.");
                throw;
            }

            Logging.Logger.Info("Application is shutting down.");
            Logging.Logger.Info("------------------------------\n");
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
            Logging.Logger.Info("App is uninstalling...");
            tools.RemoveShortcutForThisExe();
        }

        [SupportedOSPlatform("windows")]
        private static void OnInitialInstall(SemanticVersion version, IAppTools tools)
        {
            Logging.Logger.Info("App has been installed, creating shortcuts...");
            try
            {
                tools.CreateShortcutForThisExe();
                Logging.Logger.Info("Shortcuts have been created.");
            }
            catch (Exception e)
            {
                Logging.Logger.Error(e);
                throw;
            }
        }
    }
}
