using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using WoWsShipBuilder.Desktop.Services;

namespace WoWsShipBuilder.Desktop.Converters;

public class UpdateStatusBooleanConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is UpdateStatus updateStatus && parameter is UpdateStatus targetStatus)
        {
            return targetStatus == updateStatus;
        }

        return new BindingNotification(new NotSupportedException("Invalid argument types"));
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return new BindingNotification(new NotSupportedException());
    }
}
