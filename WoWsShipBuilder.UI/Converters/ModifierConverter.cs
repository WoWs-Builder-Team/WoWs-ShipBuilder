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
        // private enum ReturnFilter
        // {
        //     All,
        //     Description,
        //     Value,
        // }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "StyleCop.CSharp.ReadabilityRules",
            "SA1123:Do not place regions within elements",
            Justification = "<The code is a fucking mess otherwise>")]
        public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            // string value = "";
            // string description = "";
            // var prefix = "PARAMS_MODIFIER_";

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
