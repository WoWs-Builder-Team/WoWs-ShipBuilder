using WoWsShipBuilder.Infrastructure.ApplicationData;
using WoWsShipBuilder.Infrastructure.DataTransfer;
using WoWsShipBuilder.Infrastructure.Utility;

namespace WoWsShipBuilder.Web.Infrastructure;

using WoWsShipBuilder.Infrastructure.HttpClients;
using WoWsShipBuilder.Web.Infrastructure.BetaAccess;
using WoWsShipBuilder.Web.Infrastructure.Data;

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

        services.AddTransient<DataInitializer>();

        return services;
    }
}
