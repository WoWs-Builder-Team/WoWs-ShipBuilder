using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Collections;
using Avalonia.Data.Converters;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.UI.Converters
{
    public class ModernizationListToIndexConverter : IMultiValueConverter
    {
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is List<Modernization> modernizations && values[1] is AvaloniaList<Modernization> selectedModernizations)
            {
                foreach (Modernization currentModernization in selectedModernizations)
                {
                    int index = modernizations.FindIndex(m => m.Index.Equals(currentModernization.Index, StringComparison.InvariantCultureIgnoreCase));
                    if (index >= 0)
                    {
                        return index + 1;
                    }
                }
            }

            return 0;
        }
    }
}
