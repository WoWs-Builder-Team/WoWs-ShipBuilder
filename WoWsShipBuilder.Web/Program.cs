using System.Globalization;
using MudBlazor;
using MudBlazor.Services;
using NLog.Web;
using Prometheus;
using ReactiveUI;
using Splat;
using Splat.Microsoft.Extensions.DependencyInjection;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Localization;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Logging.ClearProviders();
builder.Host.UseNLog(new() { RemoveLoggerFactoryFilter = false });
SetupExtensions.ConfigureNlog(builder.Configuration.GetValue<bool>("DisableLoki"));

PlatformRegistrationManager.SetRegistrationNamespaces(RegistrationNamespace.Blazor);

builder.Services.UseMicrosoftDependencyResolver();
var resolver = Locator.CurrentMutable;
resolver.InitializeSplat();
resolver.InitializeReactiveUI(RegistrationNamespace.Blazor);

builder.Services.AddMudServices(config => { config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight; });
builder.Services.AddShipBuilderServerServices();

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
app.UseHttpMetrics(options =>
{
    options.AddCustomLabel("path", context => context.Request.Path);
});

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapMetrics();
    endpoints.MapBlazorHub();
    endpoints.MapFallbackToPage("/_Host");
});

var culture = AppConstants.DefaultCultureDetails.CultureInfo;
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;
Thread.CurrentThread.CurrentCulture = culture;
Thread.CurrentThread.CurrentUICulture = culture;

// AppData.ShipSummaryList ??= await app.Services.GetRequiredService<IAppDataService>().GetShipSummaryList(ServerType.Live);
await app.Services.GetRequiredService<ILocalizationProvider>().RefreshDataAsync(ServerType.Live, AppConstants.SupportedLanguages.ToArray());
var appDataService = app.Services.GetRequiredService<IAppDataService>();
if (appDataService is ServerAppDataService serverAppDataService)
{
    await serverAppDataService.FetchData();
    AppData.ShipSummaryList = await serverAppDataService.GetShipSummaryList(ServerType.Live);
}

try
{
    app.Run();
}
finally
{
    NLog.LogManager.Shutdown();
}
