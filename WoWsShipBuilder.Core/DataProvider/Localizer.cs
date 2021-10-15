using System;
using System.Collections.Generic;
using NLog;

namespace WoWsShipBuilder.Core.DataProvider
{
    public class Localizer
    {
        private static readonly Lazy<Localizer> InstanceProducer = new(() => new Localizer());
        private static readonly Logger Logger = Logging.GetLogger("Localization");

        private readonly AppDataHelper dataHelper;

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

        public static Localizer Instance { get; } = InstanceProducer.Value;

        public (bool IsLocalized, string Localization) this[string key] =>
            languageData.TryGetValue(key.ToUpperInvariant(), out var localization) ? (true, localization) : (false, key);

        public bool UpdateLanguage(string newLocale)
        {
            if (locale == newLocale)
            {
                Logger.Info("Old and new locale are identical. Ignoring localization update.");
                return true;
            }

            locale = newLocale;

            Dictionary<string, string>? localLanguageData = dataHelper.ReadLocalizationData(AppData.Settings.SelectedServerType, locale);
            if (localLanguageData != null)
            {
                languageData = localLanguageData;
                Logger.Info("Updated localization data to locale {0}.", newLocale);
                return true;
            }

            Logger.Warn("Unable to load localization data for locale {0}. Keeping existing data and rejecting change.", newLocale);
            return false;
        }
    }
}
