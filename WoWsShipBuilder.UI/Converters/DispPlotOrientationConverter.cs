using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Layout;

namespace WoWsShipBuilder.UI.Converters
{
    public class DispPlotOrientationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var index = Orientation.Horizontal;

            if (value is bool isVertical && !isVertical)
            {
                index = Orientation.Vertical;
            }

            return index;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
