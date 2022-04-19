using System.IO.Abstractions;
using DnetIndexedDb;
using DnetIndexedDb.Fluent;
using DnetIndexedDb.Models;
using NLog;
using NLog.Config;
using NLog.Targets;
using WoWsShipBuilder.Core.DataProvider.Updater;
using WoWsShipBuilder.Core.HttpClients;
using WoWsShipBuilder.Core.Localization;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.Core.Settings;
using WoWsShipBuilder.Web.Data;
using LogLevel = NLog.LogLevel;

namespace WoWsShipBuilder.Web.Services;

public static class SetupExtensions
{
    public static IServiceCollection AddShipBuilderServices(this IServiceCollection services)
    {
        var gameDataDbModel = new IndexedDbDatabaseModel().WithName("data").WithVersion(1);
        gameDataDbModel.AddStore<GameDataDto>("live");
        gameDataDbModel.AddStore<GameDataDto>("pts");
#if DEBUG
        gameDataDbModel.AddStore<GameDataDto>("dev1");
        gameDataDbModel.AddStore<GameDataDto>("dev2");
        gameDataDbModel.AddStore<GameDataDto>("dev3");
#endif
        services.AddIndexedDbDatabase<GameDataDb>(options =>
        {
            options.UseDatabase(gameDataDbModel);
        });

        services.AddScoped<AppSettingsHelper>();
        services.AddScoped<AppSettings>();
        services.AddScoped<IDataService, WebDataService>();
        services.AddScoped<IAppDataService, WebAppDataService>();
        services.AddScoped<ILocalDataUpdater, WebDataUpdate>();
        services.AddScoped<IAwsClient, AwsClient>();
        services.AddScoped<ILocalizer, Localizer>();
        services.AddScoped<ILocalizationProvider, LocalizationProvider>();
        return services;
    }

    public static IServiceCollection AddShipBuilderServerServices(this IServiceCollection services)
    {
        services.AddSingleton<IFileSystem, FileSystem>();
        services.AddSingleton<IDataService, DesktopDataService>();
        services.AddSingleton<ILocalizationProvider, LocalizationProvider>();

        services.AddScoped<ILocalizer, Localizer>();
        services.AddScoped<AppSettingsHelper>();
        services.AddScoped<AppSettings>();

        return services;
    }

    public static void SetupLogging()
    {
        var config = new LoggingConfiguration();
        var target = new ConsoleTarget()
        {
            Layout = "${level}|${logger}|${message:withException=true}",
        };
        config.AddTarget("Console", target);
        config.LoggingRules.Add(new LoggingRule("*", LogLevel.Info, target));
        LogManager.Configuration = config;
        LogManager.ReconfigExistingLoggers();
    }
}
