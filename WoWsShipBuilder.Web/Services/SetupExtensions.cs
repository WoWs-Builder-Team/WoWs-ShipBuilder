using System.IO.Abstractions;
using System.Reflection;
using Microsoft.AspNetCore.Components.Server.Circuits;
using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Loki;
using NLog.Targets;
using WoWsShipBuilder.Core.Localization;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.Core.Settings;
using WoWsShipBuilder.Web.Data;
using LogLevel = NLog.LogLevel;

namespace WoWsShipBuilder.Web.Services;

public static class SetupExtensions
{
    public static IServiceCollection AddShipBuilderServerServices(this IServiceCollection services)
    {
        services.AddSingleton<IFileSystem, FileSystem>();
        services.AddSingleton<IDataService, DesktopDataService>();
        services.AddSingleton<ILocalizationProvider, LocalizationProvider>();
        services.AddSingleton<IMetricsService, MetricsService>();
        services.AddSingleton<CircuitHandler, MetricCircuitHandler>();

        services.AddScoped<ILocalizer, Localizer>();
        services.AddScoped<AppSettingsHelper>();
        services.AddScoped<AppSettings>();
        services.AddScoped<RefreshNotifierService>();
        services.AddScoped<ChartJsInterop>();
        services.AddScoped<MouseEventInterop>();
        services.AddScoped<IClipboardService, WebClipboardService>();
        services.AddScoped<DepthChargeDamageDistributionChartInterop>();

        return services;
    }

    public static void ConfigureNlog(bool lokiDisabled)
    {
        var configuration = new LoggingConfiguration();
        var logConsole = new ConsoleTarget("logconsole");
        configuration.AddRule(LogLevel.Info, LogLevel.Fatal, logConsole);
        if (!lokiDisabled)
        {
            var logLoki = new LokiTarget
            {
                Name = "loki",
                TaskDelayMilliseconds = 500,
                Endpoint = "http://loki:3100",
                Layout = Layout.FromString("${level}|${message}${onexception:|${exception:format=type,message,method:maxInnerExceptionLevel=5:innerFormat=shortType,message,method}}|source=${logger}"),
                Labels =
                {
                    new LokiTargetLabel
                    {
                        Name = "app",
                        Layout = "wowssb-main",
                    }
                },
            };

            configuration.AddRule(LogLevel.Info, LogLevel.Fatal, logLoki);
        }

        var version = Assembly.GetEntryAssembly()?.GetName().Version ?? new Version(0, 0);
        var release = $"{version.Major}.{version.Minor}.{version.Build}";
        configuration.AddSentry(o =>
        {
            o.Release = release;
            o.Layout = "${message}";
            o.BreadcrumbLayout = "${logger}: ${message}";
            o.MinimumBreadcrumbLevel = LogLevel.Info;
            o.MinimumEventLevel = LogLevel.Error;
            o.AddTag("logger", "${logger}");

            o.SendDefaultPii = false;
        });

        LogManager.Configuration = configuration;
        LogManager.ReconfigExistingLoggers();
    }
}
