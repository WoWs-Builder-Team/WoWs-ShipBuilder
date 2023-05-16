using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace WoWsShipBuilder.Desktop.Converters;

public class BooleanToBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue && targetType == typeof(IBrush))
        {
            return boolValue ? Brushes.Gold : Brushes.Transparent;
        }

        return new BindingNotification(new NotSupportedException());
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return new BindingNotification(new NotSupportedException());
    }
}
