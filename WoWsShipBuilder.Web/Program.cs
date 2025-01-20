using System.Globalization;
using NLog.Web;
using Prometheus;
using ReactiveUI;
using WoWsShipBuilder.Infrastructure.ApplicationData;
using WoWsShipBuilder.Infrastructure.Utility;
using WoWsShipBuilder.Web.Features.Authentication;
using WoWsShipBuilder.Web.Features.Host;
using WoWsShipBuilder.Web.Infrastructure;
using WoWsShipBuilder.Web.Infrastructure.Data;
using WoWsShipBuilder.Web.Infrastructure.Metrics;
using _Imports = WoWsShipBuilder._Imports;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// builder.Services.AddRazorPages(options =>
// {
//     options.RootDirectory = "/Features/Host";
// });
// builder.Services.AddServerSideBlazor(options =>
// {
//     options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(10);
// });
builder.Services.AddControllers();
builder.ConfigureShipBuilderOptions();

builder.Logging.ClearProviders();
builder.Host.UseNLog(new() { RemoveLoggerFactoryFilter = false, ParseMessageTemplates = true });
SetupExtensions.ConfigureNlog(builder.Environment.IsDevelopment());

PlatformRegistrationManager.SetRegistrationNamespaces(RegistrationNamespace.Blazor);

builder.Services.UseShipBuilderWeb();
builder.Services.AddCookieAuth();

builder.Services.AddServerSideBlazor().AddHubOptions(opt => opt.MaximumReceiveMessageSize = null); //needed because of this https://github.com/MudBlazor/MudBlazor/issues/7263

var app = builder.Build();
Logging.Initialize(app.Services.GetRequiredService<ILoggerFactory>());

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseHttpMetrics(options =>
{
    options.AddCustomLabel("path", context => context.Request.Path);
});
app.UseReferrerTracking();
app.UseShipBuilderAuth();
app.UseAntiforgery();

app.MapControllers();
app.MapMetrics();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(_Imports).Assembly);

var culture = AppConstants.DefaultCultureDetails.CultureInfo;
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;
Thread.CurrentThread.CurrentCulture = culture;
Thread.CurrentThread.CurrentUICulture = culture;

var dataInitializer = app.Services.GetRequiredService<DataInitializer>();
await dataInitializer.InitializeData();

try
{
    await app.RunAsync();
}
finally
{
    NLog.LogManager.Shutdown();
}
