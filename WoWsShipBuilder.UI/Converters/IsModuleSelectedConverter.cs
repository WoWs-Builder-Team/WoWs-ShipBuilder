using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Collections;
using Avalonia.Data.Converters;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.UI.Converters
{
    /// <summary>
    /// Converter to determine whether a ship upgrade is part of a list of selected upgrades.
    /// </summary>
    public class IsModuleSelectedConverter : IMultiValueConverter
    {
        /// <summary>
        /// Processes data from a multi binding and returns a boolean indicating whether the module is one of the selected upgrades.
        /// </summary>
        /// <param name="values">A list of values. First item should be the list of selected upgrades, the second one should be the current upgrade.</param>
        /// <param name="targetType">The target type of the conversion.</param>
        /// <param name="parameter">The converter parameter.</param>
        /// <param name="culture">The converter culture.</param>
        /// <returns><see langword="true"/> if the module is part of the selected modules, <see langword="false"/> otherwise.</returns>
        /// <exception cref="NotSupportedException">Occurs if the content of the provided value list does not match the expected types.</exception>
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is AvaloniaList<ShipUpgrade> shipUpgrades && values[1] is ShipUpgrade thisUpgrade)
            {
                return shipUpgrades.Contains(thisUpgrade);
            }

            throw new NotSupportedException();
        }
    }
}
