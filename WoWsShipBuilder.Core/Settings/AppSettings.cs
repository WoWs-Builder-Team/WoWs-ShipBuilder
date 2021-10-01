using System.Globalization;
using System.Threading;

namespace WoWsShipBuilder.Core.Settings
{
    public class AppSettings
    {
        private string locale = "en_GB";

        public bool AutoUpdateEnabled { get; set; } = true;

        public string Locale
        {
            get => locale;
            set
            {
                var culture = new CultureInfo(value);
                Thread.CurrentThread.CurrentCulture = culture;
                locale = value;
            }
        }
    }
}
