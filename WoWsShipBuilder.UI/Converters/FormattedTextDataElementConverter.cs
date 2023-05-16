using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using WoWsShipBuilder.DataElements.DataElements;
using WoWsShipBuilder.Infrastructure.Localization;
using WoWsShipBuilder.Infrastructure.Utility;
using AppSettingsHelper = WoWsShipBuilder.UI.Settings.AppSettingsHelper;

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

        try
        {
            return FormattedTextHelper.ConvertFormattedText(formattedTextDataElement, localizer);
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
