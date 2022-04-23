﻿using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Splat;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilder.Core.Localization;

namespace WoWsShipBuilder.UI.Converters;

public class DataElementValueConverter : IMultiValueConverter
{
    private readonly ILocalizer localizer;

    public DataElementValueConverter()
    {
        localizer = Locator.Current.GetServiceSafe<ILocalizer>();
    }

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values[0] is not null && values[1] is bool isValueKey && values[2] is bool isAppLocalizationKey)
        {
            var value = values[0];
            if (!isValueKey)
            {
                return value;
            }

            if (values[0] is string key)
            {
                return isAppLocalizationKey ? localizer.GetAppLocalization(key) : localizer.GetGameLocalization(key);
            }
        }

        return new BindingNotification(new NotSupportedException(), BindingErrorType.Error);
    }
}
