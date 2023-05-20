using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.Features.Settings;
using WoWsShipBuilder.Infrastructure;
using WoWsShipBuilder.Infrastructure.Data;
using WoWsShipBuilder.Infrastructure.Localization;

namespace WoWsShipBuilder.Desktop.Settings;

[SuppressMessage("System.IO.Abstractions", "IO0002", Justification = "This class is never tested.")]
[SuppressMessage("System.IO.Abstractions", "IO0006", Justification = "This class is never tested.")]
public static class AppSettingsHelper
{
    private static IAppDataService appDataService = default!;

    private static AppSettings? settings;

    private static ILocalizer? localizer;

    private static IDataService dataService = default!;

    private static string SettingFile => Path.Combine(appDataService.DefaultAppDataDirectory, "settings.json");

    public static AppSettings Settings => settings ?? new();

    public static ILocalizer LocalizerInstance => localizer ?? DesignDataHelper.DemoLocalizer;

    public static string BuildImageOutputDirectory => Settings.CustomImagePath ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), AppConstants.ShipBuilderName);

    public static void SaveSettings() => dataService.StoreAsync(Settings, SettingFile);

    public static void LoadSettings(IServiceProvider services)
    {
        if (File.Exists(SettingFile))
        {
            Logging.Logger.LogInformation("Trying to load settings from settings file...");
            var settings = dataService.Load<AppSettings>(SettingFile);
            if (settings == null)
            {
                Logging.Logger.LogError("Unable to parse local settings file. Creating empty settings instance");
                settings = new();
            }

            Settings.UpdateFromSettings(settings);
        }
        else
        {
            Logging.Logger.LogInformation("No settings file found, creating new settings...");
            Settings.UpdateFromSettings(new());
        }

        AppData.IsInitialized = true;
        Logging.Logger.LogInformation("Settings initialized");
        UpdateThreadCulture(Settings.SelectedLanguage.CultureInfo);
    }

    public static void Initialize(IServiceProvider services)
    {
        appDataService = services.GetRequiredService<IAppDataService>();
        settings = services.GetRequiredService<AppSettings>();
        localizer = services.GetRequiredService<ILocalizer>();
        dataService = services.GetRequiredService<IDataService>();
    }

    private static void UpdateThreadCulture(CultureInfo cultureInfo)
    {
        Thread.CurrentThread.CurrentCulture = cultureInfo;
        Thread.CurrentThread.CurrentUICulture = cultureInfo;
    }
}
