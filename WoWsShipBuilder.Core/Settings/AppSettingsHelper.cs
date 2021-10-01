using System.IO;
using Newtonsoft.Json;
using WoWsShipBuilder.Core.DataProvider;

namespace WoWsShipBuilder.Core.Settings
{
    public static class AppSettingsHelper
    {
        private static string settingFile = Path.Combine(AppDataHelper.AppDataDirectory, "settings.json");

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
