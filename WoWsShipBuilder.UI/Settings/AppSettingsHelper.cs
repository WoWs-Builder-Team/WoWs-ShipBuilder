using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Threading;
using Splat;
using WoWsShipBuilder.Core;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilder.Core.Localization;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.Core.Settings;

namespace WoWsShipBuilder.UI.Settings
{
    [SuppressMessage("System.IO.Abstractions", "IO0002", Justification = "This class is never tested.")]
    [SuppressMessage("System.IO.Abstractions", "IO0006", Justification = "This class is never tested.")]
    public static class AppSettingsHelper
    {
        private static readonly string SettingFile = Path.Combine(DesktopAppDataService.Instance.DefaultAppDataDirectory, "settings.json");

        public static AppSettings Settings => Locator.Current.GetService<AppSettings>() ?? new();

        public static ILocalizer LocalizerInstance => Locator.Current.GetService<ILocalizer>() ?? DataHelper.DemoLocalizer;

        public static void SaveSettings() => Locator.Current.GetServiceSafe<IDataService>().StoreAsync(Settings, SettingFile);

        public static void LoadSettings()
        {
            if (File.Exists(SettingFile))
            {
                Logging.Logger.Info("Trying to load settings from settings file...");
                var settings = Locator.Current.GetServiceSafe<IDataService>().Load<AppSettings>(SettingFile);
                if (settings == null)
                {
                    Logging.Logger.Error("Unable to parse local settings file. Creating empty settings instance.");
                    settings = new();
                }

                Settings.UpdateFromSettings(settings);
            }
            else
            {
                Logging.Logger.Info("No settings file found, creating new settings...");
                Settings.UpdateFromSettings(new());
            }

            AppData.IsInitialized = true;
            Logging.Logger.Info("Settings initialized.");
            UpdateThreadCulture(Settings.SelectedLanguage.CultureInfo);
        }

        private static void UpdateThreadCulture(CultureInfo cultureInfo)
        {
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;
        }
    }
}
