using System;
using System.Collections;
using System.Globalization;
using Avalonia.Data.Converters;

namespace WoWsShipBuilder.UI.Converters
{
    public class ListVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IList list)
            {
                return list.Count > 0;
            }

            throw new NotSupportedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
