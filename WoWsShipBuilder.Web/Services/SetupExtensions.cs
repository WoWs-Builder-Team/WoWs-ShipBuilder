using System.IO.Abstractions;
using WoWsShipBuilder.Core.Localization;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.Core.Settings;
using WoWsShipBuilder.Web.Data;

namespace WoWsShipBuilder.Web.Services;

public static class SetupExtensions
{
    public static IServiceCollection AddShipBuilderServerServices(this IServiceCollection services)
    {
        services.AddSingleton<IFileSystem, FileSystem>();
        services.AddSingleton<IDataService, DesktopDataService>();
        services.AddSingleton<ILocalizationProvider, LocalizationProvider>();

        services.AddScoped<ILocalizer, Localizer>();
        services.AddScoped<AppSettingsHelper>();
        services.AddScoped<AppSettings>();
        services.AddScoped<RefreshNotifierService>();

        return services;
    }
}
