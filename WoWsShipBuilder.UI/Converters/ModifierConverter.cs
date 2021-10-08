using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Avalonia.Data.Converters;
using WoWsShipBuilder.Core.DataProvider;

namespace WoWsShipBuilder.UI.Converters
{
    public class ModifierConverter : IMultiValueConverter
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "StyleCop.CSharp.ReadabilityRules",
            "SA1123:Do not place regions within elements",
            Justification = "<The code is a fucking mess otherwise>")]
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            string value = "";
            string description = "";
            var found = false;
            var prefix = "PARAMS_MODIFIER";

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
                #region Value Parsing

                // Bonus from Depth Charge upgrade. Needs to be put as first entry because it contains the word "bonus".
                if (localizerKey.Contains("dcNumPacksBonus", StringComparison.InvariantCultureIgnoreCase))
                {
                    value = $"+{(int)modifier}";
                }

                // This is Demolition Expert
                else if (localizerKey.Contains("Bonus", StringComparison.InvariantCultureIgnoreCase))
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
                if (localizerKey.ToUpper().Equals("VISIBILITYDISTCOEFF", StringComparison.InvariantCultureIgnoreCase) ||
                    localizerKey.ToUpper().Equals("AABubbleDamage", StringComparison.InvariantCultureIgnoreCase))
                {
                    localizerKey = $"{localizerKey}_DESTROYER";
                }

                localizerKey = $"{prefix}_{localizerKey}";

                (found, description) = Localizer.Instance[localizerKey.ToUpper()];

                if (description.Equals("Reload time") || description.Equals("Consumable reload time") || description.Equals("Consumable action time") ||
                    description.Equals("Number of Shell Explosions"))
                {
                    (found, description) = Localizer.Instance[$"{localizerKey.ToUpper()}_SKILL"];
                }

                if (!found)
                {
                    (found, description) = Localizer.Instance[$"{localizerKey.ToUpper()}_MODERNIZATION"];
                }

                if (!found)
                {
                    description = "";
                }

                #endregion
            }

            if (string.IsNullOrEmpty(description.Trim()))
            {
                return "";
            }
            else
            {
                return value + " " + description.Trim();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
