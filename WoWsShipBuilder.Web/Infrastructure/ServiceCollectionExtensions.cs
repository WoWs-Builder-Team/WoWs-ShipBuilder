using WoWsShipBuilder.Features.DataTransfer;
using WoWsShipBuilder.Infrastructure.ApplicationData;
using WoWsShipBuilder.Infrastructure.HttpClients;
using WoWsShipBuilder.Infrastructure.Utility;
using WoWsShipBuilder.Web.Features.Authentication;
using WoWsShipBuilder.Web.Features.BetaAccess;
using WoWsShipBuilder.Web.Infrastructure.Data;

namespace WoWsShipBuilder.Web.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection UseShipBuilderWeb(this IServiceCollection services)
    {
        services.UseShipBuilder();

        services.AddSingleton<IAwsClient, ServerAwsClient>();
        services.AddSingleton<IAppDataService, ServerAppDataService>();
        services.AddSingleton<IBetaAccessManager, BetaAccessManager>();

        services.AddScoped<IUserDataService, WebUserDataService>();
        services.AddScoped<IClipboardService, WebClipboardService>();
        services.AddScoped<ISettingsAccessor, WebSettingsAccessor>();
        services.AddScoped<AuthenticationService>();

        services.AddTransient<DataInitializer>();

        return services;
    }
}
