namespace WoWsShipBuilder.Web.Services;

using BlazorDB;
using WoWsShipBuilder.Web.Data;

public static class SetupExtensions
{
    public static IServiceCollection AddShipBuilderServices(this IServiceCollection services)
    {
        services.AddBlazorDB(options =>
        {
            options.Name = "data";
            options.Version = 1;
            options.StoreSchemas = new()
            {
                new()
                {
                    Name = "live",
                    PrimaryKey = "path",
                    PrimaryKeyAuto = false,
                },
                new()
                {
                    Name = "pts",
                    PrimaryKey = "path",
                    PrimaryKeyAuto = false,
                },
#if DEBUG
                new()
                {
                    Name = "dev1",
                    PrimaryKey = "path",
                    PrimaryKeyAuto = false,
                },
                new()
                {
                    Name = "dev2",
                    PrimaryKey = "path",
                    PrimaryKeyAuto = false,
                },
                new()
                {
                    Name = "dev3",
                    PrimaryKey = "path",
                    PrimaryKeyAuto = false,
                },
#endif
            };
        });

        services.AddSingleton<AppSettingsHelper>();
        services.AddSingleton<IAppSettingsService>(new AppSettingsService());

        return services;
    }
}
