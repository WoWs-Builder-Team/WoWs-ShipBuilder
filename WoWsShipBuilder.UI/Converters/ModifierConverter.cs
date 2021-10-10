using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using Avalonia.Data.Converters;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.UI.Translations;

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

                // Incoming fire alert. Range is in BigWorld Unit
                else if(localizerKey.Contains("artilleryAlertMinDistance", StringComparison.InvariantCultureIgnoreCase))
                {
                    value = $"{(modifier * 30) / 1000} Km";
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
                    // If Modifier is higher than 1000, we can assume it's in meter, so we convert it to Km for display purposes
                    if (modifier > 1000)
                    {
                        value = $"+{modifier / 1000} Km";
                    }
                    else
                    {
                        value = $"+{(int)modifier}";
                    }
                }

                #endregion

                #region Description Localization

                // There is one translation per class, but all values are equal, so we can just choose a random one. I like DDs.
                if (localizerKey.ToUpper().Equals("VISIBILITYDISTCOEFF", StringComparison.InvariantCultureIgnoreCase) || 
                    localizerKey.ToUpper().Equals("AABubbleDamage", StringComparison.InvariantCultureIgnoreCase) ||
                    localizerKey.ToUpper().Equals("GMROTATIONSPEED", StringComparison.InvariantCultureIgnoreCase))
                {
                    localizerKey = $"{localizerKey}_DESTROYER";
                }

                localizerKey = $"{prefix}_{localizerKey}";

                bool found;
                (found, description) = Localizer.Instance[localizerKey.ToUpper()];

                // We need this to deal with the consumable mod of slot 5
                var moduleFallback = "";

                if (description.Equals("Reload time") || description.Equals("Consumable reload time") || description.Equals("Consumable action time") ||
                    description.Equals("Number of Shell Explosions"))
                {
                    moduleFallback = description;
                    (found, description) = Localizer.Instance[$"{localizerKey.ToUpper()}_SKILL"];
                }

                if (!found)
                {
                    (found, description) = Localizer.Instance[$"{localizerKey.ToUpper()}_MODERNIZATION"];
                }

                if (!found)
                {
                    if (!string.IsNullOrEmpty(moduleFallback))
                    {
                        description = moduleFallback;
                    }
                    else
                    {
                        description = "";
                    }
                }

                if (localizerKey.Contains("artilleryAlertMinDistance", StringComparison.InvariantCultureIgnoreCase))
                {
                    description = Translation.ResourceManager.GetString("IncomingFireAlertDesc", Translation.Culture)!;
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
