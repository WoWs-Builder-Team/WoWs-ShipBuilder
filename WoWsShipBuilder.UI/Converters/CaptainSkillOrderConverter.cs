using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Avalonia.Data.Converters;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.UI.Converters
{
    public class CaptainSkillOrderConverter : IMultiValueConverter
    {
        public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            string index = "";
            if (values[0] is ObservableCollection<Skill> skillList && values[1] is Skill skill)
            {
                index = (skillList.IndexOf(skill) + 1).ToString();
            }

            return index;
        }
    }
}
