using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Avalonia.Data.Converters;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.DataUI;
using WoWsShipBuilder.Core.Translations;

namespace WoWsShipBuilder.UI.Converters
{
    public class ModifierConverter : IMultiValueConverter
    {
        public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            var returnFilter = ReturnFilter.All;
            if (parameter is string stringParam)
            {
                if (stringParam.Contains("desc", StringComparison.InvariantCultureIgnoreCase))
                {
                    returnFilter = ReturnFilter.Description;
                }
                else if (stringParam.Contains("value", StringComparison.InvariantCultureIgnoreCase))
                {
                    returnFilter = ReturnFilter.Value;
                }
            }

            float modifier = 0;
            var modifierInitialized = false;

            switch (values[1])
            {
                case float floatMod:
                    modifier = floatMod;
                    modifierInitialized = true;
                    break;
                case double doubleMod:
                    modifier = (float)doubleMod;
                    modifierInitialized = true;
                    break;
            }

            if (values[0] is string localizerKey && modifierInitialized)
            {
                return ModifierProcessor.GetUiModifierString(localizerKey, modifier, returnFilter);
            }

            return "";
        }
    }
}
