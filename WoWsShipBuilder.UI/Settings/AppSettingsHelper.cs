using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Logging;
using Splat;
using WoWsShipBuilder.Common.Infrastructure;
using WoWsShipBuilder.Common.Infrastructure.Data;
using WoWsShipBuilder.Common.Infrastructure.Localization;
using WoWsShipBuilder.Common.Settings;
using WoWsShipBuilder.Core;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Localization;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.UI.Extensions;

namespace WoWsShipBuilder.UI.Settings;

[SuppressMessage("System.IO.Abstractions", "IO0002", Justification = "This class is never tested.")]
[SuppressMessage("System.IO.Abstractions", "IO0006", Justification = "This class is never tested.")]
public static class AppSettingsHelper
{
    private static readonly string SettingFile = Path.Combine(Locator.Current.GetRequiredService<IAppDataService>().DefaultAppDataDirectory, "settings.json");

    public static AppSettings Settings => Locator.Current.GetService<AppSettings>() ?? new();

    public static ILocalizer LocalizerInstance => Locator.Current.GetService<ILocalizer>() ?? DesignDataHelper.DemoLocalizer;

    public static string BuildImageOutputDirectory => Settings.CustomImagePath ?? Locator.Current.GetRequiredService<IDataService>().CombinePaths(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), AppConstants.ShipBuilderName);

    public static void SaveSettings() => Locator.Current.GetRequiredService<IDataService>().StoreAsync(Settings, SettingFile);

    public static void LoadSettings()
    {
        if (File.Exists(SettingFile))
        {
            Logging.Logger.LogInformation("Trying to load settings from settings file...");
            var settings = Locator.Current.GetRequiredService<IDataService>().Load<AppSettings>(SettingFile);
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

    private static void UpdateThreadCulture(CultureInfo cultureInfo)
    {
        Thread.CurrentThread.CurrentCulture = cultureInfo;
        Thread.CurrentThread.CurrentUICulture = cultureInfo;
    }
}
