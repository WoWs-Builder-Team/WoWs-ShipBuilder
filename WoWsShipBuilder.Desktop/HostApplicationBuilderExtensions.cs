using System.IO.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.Desktop.Infrastructure;
using WoWsShipBuilder.Desktop.Services;
using WoWsShipBuilder.Desktop.Updater;
using WoWsShipBuilder.Desktop.Utilities;
using WoWsShipBuilder.Features.Settings;
using WoWsShipBuilder.Infrastructure;
using WoWsShipBuilder.Infrastructure.Data;
using WoWsShipBuilder.Infrastructure.HttpClients;

namespace WoWsShipBuilder.Desktop;

public static class HostApplicationBuilderExtensions
{
    public static HostApplicationBuilder UseShipBuilderDesktop(this HostApplicationBuilder builder)
    {
        builder.Services.Configure<CdnOptions>(builder.Configuration.GetSection(CdnOptions.SectionName));

        builder.Services.UseShipBuilder();
        builder.Services.AddSingleton<IFileSystem, FileSystem>();
        builder.Services.AddSingleton<IDataService, DesktopDataService>();
        builder.Services.AddSingleton<IAppDataService, DesktopAppDataService>();
        builder.Services.AddSingleton<IUserDataService>(x => (IUserDataService)x.GetRequiredService<IAppDataService>());
        builder.Services.AddSingleton<AwsClient>();
        builder.Services.AddSingleton<IAwsClient>(x => x.GetRequiredService<AwsClient>());
        builder.Services.AddSingleton<IDesktopAwsClient>(x => x.GetRequiredService<AwsClient>());
        builder.Services.AddSingleton<INavigationService, NavigationService>();
        builder.Services.AddSingleton<IClipboardService, AvaloniaClipboardService>();
        builder.Services.AddSingleton<ISettingsAccessor, DesktopSettingsAccessor>();
        builder.Services.AddTransient<ILocalDataUpdater, LocalDataUpdater>();
        builder.Services.AddSingleton<AppNotificationService>();

        // Replace the default scoped instance with a singleton in order to ensure that the same instance is used throughout the application
        builder.Services.RemoveAll<AppSettings>();
        builder.Services.AddSingleton<AppSettings>();
        builder.Services.RemoveAll<RefreshNotifierService>();
        builder.Services.AddSingleton<RefreshNotifierService>();

        builder.Services.AddTransient<SplashScreenViewModel>();
        builder.Services.AddWindowsFormsBlazorWebView();
        builder.Services.AddBlazorWebViewDeveloperTools();

        return builder;
    }
}
