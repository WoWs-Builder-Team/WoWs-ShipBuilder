using System.Globalization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using ReactiveUI;
using Splat;
using Splat.Microsoft.Extensions.DependencyInjection;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.Core.Settings;
using WoWsShipBuilder.Web;
using WoWsShipBuilder.Web.Data;
using WoWsShipBuilder.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.UseMicrosoftDependencyResolver();
var resolver = Locator.CurrentMutable;
resolver.InitializeSplat();
resolver.InitializeReactiveUI();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new(builder.HostEnvironment.BaseAddress) });
builder.Services.AddMudServices();

builder.Services.AddShipBuilderServices();

var host = builder.Build();

var settingsHelper = host.Services.GetRequiredService<AppSettingsHelper>();
var settings = await settingsHelper.LoadSettings() ?? new AppSettings();

AppData.Settings = settings;

CultureInfo.DefaultThreadCurrentCulture = settings.SelectedLanguage.CultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = settings.SelectedLanguage.CultureInfo;

await settingsHelper.SaveSettings(AppData.Settings);

SetupExtensions.SetupLogging();

AppData.ShipSummaryList ??= await host.Services.GetRequiredService<IAppDataService>().GetShipSummaryList(AppData.Settings.SelectedServerType);

await Localizer.Instance.UpdateLanguage(AppData.Settings.SelectedLanguage, true);

await host.RunAsync();
