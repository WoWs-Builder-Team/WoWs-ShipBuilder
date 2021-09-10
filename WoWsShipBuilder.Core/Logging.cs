using NLog;
using NLog.Config;
using NLog.Targets;
using WoWsShipBuilder.UI.Settings;

namespace WoWsShipBuilder.UI
{
    public static class Logging
    {
        public static Logger Logger { get; } = LogManager.GetLogger("ShipBuilder");

        public static Logger GetLogger(string name = "ShipBuilder") => LogManager.GetLogger(name);

        public static void InitializeLogging()
        {
            var config = new LoggingConfiguration();
            var target = new FileTarget
            {
                FileName = "${specialfolder:folder=ApplicationData}/WoWsShipBuilder/logs/WoWsShipBuilder-${shortdate}.log",
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
    }
}
