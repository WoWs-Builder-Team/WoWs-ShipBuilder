using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Microsoft.Extensions.Logging;
using WoWsShipBuilder.Common.Infrastructure;
using WoWsShipBuilder.Common.Infrastructure.Localization;
using WoWsShipBuilder.UI.Settings;

namespace WoWsShipBuilder.UI.Translations
{
    /// <summary>
    /// An <see cref="IValueConverter"/> that allows to convert a string to its localized version.
    /// Accesses the localizations provided by <see cref="ILocalizer"/>.
    /// </summary>
    public class LocalizeConverter : IValueConverter
    {
        /// <summary>
        /// Converts a string to the localized string for the currently selected language.
        /// </summary>
        /// <param name="value">The object to convert. Should be a string.</param>
        /// <param name="targetType">The target type of the conversion. Should be of type string.</param>
        /// <param name="parameter">The conversion parameter. Ignored by this converter.</param>
        /// <param name="culture">The conversion culture. Ignored by this converter, use the selected locale of <see cref="ApplicationSettings"/> instead.</param>
        /// <returns>The localized string or the provided value if there is no localization available.</returns>
        /// <exception cref="NotSupportedException">Occurs if the provided value is not a string and the target type is not string either.</exception>
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string localizerKey && targetType.IsAssignableFrom(typeof(string)))
            {
                if (parameter is not string stringParam)
                {
                    string localization = AppSettingsHelper.LocalizerInstance.GetGameLocalization(localizerKey).Localization.Trim();
                    return !string.IsNullOrEmpty(localization) ? localization : "noName";
                }

                if (stringParam.Equals("RESX", StringComparison.InvariantCultureIgnoreCase))
                {
                    string? localization = Translation.ResourceManager.GetString(localizerKey, culture);

                    // TODO: fix properly by handling Unit property in TooltipDataElement generation
                    if (localization == null && !string.IsNullOrWhiteSpace(localizerKey))
                    {
                        Logging.Logger.LogWarning("Missing localization for key {LocalizerKey}", localizerKey);
                        Debug.WriteLine(localizerKey);
                    }

                    return localization ?? localizerKey;
                }

                if (localizerKey.Contains("Placeholder", StringComparison.InvariantCultureIgnoreCase))
                {
                    return Translation.ResourceManager.GetString($"{stringParam}_{localizerKey}", culture) ?? string.Empty;
                }

                if (stringParam.Equals("SKILL") || stringParam.Equals("SKILL_DESC"))
                {
                    localizerKey = ToSnakeCase(localizerKey);
                }

                LocalizationResult result;
                if (stringParam.StartsWith('_'))
                {
                    result = AppSettingsHelper.LocalizerInstance.GetGameLocalization(localizerKey + stringParam);
                }
                else
                {
                    if (!stringParam.EndsWith("_"))
                    {
                        stringParam += "_";
                    }

                    result = AppSettingsHelper.LocalizerInstance.GetGameLocalization(stringParam + localizerKey);
                }

                return result.Localization.Trim();
            }

            if (value is Enum enumValue)
            {
                var enumString = enumValue.ToString();
                return Translation.ResourceManager.GetString(enumString, culture) ?? enumString;
            }

            return new BindingNotification(new NotSupportedException(), BindingErrorType.Error, value);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return new BindingNotification(new NotSupportedException(), BindingErrorType.Error, value);
        }

        private static string ToSnakeCase(string camelCaseString)
        {
            if (camelCaseString == null)
            {
                throw new ArgumentNullException(nameof(camelCaseString));
            }

            if (camelCaseString.Length < 2)
            {
                return camelCaseString;
            }

            var sb = new StringBuilder();
            sb.Append(char.ToLowerInvariant(camelCaseString[0]));
            for (var i = 1; i < camelCaseString.Length; ++i)
            {
                char c = camelCaseString[i];
                if (char.IsUpper(c))
                {
                    sb.Append('_');
                    sb.Append(char.ToLowerInvariant(c));
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }
    }
}
