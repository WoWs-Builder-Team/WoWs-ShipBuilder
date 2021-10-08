using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Avalonia.Collections;
using Avalonia.Data;
using Avalonia.Data.Converters;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.UI.Converters
{
    public class ModifierConverter : IMultiValueConverter
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1123:Do not place regions within elements", Justification = "<The code is a fucking mess otherwise>")]
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            string value = "";
            string description = "";
            var prefix = "PARAMS_MODIFIER";

            if (values[0] is string localizerKey && values[1] is float modifier)
            {
                #region Value Parsing
                // This is Demolition Expert
                if (localizerKey.Contains("Bonus", StringComparison.InvariantCultureIgnoreCase))
                {
                    value = $"+{modifier * 100}%";
                }
                // This is Adrenaline Rush
                else if (localizerKey.Contains("lastChanceReloadCoefficient", StringComparison.InvariantCultureIgnoreCase))
                {
                    value = $"-{modifier}%";
                }
                // Something in Last stand. Not sure what make of it tho.
                else if (localizerKey.Contains("SGCritRudderTime"))
                {
                    value = $"+{modifier}";
                }
                else if (Math.Abs(modifier % 1) > (double.Epsilon * 100))
                {
                    Debug.WriteLine(localizerKey);
                    
                    if (modifier > 1)
                    {
                        int modifierValue = (int)(Math.Round(modifier - 1, 2) * 100);
                        value = $"+{modifierValue}%";
                    }
                    else
                    {
                        int modifierValue = (int)(Math.Round(1 - modifier, 2) * 100);
                        value = $"-{modifierValue}%";
                    }
                }
                else
                {
                    value = $"+{(int)modifier}";
                }
                #endregion

                #region Description Localization

                // There is one translation per class, but all values are equal, so we can just choose a random one. I like DDs.
                if (localizerKey.ToUpper().Equals("VISIBILITYDISTCOEFF"))
                {
                    localizerKey = $"{localizerKey}_DESTROYER";
                }

                localizerKey = $"{prefix}_{localizerKey}";

                description = Localizer.Instance[localizerKey.ToUpper()].Localization.Trim();

                if (parameter != null && parameter.Equals("PARAMS_MODIFIER") && description.Equals(localizerKey.ToUpper()))
                {
                    return "";
                }

                if (description.Equals("Reload time") || description.Equals("Consumable reload time") || description.Equals("Consumable action time") || description.Equals("Number of Shell Explosions"))
                {
                    description = Localizer.Instance[$"{localizerKey.ToUpper()}_SKILL"].Localization.Trim();
                }

                if (description.Equals($"{localizerKey.ToUpper()}_SKILL"))
                {
                    description = Localizer.Instance[$"{localizerKey.ToUpper()}_MODERNIZATION"].Localization.Trim();
                }
                #endregion
            }

            return value + " " + description;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
