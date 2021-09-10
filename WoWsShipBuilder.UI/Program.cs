using Avalonia;
using Avalonia.ReactiveUI;
using NLog;
using NLog.Config;
using NLog.Targets;
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
            Logging.InitializeLogging();
            Logging.Logger.Info("Starting application...");

            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        public static void InitializeLogging()
        {
            var config = new LoggingConfiguration();
            var target = new FileTarget
            {
                FileName = "${basedir}/logs/WoWsShipBuilder-${shortdate}.log",
                MaxArchiveFiles = 5,
                ArchiveAboveSize = 10240,
            };
            config.AddTarget("logfile", target);
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, target));

            config.AddSentry(o =>
            {
                o.Layout = "${message}";
                o.BreadcrumbLayout = "${logger}: ${message}";
                o.MinimumBreadcrumbLevel = LogLevel.Info;
                o.MinimumEventLevel = LogLevel.Error;
                o.AddTag("logger", "${logger}");

                o.SendDefaultPii = false;
                o.Dsn = ApplicationSettings.ApplicationOptions.SentryDsn;
            });
            LogManager.Configuration = config;
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace()
                .UseReactiveUI();
    }
}
