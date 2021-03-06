using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data;
using Avalonia.Data.Converters;
using WoWsShipBuilder.Core.Localization;
using WoWsShipBuilder.DataElements.DataElements;
using WoWsShipBuilder.UI.Settings;

namespace WoWsShipBuilder.UI.Converters;

public class FormattedTextDataElementConverter : IValueConverter
{
    private readonly ILocalizer localizer;

    public FormattedTextDataElementConverter()
    {
        localizer = AppSettingsHelper.LocalizerInstance;
    }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not FormattedTextDataElement formattedTextDataElement)
        {
            return new BindingNotification(new NotSupportedException(), BindingErrorType.Error);
        }

        string text = formattedTextDataElement.Text;
        if (formattedTextDataElement.IsTextKey)
        {
            text = formattedTextDataElement.IsTextAppLocalization ? localizer.GetAppLocalization(text).Localization : localizer.GetGameLocalization(text).Localization;
        }

        IEnumerable<string> values = formattedTextDataElement.Values;
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

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return new BindingNotification(new NotSupportedException(), BindingErrorType.Error);
    }
}
