using System.Diagnostics;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace WoWsShipBuilder.Core
{
    public static class Logging
    {
        public static Logger Logger { get; } = LogManager.GetLogger("ShipBuilder");

        public static Logger GetLogger(string name = "ShipBuilder") => LogManager.GetLogger(name);

        public static void InitializeLogging(string? sentryDsn = null)
        {
            var config = new LoggingConfiguration();
            var target = new FileTarget
            {
                FileName = "${specialfolder:folder=ApplicationData}/WoWsShipBuilder/logs/WoWsShipBuilder-${shortdate}.log",
                Layout = "${longdate}|${level}|${logger}|${message:withException=true}",
                MaxArchiveFiles = 5,
                ArchiveAboveSize = 10240000,
            };
            config.AddTarget("logfile", target);
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Info, target));
            Trace.Listeners.Add(new NLogTraceListener());

#if DEBUG
            var debugTarget = new FileTarget
            {
                FileName = "${specialfolder:folder=ApplicationData}/WoWsShipBuilder/logs/WoWsShipBuilder-${shortdate}-debug.log",
                Layout = "${longdate}|${level}|${logger}|${message:withException=true}",
                MaxArchiveFiles = 5,
                ArchiveAboveSize = 10240000,
            };
            config.AddTarget("logfile-debug", debugTarget);
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, debugTarget));
#endif

            config.AddSentry(o =>
            {
                o.Layout = "${message}";
                o.BreadcrumbLayout = "${logger}: ${message}";
                o.MinimumBreadcrumbLevel = LogLevel.Info;
                o.MinimumEventLevel = LogLevel.Error;
                o.AddTag("logger", "${logger}");

                o.SendDefaultPii = false;
                o.Dsn = sentryDsn;
            });
            LogManager.Configuration = config;

            if (sentryDsn != null)
            {
                Logger.Debug("Non-null sentry dsn was detected. Trying to initialize sentry sdk.");
            }
        }
    }
}
