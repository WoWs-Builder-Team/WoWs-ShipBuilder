using System;
using System.Collections.Generic;
using NLog;

namespace WoWsShipBuilder.Core.DataProvider
{
    public class Localizer
    {
        private static readonly Lazy<Localizer> InstanceProducer = new(() => new());
        private static readonly Logger Logger = Logging.GetLogger("Localization");

        private readonly AppDataHelper dataHelper;

        private Dictionary<string, string> languageData = new();
        private CultureDetails? locale;

        public Localizer()
            : this(AppDataHelper.Instance)
        {
        }

        public Localizer(AppDataHelper dataHelper)
        {
            this.dataHelper = dataHelper;

            // Prevent exception when using Avalonia previewer due to uninitialized settings.
            var updateLanguage = AppData.IsInitialized ? AppData.Settings.SelectedLanguage : dataHelper.DefaultCultureDetails;
            UpdateLanguage(updateLanguage, true);
        }

        public static Localizer Instance => InstanceProducer.Value;

        public (bool IsLocalized, string Localization) this[string key] =>
            languageData.TryGetValue(key.ToUpperInvariant(), out var localization) ? (true, localization) : (false, key);

        public void UpdateLanguage(CultureDetails newLocale, bool forceLanguageUpdate)
        {
            if (locale == newLocale && languageData.Count > 0 && !forceLanguageUpdate)
            {
                Logger.Info("Old and new locale are identical. Ignoring localization update.");
                return;
            }

            var serverType = AppData.IsInitialized ? AppData.Settings.SelectedServerType : ServerType.Live;
            Dictionary<string, string>? localLanguageData = dataHelper.ReadLocalizationData(serverType, newLocale.LocalizationFileName);
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
