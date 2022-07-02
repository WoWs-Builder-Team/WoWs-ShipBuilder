using System.Globalization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;
using ReactiveUI;
using Splat;
using Splat.Microsoft.Extensions.DependencyInjection;
using WoWsShipBuilder.Core;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.DataProvider.Updater;
using WoWsShipBuilder.Core.Localization;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.Core.Settings;
using WoWsShipBuilder.Web;
using WoWsShipBuilder.Web.Data;
using WoWsShipBuilder.Web.Services;
using WoWsShipBuilder.Web.WebWorkers;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.UseMicrosoftDependencyResolver();
var resolver = Locator.CurrentMutable;
resolver.InitializeSplat();
resolver.InitializeReactiveUI();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new(builder.HostEnvironment.BaseAddress) });
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
});

builder.Services.AddShipBuilderServices();

var host = builder.Build();
host.Services.UseMicrosoftDependencyResolver();

var settingsHelper = host.Services.GetRequiredService<AppSettingsHelper>();
var settings = await settingsHelper.LoadSettings() ?? new AppSettings();
var appSettings = host.Services.GetRequiredService<AppSettings>();
appSettings.UpdateFromSettings(settings);

CultureInfo.DefaultThreadCurrentCulture = settings.SelectedLanguage.CultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = settings.SelectedLanguage.CultureInfo;

await settingsHelper.SaveSettings(appSettings);
WebWorkerDataService.Settings = appSettings;

SetupExtensions.SetupLogging();
var logger = Logging.GetLogger("ShipBuilderInit");

await host.Services.GetRequiredService<ILocalDataUpdater>().RunDataUpdateCheck(settings.SelectedServerType, new Progress<(int, string)>());
await host.Services.GetRequiredService<ILocalizationProvider>().RefreshDataAsync(appSettings.SelectedServerType, appSettings.SelectedLanguage);

AppData.ShipDictionary = new();
logger.Debug("Initializing summary list...");
AppData.ShipSummaryList ??= await host.Services.GetRequiredService<IAppDataService>().GetShipSummaryList(appSettings.SelectedServerType);
logger.Debug("Summary list initialized. Preparation finished.");

await host.RunAsync();
