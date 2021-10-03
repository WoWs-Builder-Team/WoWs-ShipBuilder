using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Collections;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace WoWsShipBuilder.UI.Converters
{
    public class CaptainSkillOrderConverter : IMultiValueConverter
    {

        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            var skillList = values[0] as AvaloniaList<string>;
            var skillName = values[1] as string;
            string index = "";
            if (skillList != null)
            {
                index = (skillList.IndexOf(skillName) + 1).ToString();
            }

            return index;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
