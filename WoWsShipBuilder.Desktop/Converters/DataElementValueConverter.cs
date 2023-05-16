using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using WoWsShipBuilder.Desktop.Settings;
using WoWsShipBuilder.Infrastructure.Localization;

namespace WoWsShipBuilder.Desktop.Converters;

public class DataElementValueConverter : IMultiValueConverter
{
    private readonly ILocalizer localizer;

    public DataElementValueConverter()
    {
        localizer = AppSettingsHelper.LocalizerInstance;
    }

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values[0] is not null && values[1] is bool isValueKey && values[2] is bool isAppLocalizationKey)
        {
            object? value = values[0];
            if (!isValueKey)
            {
                return value;
            }

            if (values[0] is string key)
            {
                return (isAppLocalizationKey ? localizer.GetAppLocalization(key) : localizer.GetGameLocalization(key)).Localization;
            }
        }

        return new BindingNotification(new NotSupportedException(), BindingErrorType.Error);
    }
}
