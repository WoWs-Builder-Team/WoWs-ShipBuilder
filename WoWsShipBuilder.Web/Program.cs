using System.Globalization;
using NLog.Web;
using Prometheus;
using ReactiveUI;
using Splat;
using Splat.Microsoft.Extensions.DependencyInjection;
using WoWsShipBuilder.Infrastructure;
using WoWsShipBuilder.Web.Infrastructure;
using WoWsShipBuilder.Web.Infrastructure.Data;
using WoWsShipBuilder.Web.Infrastructure.Metrics;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor(options =>
{
    options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(10);
});
builder.ConfigureShipBuilderOptions();

builder.Logging.ClearProviders();
builder.Host.UseNLog(new() { RemoveLoggerFactoryFilter = false, ParseMessageTemplates = true });
SetupExtensions.ConfigureNlog(builder.Configuration.GetValue<bool>("DisableLoki"), builder.Environment.IsDevelopment());

PlatformRegistrationManager.SetRegistrationNamespaces(RegistrationNamespace.Blazor);

builder.Services.UseMicrosoftDependencyResolver();
var resolver = Locator.CurrentMutable;
resolver.InitializeSplat();
resolver.InitializeReactiveUI(RegistrationNamespace.Blazor);

builder.Services.UseShipBuilderWeb();

var app = builder.Build();
app.Services.UseMicrosoftDependencyResolver();
Logging.Initialize(app.Services.GetRequiredService<ILoggerFactory>());

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();
app.UseHttpMetrics(options =>
{
    options.AddCustomLabel("path", context => context.Request.Path);
});
app.UseReferrerTracking();

app.MapControllers();
app.MapMetrics();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

var culture = AppConstants.DefaultCultureDetails.CultureInfo;
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;
Thread.CurrentThread.CurrentCulture = culture;
Thread.CurrentThread.CurrentUICulture = culture;

var dataInitializer = app.Services.GetRequiredService<DataInitializer>();
await dataInitializer.InitializeData();

try
{
    app.Run();
}
finally
{
    NLog.LogManager.Shutdown();
}
