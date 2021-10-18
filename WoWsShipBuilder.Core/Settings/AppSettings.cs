using System;
using System.Globalization;
using System.Threading;
using WoWsShipBuilder.Core.DataProvider;

namespace WoWsShipBuilder.Core.Settings
{
    public class AppSettings
    {
        private string locale = "en-GB";

        public bool AutoUpdateEnabled { get; set; } = true;

        public string Locale
        {
            get => locale;
            set
            {
                var culture = new CultureInfo(value);
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
                locale = value;

                if (!AppData.IsInitialized)
                {
                    Logging.Logger.Debug("Initialization attempted before full app data setup. Ignoring language update.");
                }
                else
                {
                    Localizer.Instance.UpdateLanguage(value);
                }
            }
        }

        public ServerType SelectedServerType { get; set; } = ServerType.Live;

        public DateTime LastDataUpdateCheck { get; set; } = DateTime.Today;

        public DateTime LastVersionUpdateCheck { get; set; } = DateTime.Now;
    }
}
