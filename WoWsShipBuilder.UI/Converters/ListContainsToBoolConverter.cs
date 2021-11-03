using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Data.Converters;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.UI.Converters
{
    public class ListContainsToBoolConverter : IMultiValueConverter
    {
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            bool contains = false;
            if (values[0] is AvaloniaList<Skill> skillList && values[1] is Skill skill)
            {
                contains = skillList.Contains(skill);
            }

            return contains;
        }
    }
}
