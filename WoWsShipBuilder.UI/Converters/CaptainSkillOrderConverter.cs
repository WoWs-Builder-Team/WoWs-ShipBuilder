using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using WoWsShipBuilder.Core;
using WoWsShipBuilder.DataStructures.Captain;
using WoWsShipBuilder.Infrastructure;

namespace WoWsShipBuilder.UI.Converters
{
    public class CaptainSkillOrderConverter : IMultiValueConverter
    {
        public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            string index = "";
            if (values[0] is CustomObservableCollection<Skill> skillList && values[1] is Skill skill)
            {
                index = (skillList.IndexOf(skill) + 1).ToString();
            }

            return index;
        }
    }
}
