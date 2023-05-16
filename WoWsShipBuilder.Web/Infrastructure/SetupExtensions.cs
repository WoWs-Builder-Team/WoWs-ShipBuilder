using System.IO.Abstractions;
using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.JSInterop;
using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Loki;
using NLog.Targets;
using Sentry;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.Features.Charts;
using WoWsShipBuilder.Features.LinkShortening;
using WoWsShipBuilder.Features.Settings;
using WoWsShipBuilder.Infrastructure;
using WoWsShipBuilder.Infrastructure.Data;
using WoWsShipBuilder.Infrastructure.DataTransfer;
using WoWsShipBuilder.Infrastructure.HttpClients;
using WoWsShipBuilder.Infrastructure.Localization;
using WoWsShipBuilder.Infrastructure.Utility;
using WoWsShipBuilder.Web.Infrastructure.BetaAccess;
using WoWsShipBuilder.Web.Infrastructure.Data;
using WoWsShipBuilder.Web.Infrastructure.Metrics;
using LogLevel = NLog.LogLevel;

namespace WoWsShipBuilder.Web.Infrastructure;

public static class SetupExtensions
{
    public static IServiceCollection AddShipBuilderServerServices(this IServiceCollection services)
    {
        services.AddSingleton<IFileSystem, FileSystem>();
        services.AddSingleton<IDataService, DesktopDataService>();
        services.AddSingleton<ILocalizationProvider, LocalizationProvider>();
        services.AddSingleton<IMetricsService, MetricsService>();
        services.AddSingleton<CircuitHandler, MetricCircuitHandler>();
        services.AddSingleton<HttpClient>(_ => new(new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.All,
        }));
        services.AddSingleton<IAwsClient, ServerAwsClient>();
        services.AddSingleton<IAppDataService, ServerAppDataService>();
        services.AddSingleton<ILinkShortener, FirebaseLinkShortener>();
        services.AddSingleton<IBetaAccessManager, BetaAccessManager>();

        services.AddScoped<ILocalizer, Localizer>();
        services.AddScoped<AppSettingsHelper>();
        services.AddScoped<AppSettings>();
        services.AddScoped<RefreshNotifierService>();
        services.AddScoped<ChartJsInterop>();
        services.AddScoped<MouseEventInterop>();
        services.AddScoped<IClipboardService, WebClipboardService>();
        services.AddScoped<SessionStateCache>();

        services.AddTransient<DataInitializer>();

        return services;
    }

    public static void ConfigureNlog(bool lokiDisabled, bool isDebug)
    {
        var configuration = new LoggingConfiguration();
        var logConsole = new ConsoleTarget("logconsole");
        var level = isDebug ? LogLevel.Debug : LogLevel.Info;
        configuration.AddRule(level, LogLevel.Fatal, logConsole);
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

            configuration.AddRule(level, LogLevel.Fatal, logLoki);
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
            o.AddExceptionFilterForType<JSDisconnectedException>();

            o.SendDefaultPii = false;
        });

        LogManager.Configuration = configuration;
        LogManager.ReconfigExistingLoggers();
    }

    public static WebApplicationBuilder ConfigureShipBuilderOptions(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<AdminOptions>(builder.Configuration.GetSection(AdminOptions.SectionName));
        builder.Services.Configure<CdnOptions>(builder.Configuration.GetSection(CdnOptions.SectionName));
        builder.Services.Configure<LinkShorteningOptions>(builder.Configuration.GetSection(LinkShorteningOptions.SectionName));
        builder.Services.Configure<ShipBuilderOptions>(builder.Configuration.GetSection(ShipBuilderOptions.SectionName));
        return builder;
    }
}
