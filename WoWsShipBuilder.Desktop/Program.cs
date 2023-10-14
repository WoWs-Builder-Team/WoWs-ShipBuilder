using System;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NLog.Extensions.Logging;
using Sentry;
using Squirrel;
using WoWsShipBuilder.Desktop.Infrastructure;
using WoWsShipBuilder.Desktop.Infrastructure.StaticConfiguration;
using WoWsShipBuilder.Infrastructure.ApplicationData;
using WoWsShipBuilder.Infrastructure.Localization;

namespace WoWsShipBuilder.Desktop;

internal sealed class Program
{
    private static ILogger<Program> logger = NullLogger<Program>.Instance;

    [STAThread]
    public static void Main(string[] args) => RunProgram(args).GetAwaiter().GetResult();

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    private static async Task RunProgram(string[] args)
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Logging.ClearProviders();
        builder.Logging.SetMinimumLevel(LogLevel.Trace);
        builder.Logging.AddNLog(LoggingSetup.CreateLoggingConfiguration(), new() { ParseMessageTemplates = true });
        builder.UseShipBuilderDesktop();

        using var app = builder.Build();
        AppData.WebMode = false;
        await app.StartAsync();

        logger = app.Services.GetRequiredService<ILogger<Program>>();
        LocalizeConverter.InitializeLocalizer(app.Services.GetRequiredService<ILocalizer>());

        var avaloniaApp = BuildAvaloniaApp(app.Services);

        SentrySdk.Init(ApplicationSettings.ApplicationOptions.SentryDsn);

        if (OperatingSystem.IsWindows())
        {
            logger.LogDebug("Operating system is windows");
            SquirrelAwareApp.HandleEvents(onInitialInstall: OnInitialInstall, onAppUninstall: OnAppUninstall, onEveryRun: OnEveryRun);
        }

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

    private static void OnEveryRun(SemanticVersion version, IAppTools tools, bool firstRun)
    {
        tools.SetProcessAppUserModelId();
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp(IServiceProvider services)
        => AppBuilder.Configure<App>(() => new() { Services = services })
            .UsePlatformDetect()
            .LogToTrace()
            .UseSkia()
            .UseReactiveUI();

    public static AppBuilder BuildAvaloniaApp() => BuildAvaloniaApp(CreatePreviewServiceProvider());

    [SupportedOSPlatform("windows")]
    private static void OnAppUninstall(SemanticVersion version, IAppTools tools)
    {
        logger.LogInformation("App is uninstalling...");
        tools.RemoveShortcutForThisExe();
    }

    [SupportedOSPlatform("windows")]
    private static void OnInitialInstall(SemanticVersion version, IAppTools tools)
    {
        logger.LogInformation("App has been installed, creating shortcuts...");
        try
        {
            tools.CreateShortcutForThisExe();
            logger.LogInformation("Shortcuts have been created.");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error during installation");
            throw;
        }
    }

    private static IServiceProvider CreatePreviewServiceProvider()
    {
        return new ServiceCollection().AddLogging(builder => builder.ClearProviders()).BuildServiceProvider();
    }
}
