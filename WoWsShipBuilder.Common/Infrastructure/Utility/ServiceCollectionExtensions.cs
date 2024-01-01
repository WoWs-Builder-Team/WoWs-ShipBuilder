using System.Net;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using WoWsShipBuilder.Features.BallisticCharts;
using WoWsShipBuilder.Features.LinkShortening;
using WoWsShipBuilder.Features.Settings;
using WoWsShipBuilder.Features.ShipStats;
using WoWsShipBuilder.Infrastructure.DataTransfer;
using WoWsShipBuilder.Infrastructure.Localization;
using WoWsShipBuilder.Infrastructure.Metrics;

namespace WoWsShipBuilder.Infrastructure.Utility;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection UseShipBuilder(this IServiceCollection services)
    {
        services.AddMudServices(config =>
        {
            config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
            config.PopoverOptions.Mode = PopoverMode.Legacy;
        });
        services.AddSingleton<ILocalizationProvider, LocalizationProvider>();
        services.AddSingleton<MetricsService>();
        if (OperatingSystem.IsBrowser())
        {
            services.AddSingleton<HttpClient>();
        }
        else
        {
            services.AddSingleton<HttpClient>(_ => new(new HttpClientHandler
            {
#pragma warning disable CA1416
                AutomaticDecompression = DecompressionMethods.All,
#pragma warning restore CA1416
            }));
        }

        services.AddSingleton<ILinkShortener, FirebaseLinkShortener>();

        services.AddScoped<ILocalizer, Localizer>();
        services.AddScoped<AppSettings>();
        services.AddScoped<RefreshNotifierService>();
        services.AddScoped<ChartJsInterop>();
        services.AddScoped<MouseEventInterop>();
        services.AddScoped<SessionStateCache>();
        services.AddScoped<ExpanderStateCache>();
        services.AddScoped<AppNavigator>();

        return services;
    }
}
