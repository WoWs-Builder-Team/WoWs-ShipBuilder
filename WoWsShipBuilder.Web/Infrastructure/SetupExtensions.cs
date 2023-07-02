using System.Reflection;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.JSInterop;
using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Loki;
using NLog.Targets;
using Sentry;
using WoWsShipBuilder.Features.LinkShortening;
using WoWsShipBuilder.Infrastructure;
using WoWsShipBuilder.Infrastructure.ApplicationData;
using WoWsShipBuilder.Infrastructure.Utility;
using LogLevel = NLog.LogLevel;

namespace WoWsShipBuilder.Web.Infrastructure;

public static class SetupExtensions
{
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

    public static IServiceCollection AddCookieAuth(this IServiceCollection services)
    {
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
        {
            options.LoginPath = "/api/auth/login";
            options.LogoutPath = "/api/auth/logout";
            options.ExpireTimeSpan = TimeSpan.FromDays(7);
            options.SlidingExpiration = true;
        });
        return services;
    }
}
