using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace WoWsShipBuilder.Desktop.Converters
{
    public class DispPlotRotationConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var index = 0;

            if (value is bool isVertical && !isVertical)
            {
                index = 270;
            }

            return index;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
