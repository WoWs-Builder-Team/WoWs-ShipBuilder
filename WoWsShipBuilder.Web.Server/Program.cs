using System.Globalization;
using System.Net;
using MudBlazor;
using MudBlazor.Services;
using ReactiveUI;
using Splat;
using Splat.Microsoft.Extensions.DependencyInjection;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.HttpClients;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.Web.Server.Services;
using WoWsShipBuilder.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

PlatformRegistrationManager.SetRegistrationNamespaces(RegistrationNamespace.Blazor);

builder.Services.UseMicrosoftDependencyResolver();
var resolver = Locator.CurrentMutable;
resolver.InitializeSplat();
resolver.InitializeReactiveUI(RegistrationNamespace.Blazor);

builder.Services.AddMudServices(config => { config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight; });

builder.Services.AddShipBuilderServerServices();
builder.Services.AddSingleton<HttpClient>(_ => new(new HttpClientHandler
{
    AutomaticDecompression = DecompressionMethods.All,
}));
builder.Services.AddSingleton<IAwsClient, ServerAwsClient>();
#if DEBUG
builder.Services.AddSingleton<IAppDataService, DesktopAppDataService>();
#else
builder.Services.AddSingleton<IAppDataService, ServerAppDataService>();
#endif

var app = builder.Build();
app.Services.UseMicrosoftDependencyResolver();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");

    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

var culture = AppConstants.DefaultCultureDetails.CultureInfo;
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;
Thread.CurrentThread.CurrentCulture = culture;
Thread.CurrentThread.CurrentUICulture = culture;

AppData.ShipSummaryList ??= await app.Services.GetRequiredService<IAppDataService>().GetShipSummaryList(ServerType.Live);
await app.Services.GetRequiredService<Localizer>().UpdateLanguage(AppConstants.DefaultCultureDetails, true);
var appDataService = app.Services.GetRequiredService<IAppDataService>();
if (appDataService is ServerAppDataService serverAppDataService)
{
    await serverAppDataService.FetchData();
}

app.Run();
