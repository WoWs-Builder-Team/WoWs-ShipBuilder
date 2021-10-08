using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Avalonia.Collections;
using Avalonia.Data;
using Avalonia.Data.Converters;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.UI.Converters
{
    public class ModifierBonusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is float modifier)
            {
                if (Math.Abs(modifier % 1) > (double.Epsilon * 100))
                {
                    if (modifier > 1)
                    {
                        int modifierValue = (int)(Math.Round(modifier - 1, 2) * 100);
                        return $"+{modifierValue}%";
                    }
                    else
                    {
                        int modifierValue = (int)(Math.Round(1 - modifier, 2) * 100);
                        return $"-{modifierValue}%";
                    } 
                }
                else
                {
                    return $"+{(int)modifier}";
                }
            }

            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
