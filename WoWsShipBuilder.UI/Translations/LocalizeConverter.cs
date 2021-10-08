using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using WoWsShipBuilder.Core.DataProvider;

namespace WoWsShipBuilder.UI.Translations
{
    /// <summary>
    /// An <see cref="IValueConverter"/> that allows to convert a string to its localized version.
    /// Accesses the localizations provided by <see cref="Localizer"/>.
    /// </summary>
    public class LocalizeConverter : IValueConverter
    {
        /// <summary>
        /// Converts a string to the localized string for the currently selected language.
        /// </summary>
        /// <param name="value">The object to convert. Should be a string.</param>
        /// <param name="targetType">The target type of the conversion. Should be of type string.</param>
        /// <param name="parameter">The conversion parameter. Ignored by this converter.</param>
        /// <param name="culture">The conversion culture. Ignored by this converter, use the selected locale of<see cref="AppData.Settings"/> instead.</param>
        /// <returns>The localized string or the provided value if there is no localization available.</returns>
        /// <exception cref="NotSupportedException">Occurs if the provided value is not a string and the target type is not string either.</exception>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string localizerKey && targetType.IsAssignableFrom(typeof(string)))
            {
                if (parameter is not string stringParam)
                {
                    return Localizer.Instance[localizerKey];
                }

                return stringParam.StartsWith('_') ? Localizer.Instance[localizerKey + stringParam] : Localizer.Instance[stringParam + localizerKey];
            }

            return new BindingNotification(new NotSupportedException(), BindingErrorType.Error, value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new BindingNotification(new NotSupportedException(), BindingErrorType.Error, value);
        }
    }
}
