using System;
using System.Globalization;
using Avalonia.Collections;
using Avalonia.Data.Converters;

namespace WoWsShipBuilder.UI.Converters
{
    public class CaptainSkillOrderConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not AvaloniaList<string> skillList || parameter is not string skillName || !targetType.IsAssignableFrom(typeof(int)))
            {
                throw new NotSupportedException();
            }

            var index = skillList.IndexOf(skillName) + 1;
            
            return index;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
