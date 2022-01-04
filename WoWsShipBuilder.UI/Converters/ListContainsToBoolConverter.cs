using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace WoWsShipBuilder.UI.Converters
{
    public class ListContainsToBoolConverter : IMultiValueConverter
    {
        public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            bool contains = false;
            if (values[0] is IList skillList && values[1] is object item)
            {
                if (item is UnsetValueType)
                {
                    return false;
                }
                else
                {
                    contains = skillList.Contains(item);
                }
            }

            return contains;
        }
    }
}
