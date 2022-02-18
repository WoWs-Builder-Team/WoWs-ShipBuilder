using System;
using System.Runtime.Versioning;
using System.Threading;
using Avalonia;
using Avalonia.ReactiveUI;
using Squirrel;
using WoWsShipBuilder.Core;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.UI.Extensions;
using WoWsShipBuilder.UI.Settings;

namespace WoWsShipBuilder.UI
{
    class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static void Main(string[] args)
        {
            Logging.InitializeLogging(ApplicationSettings.ApplicationOptions.SentryDsn);
            Logging.Logger.Info("------------------------------");
            Logging.Logger.Info("Starting application...");
            var culture = AppDataHelper.Instance.DefaultCultureDetails.CultureInfo;
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            if (OperatingSystem.IsWindows())
            {
                SquirrelAwareApp.HandleEvents(OnInitialInstall, onAppUninstall: OnAppUninstall, onEveryRun: OnEveryRun);
            }

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
            tools.CreateShortcutForThisExe();
        }
    }
}
