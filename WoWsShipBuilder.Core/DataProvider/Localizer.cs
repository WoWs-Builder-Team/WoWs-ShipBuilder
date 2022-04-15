using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NLog;
using Splat;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.Core.Settings;
using WoWsShipBuilder.Core.Translations;

namespace WoWsShipBuilder.Core.DataProvider
{
    public class Localizer
    {
        private static readonly Lazy<Localizer> InstanceProducer = new(() => Locator.Current.GetService<Localizer>() ?? new(DesktopAppDataService.PreviewInstance));

        private static readonly Logger Logger = Logging.GetLogger("Localization");

        private static Localizer? instance;

        private readonly IAppDataService appDataService;

        private readonly AppSettings appSettings;

        private Dictionary<string, string> languageData = new();
        private CultureDetails? locale;

        public Localizer(IAppDataService appDataService)
        {
            this.appDataService = appDataService;
            this.appSettings = new();
        }

        public static Localizer Instance => instance ?? InstanceProducer.Value;

        public (bool IsLocalized, string Localization) this[string key] =>
            languageData.TryGetValue(key.ToUpperInvariant(), out var localization) ? (true, localization) : (false, key);

        public static void SetInstance(Localizer newInstance)
        {
            instance = newInstance;
        }

        public string GetAppLocalization(string translationKey)
        {
            return Translation.ResourceManager.GetString(translationKey, locale!.CultureInfo) ?? translationKey;
        }

        public async Task UpdateLanguage(CultureDetails newLocale, bool forceLanguageUpdate)
        {
            if (locale == newLocale && languageData.Count > 0 && !forceLanguageUpdate)
            {
                Logger.Info("Old and new locale are identical. Ignoring localization update.");
                return;
            }

            var serverType = AppData.IsInitialized ? appSettings.SelectedServerType : ServerType.Live;
            Dictionary<string, string>? localLanguageData = await appDataService.ReadLocalizationData(serverType, newLocale.LocalizationFileName);

            if (localLanguageData == null)
            {
                Logger.Warn("Unable to load localization data for locale {0}.", newLocale);
                return;
            }

            locale = newLocale;
            languageData = localLanguageData;
            Logger.Info("Updated localization data to locale {0}.", newLocale);
        }
    }
}
