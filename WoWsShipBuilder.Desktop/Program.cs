using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Sentry;
using Velopack;
using WoWsShipBuilder.Desktop.Infrastructure;
using WoWsShipBuilder.Desktop.Infrastructure.StaticConfiguration;
using WoWsShipBuilder.Infrastructure.ApplicationData;
using WoWsShipBuilder.Infrastructure.Localization;

namespace WoWsShipBuilder.Desktop;

internal sealed class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Logging.ClearProviders();
        builder.Logging.SetMinimumLevel(LogLevel.Trace);
        builder.Logging.AddNLog(LoggingSetup.CreateLoggingConfiguration(), new() { ParseMessageTemplates = true });
        builder.UseShipBuilderDesktop();
        using var app = builder.Build();
        if (OperatingSystem.IsWindows())
        {
            VelopackApp.Build().Run(app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Velopack"));
        }

        RunProgram(app, args).GetAwaiter().GetResult();
    }

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    private static async Task RunProgram(IHost app, string[] args)
    {
        AppData.WebMode = false;
        await app.StartAsync();

        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        LocalizeConverter.InitializeLocalizer(app.Services.GetRequiredService<ILocalizer>());

        var avaloniaApp = BuildAvaloniaApp(app.Services);

        SentrySdk.Init(ApplicationSettings.ApplicationOptions.SentryDsn);

        logger.LogDebug("------------------------------");
        logger.LogDebug("Starting application...");
        var culture = AppConstants.DefaultCultureDetails.CultureInfo;
        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;

        try
        {
            avaloniaApp.StartWithClassicDesktopLifetime(args);
        }
        catch (Exception e)
        {
            logger.LogCritical(e, "Encountered a critical error that will end the application.");
            throw;
        }
        finally
        {
            logger.LogInformation("Application is shutting down.");
            logger.LogInformation("------------------------------\n");
            await app.StopAsync();
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp(IServiceProvider services)
        => AppBuilder.Configure<App>(() => new() { Services = services })
            .UsePlatformDetect()
            .LogToTrace()
            .UseSkia()
            .UseReactiveUI();

    public static AppBuilder BuildAvaloniaApp() => BuildAvaloniaApp(CreatePreviewServiceProvider());

    private static IServiceProvider CreatePreviewServiceProvider()
    {
        return new ServiceCollection().AddLogging(builder => builder.ClearProviders()).BuildServiceProvider();
    }
}
