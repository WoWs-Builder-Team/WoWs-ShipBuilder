using System;
using System.Diagnostics;
using System.Reflection;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace WoWsShipBuilder.Desktop.Utilities;

public static class LoggingSetup
{
    public static LoggingConfiguration CreateLoggingConfiguration()
    {
        var config = new LoggingConfiguration();
        var target = new FileTarget
        {
#if DEBUG
            FileName = "${specialfolder:folder=ApplicationData}/WoWsShipBuilderDev/logs/WoWsShipBuilder-${shortdate}.log",
#else
            FileName = "${specialfolder:folder=ApplicationData}/WoWsShipBuilder/logs/WoWsShipBuilder-${shortdate}.log",
#endif
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
            FileName = "${specialfolder:folder=ApplicationData}/WoWsShipBuilderDev/logs/WoWsShipBuilder-${shortdate}-debug.log",
            Layout = "${longdate}|${level}|${logger}|${message:withException=true}",
            MaxArchiveFiles = 5,
            ArchiveAboveSize = 10240000,
        };
        config.AddTarget("logfile-debug", debugTarget);
        config.LoggingRules.Add(new("*", LogLevel.Debug, debugTarget));
#endif

        var version = Assembly.GetEntryAssembly()?.GetName().Version ?? new Version(0, 0);
        var release = $"{version.Major}.{version.Minor}.{version.Build}";

        config.AddSentry(o =>
        {
            o.Release = release;
            o.Layout = "${message}";
            o.BreadcrumbLayout = "${logger}: ${message}";
            o.MinimumBreadcrumbLevel = LogLevel.Info;
            o.MinimumEventLevel = LogLevel.Error;
            o.AddTag("logger", "${logger}");

            o.SendDefaultPii = false;

            o.AutoSessionTracking = false;
        });

        return config;
    }
}
