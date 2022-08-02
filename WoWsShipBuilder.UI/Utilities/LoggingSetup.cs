﻿using System;
using System.Diagnostics;
using System.Reflection;
using NLog;
using NLog.Config;
using NLog.Targets;
using WoWsShipBuilder.Core;
using WoWsShipBuilder.Core.Settings;

namespace WoWsShipBuilder.UI.Utilities;

public static class LoggingSetup
{
    private static bool sentryInitialized;

    public static void InitializeLogging(string? sentryDsn, AppSettings appSettings, bool initializeSentry = false)
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

        var version = Assembly.GetEntryAssembly()?.GetName().Version ?? new Version(0, 0);
        var release = $"{version.Major}.{version.Minor}.{version.Build}";

        if (initializeSentry && !sentryInitialized)
        {
            sentryInitialized = true;
            config.AddSentry(o =>
            {
                o.Release = release;
                o.Layout = "${message}";
                o.BreadcrumbLayout = "${logger}: ${message}";
                o.MinimumBreadcrumbLevel = LogLevel.Info;
                o.MinimumEventLevel = LogLevel.Error;
                o.AddTag("logger", "${logger}");

                o.SendDefaultPii = false;
                o.Dsn = sentryDsn;

                o.AutoSessionTracking = appSettings.SendTelemetryData;
            });
        }

        LogManager.Configuration = config;

        if (sentryDsn != null)
        {
            Logging.Logger.Debug("Non-null sentry dsn was detected. Trying to initialize sentry sdk.");
        }

        LogManager.ReconfigExistingLoggers();
    }
}