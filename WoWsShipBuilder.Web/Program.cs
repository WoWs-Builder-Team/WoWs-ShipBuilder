using System.Globalization;
using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ReactiveUI;
using Splat;
using Splat.Microsoft.Extensions.DependencyInjection;
using WoWsShipBuilder.Web;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.UseMicrosoftDependencyResolver();
var resolver = Locator.CurrentMutable;
resolver.InitializeSplat();
resolver.InitializeReactiveUI();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new(builder.HostEnvironment.BaseAddress) });
builder.Services.AddBlazorise(options => { options.Immediate = true; })
    .AddBootstrap5Providers()
    .AddFontAwesomeIcons();

var currentCulture = new CultureInfo("en-GB");
CultureInfo.DefaultThreadCurrentCulture = currentCulture;
CultureInfo.DefaultThreadCurrentUICulture = currentCulture;
await builder.Build().RunAsync();
