using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using ReactiveUI.Avalonia;
using Sentry;
using Velopack;
using Velopack.Logging;
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
            var velopackLogger = new VelopackLogger(app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Velopack"));
            VelopackApp.Build().SetLogger(velopackLogger).Run();
        }

        RunProgram(app, args).GetAwaiter().GetResult();
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp(IServiceProvider services)
    {
        return AppBuilder.Configure<App>(() => new() { Services = services })
            .UsePlatformDetect()
            .LogToTrace()
            .UseSkia()
            .UseReactiveUI();
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        return BuildAvaloniaApp(CreatePreviewServiceProvider());
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

    private static IServiceProvider CreatePreviewServiceProvider()
    {
        return new ServiceCollection().AddLogging(builder => builder.ClearProviders()).BuildServiceProvider();
    }

    private sealed class VelopackLogger(ILogger logger) : IVelopackLogger
    {
        public void Log(VelopackLogLevel logLevel, string? message, Exception? exception)
        {
            var level = logLevel switch
            {
                VelopackLogLevel.Trace => LogLevel.Trace,
                VelopackLogLevel.Debug => LogLevel.Debug,
                VelopackLogLevel.Information => LogLevel.Information,
                VelopackLogLevel.Warning => LogLevel.Warning,
                VelopackLogLevel.Error => LogLevel.Error,
                VelopackLogLevel.Critical => LogLevel.Critical,
                _ => throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null),
            };

            logger.Log(level, exception, "{Message}", message);
        }
    }
}
