using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace WoWsShipBuilder.UI.Converters
{
    // TODO: remove warning suppression
    public class EnumConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
#pragma warning disable CS8603 // Possible null reference return.
            return value?.Equals(parameter);
#pragma warning restore CS8603 // Possible null reference return.
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
#pragma warning disable CS8603 // Possible null reference return.
            return value?.Equals(true) == true ? parameter : BindingOperations.DoNothing;
#pragma warning restore CS8603 // Possible null reference return.
        }
    }
}
