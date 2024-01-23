using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using WoWsShipBuilder.Desktop.Features.LinkShortening;
using WoWsShipBuilder.Desktop.Features.SplashScreen;
using WoWsShipBuilder.Desktop.Features.Updater;
using WoWsShipBuilder.Desktop.Infrastructure.AwsClient;
using WoWsShipBuilder.Desktop.Infrastructure.Data;
using WoWsShipBuilder.Features.LinkShortening;
using WoWsShipBuilder.Features.Settings;
using WoWsShipBuilder.Infrastructure.ApplicationData;
using WoWsShipBuilder.Infrastructure.DataTransfer;
using WoWsShipBuilder.Infrastructure.HttpClients;
using WoWsShipBuilder.Infrastructure.Utility;

namespace WoWsShipBuilder.Desktop.Infrastructure;

public static class HostApplicationBuilderExtensions
{
    public static HostApplicationBuilder UseShipBuilderDesktop(this HostApplicationBuilder builder)
    {
        builder.Services.Configure<CdnOptions>(builder.Configuration.GetSection(CdnOptions.SectionName));
        builder.Services.Configure<ShipBuilderOptions>(builder.Configuration.GetSection(ShipBuilderOptions.SectionName));
        builder.Services.Configure<LinkShorteningOptions>(builder.Configuration.GetSection(LinkShorteningOptions.SectionName));

        builder.Services.UseShipBuilder();
        builder.Services.AddSingleton<IFileSystem, FileSystem>();
        builder.Services.AddSingleton<IDataService, DesktopDataService>();
        builder.Services.AddSingleton<IAppDataService, DesktopAppDataService>();
        builder.Services.AddSingleton<IUserDataService, DesktopUserDataService>();
        builder.Services.AddSingleton<AwsClient.AwsClient>();
        builder.Services.AddSingleton<IAwsClient>(x => x.GetRequiredService<AwsClient.AwsClient>());
        builder.Services.AddSingleton<IDesktopAwsClient>(x => x.GetRequiredService<AwsClient.AwsClient>());
        builder.Services.AddSingleton<IClipboardService, AvaloniaClipboardService>();
        builder.Services.AddSingleton<ISettingsAccessor, DesktopSettingsAccessor>();
        builder.Services.AddTransient<ILocalDataUpdater, LocalDataUpdater>();
        builder.Services.AddSingleton<AppNotificationService>();
        builder.Services.AddSingleton<ILinkShortener, ApiLinkShortener>();

        // Replace the default scoped instance with a singleton in order to ensure that the same instance is used throughout the application
        builder.Services.RemoveAll<AppSettings>();
        builder.Services.AddSingleton<AppSettings>();
        builder.Services.RemoveAll<RefreshNotifierService>();
        builder.Services.AddSingleton<RefreshNotifierService>();

        builder.Services.AddTransient<SplashScreenViewModel>();
        builder.Services.AddWindowsFormsBlazorWebView();
#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
#endif

        return builder;
    }
}
