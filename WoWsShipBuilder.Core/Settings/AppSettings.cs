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
                Localizer.Instance.UpdateLanguage(value);
            }
        }

        public ServerType SelectedServerType { get; set; } = ServerType.Live;
    }
}
