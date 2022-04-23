using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Splat;
using WoWsShipBuilder.Core.DataUI.DataElements;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilder.Core.Localization;

namespace WoWsShipBuilder.UI.Converters;

public class FormattedTextDataElementConverter : IValueConverter
{
    private readonly ILocalizer localizer;

    public FormattedTextDataElementConverter()
    {
        localizer = Locator.Current.GetServiceSafe<ILocalizer>();
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is FormattedTextDataElement formattedTextDataElement)
        {
            var text = formattedTextDataElement.Text;
            if (formattedTextDataElement.IsTextKey)
            {
                text = formattedTextDataElement.IsTextAppLocalization ? localizer.GetAppLocalization(text).Localization : localizer.GetGameLocalization(text).Localization;
            }

            var values = formattedTextDataElement.Values;
            if (formattedTextDataElement.AreValuesKeys)
            {
                values = formattedTextDataElement.AreValuesAppLocalization ? values.Select(x => localizer.GetAppLocalization(x).Localization) : values.Select(x => localizer.GetGameLocalization(x).Localization);
            }

            try
            {
                return string.Format(text, values.Cast<object>().ToArray());
            }
            catch (Exception e)
            {
                return new BindingNotification(e, BindingErrorType.Error);
            }
        }

        return new BindingNotification(new NotSupportedException(), BindingErrorType.Error);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return new BindingNotification(new NotSupportedException(), BindingErrorType.Error);
    }
}
