using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace WoWsShipBuilder.UI.Converters
{
    public class StatGridConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not int listSize)
            {
                return new BindingNotification(new NotSupportedException());
            }

            if (targetType == typeof(GridLength))
            {
                if (parameter is "separator")
                {
                    return listSize > 0 ? new GridLength(5) : new(0);
                }

                return listSize > 0 ? GridLength.Star : new(0);
            }

            if (targetType == typeof(double))
            {
                return listSize > 0 ? 180 : 0;
            }

            return new BindingNotification(new NotSupportedException());
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return new BindingNotification(new NotSupportedException(), BindingErrorType.Error);
        }
    }
}
