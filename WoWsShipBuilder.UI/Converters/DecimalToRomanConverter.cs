using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace WoWsShipBuilder.UI.Converters
{
    public class DecimalToRomanConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            string shipTier = string.Empty;
            if (value is int tier)
            {
                shipTier = tier switch
                {
                    1 => "I",
                    2 => "II",
                    3 => "III",
                    4 => "IV",
                    5 => "V",
                    6 => "VI",
                    7 => "VII",
                    8 => "VIII",
                    9 => "IX",
                    10 => "X",
                    11 => "XI",
                    _ => "?",
                };
            }

            return shipTier;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
