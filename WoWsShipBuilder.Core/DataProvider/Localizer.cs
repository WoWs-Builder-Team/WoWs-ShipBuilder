using System.Collections.Generic;
using NLog;

namespace WoWsShipBuilder.Core.DataProvider
{
    public class Localizer
    {
        private readonly AppDataHelper dataHelper;

        private readonly Logger logger = Logging.GetLogger("Localization");
        private Dictionary<string, string> languageData = new();
        private string locale;

        public Localizer()
            : this(AppDataHelper.Instance)
        {
        }

        public Localizer(AppDataHelper dataHelper)
        {
            this.dataHelper = dataHelper;
            locale = string.Empty;
            UpdateLanguage(AppData.Settings.Locale);
        }

        public static Localizer Instance { get; } = new();

        public string this[string key] => languageData.TryGetValue(key, out var localization) ? localization : key;

        public bool UpdateLanguage(string newLocale)
        {
            if (locale == newLocale)
            {
                logger.Info("Old and new locale are identical. Ignoring localization update.");
                return true;
            }

            locale = newLocale;

            Dictionary<string, string>? localLanguageData = dataHelper.ReadLocalizationData(AppData.Settings.SelectedServerType, locale);
            if (localLanguageData != null)
            {
                languageData = localLanguageData;
                logger.Info("Updated localization data to locale {0}.", newLocale);
                return true;
            }

            logger.Warn("Unable to load localization data for locale {0}. Keeping existing data and rejecting change.", newLocale);
            return false;
        }
    }
}
