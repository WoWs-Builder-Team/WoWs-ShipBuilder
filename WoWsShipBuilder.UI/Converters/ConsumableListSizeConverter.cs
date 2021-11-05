using System;
using System.Collections;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace WoWsShipBuilder.UI.Converters
{
    public class ConsumableListSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IList list && parameter is string targetSizeString)
            {
                int targetSize = int.Parse(targetSizeString);
                if (targetSize >= 0)
                {
                    return list.Count >= targetSize;
                }

                return list.Count < Math.Abs(targetSize);
            }

            return new BindingNotification(new NotSupportedException());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new BindingNotification(new NotSupportedException());
        }
    }
}
