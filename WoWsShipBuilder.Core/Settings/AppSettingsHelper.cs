using System.Diagnostics.CodeAnalysis;
using System.IO;
using Newtonsoft.Json;
using WoWsShipBuilder.Core.DataProvider;

namespace WoWsShipBuilder.Core.Settings
{
    [SuppressMessage("System.IO.Abstractions", "IO0002", Justification = "This class is never tested.")]
    [SuppressMessage("System.IO.Abstractions", "IO0006", Justification = "This class is never tested.")]
    public static class AppSettingsHelper
    {
        private static string settingFile = Path.Combine(AppDataHelper.Instance.AppDataDirectory, "settings.json");

        public static void SaveSettings()
        {
            var settingString = JsonConvert.SerializeObject(AppData.Settings);
            File.WriteAllText(settingFile, settingString); 
        }

        public static void LoadSettings()
        {
            var jsonSettings = File.ReadAllText(settingFile);
            var settings = JsonConvert.DeserializeObject<AppSettings>(jsonSettings);
            AppData.Settings = settings;
        }
    }
}
