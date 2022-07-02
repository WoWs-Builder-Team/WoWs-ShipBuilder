using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Avalonia.Data.Converters;
using Splat;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilder.Core.Localization;
using WoWsShipBuilder.Core.Translations;
using WoWsShipBuilder.UI.Settings;

namespace WoWsShipBuilder.UI.Converters
{
    public class ModifierConverter : IMultiValueConverter
    {
        private readonly ILocalizer localizer;

        public ModifierConverter()
        {
            localizer = AppSettingsHelper.LocalizerInstance;
        }

        private enum ReturnFilter
        {
            All,
            Description,
            Value,
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "StyleCop.CSharp.ReadabilityRules",
            "SA1123:Do not place regions within elements",
            Justification = "<The code is a fucking mess otherwise>")]
        public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            var value = "";
            var description = "";
            var prefix = "PARAMS_MODIFIER_";

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
                #region Value Parsing

                if (returnFilter is ReturnFilter.All or ReturnFilter.Description)
                {
                    Debug.WriteLine("Key: " + localizerKey);
                    Debug.WriteLine("Modifier: " + modifier);
                }

                // Because removing unused things is too hard, right WG?
                if (localizerKey.Contains("[UNUSED]", StringComparison.InvariantCultureIgnoreCase) ||
                    localizerKey.Contains("torpedoDetectionCoefficientByPlane", StringComparison.InvariantCultureIgnoreCase))
                {
                    return "";
                }

                switch (localizerKey)
                {
                    // defAA modifiers
                    case { } str when str.Contains("bubbleDamageMultiplier"):
                        value = $"+{(modifier - 1) * 100} {Translation.Unit_PerCent}";
                        break;

                    case { } str when str.Contains("areaDamageMultiplier"):
                        value = $"+{(modifier - 1) * 100} {Translation.Unit_PerCent}";
                        break;

                    // custom modifier to show hp per heal
                    case { } str when str.Contains("hpPerHeal", StringComparison.InvariantCultureIgnoreCase):
                        value = $"+{(int)modifier}";
                        break;

                    // Bonus from Depth Charge upgrade. Needs to be put as first entry because it contains the word "bonus".
                    case { } str when str.Contains("dcNumPacksBonus", StringComparison.InvariantCultureIgnoreCase):
                        value = $"+{(int)modifier}";
                        break;

                    case { } str when str.Contains("prioritySectorStrengthBonus", StringComparison.InvariantCultureIgnoreCase):
                        value = $"+{(int)modifier} {Translation.Unit_PerCent}";
                        break;

                    // this is for Vigilance for BBs
                    case { } str when str.Contains("uwCoeffBonus", StringComparison.InvariantCultureIgnoreCase) ||
                                             str.Contains("ignorePTZBonus", StringComparison.InvariantCultureIgnoreCase):
                        value = $"+{(int)modifier} {Translation.Unit_PerCent}";
                        break;

                    // This is for IFHE. At the start because of DE sharing similar modifier name
                    case { } str when str.Contains("burnChanceFactorHighLevel", StringComparison.InvariantCultureIgnoreCase) ||
                                             str.Contains("burnChanceFactorLowLevel", StringComparison.InvariantCultureIgnoreCase):
                        value = $"-{(int)Math.Round(modifier * 100)} {Translation.Unit_PerCent}";
                        break;

                    // this is for HP module
                    case { } str when str.Contains("AAMaxHP", StringComparison.InvariantCultureIgnoreCase) ||
                                      str.Contains("GSMaxHP", StringComparison.InvariantCultureIgnoreCase) ||
                                      str.Contains("SGCritRudderTime", StringComparison.InvariantCultureIgnoreCase):
                    {
                        var numericValue = (int)(Math.Round(modifier * 100, 2) - 100);
                        value = numericValue > 0 ? $"+{numericValue} {Translation.Unit_PerCent}" : $"{numericValue} {Translation.Unit_PerCent}";
                        break;
                    }

                    // wg doesn't know how math works. -x% drain rate != +x% drain rate
                    case { } str when str.Contains("planeForsageDrainRate", StringComparison.InvariantCultureIgnoreCase):
                    {
                        double numericValue = Math.Round(((1 / modifier) - 1) * 100, 2);
                        value = numericValue > 0 ? $"+{numericValue} {Translation.Unit_PerCent}" : $"{numericValue} {Translation.Unit_PerCent}";
                        break;
                    }

                    // this is for midway leg mod. more accurate numbers
                    case { } str when str.Contains("diveBomberMaxSpeedMultiplier", StringComparison.InvariantCultureIgnoreCase) ||
                                      str.Contains("diveBomberMinSpeedMultiplier", StringComparison.InvariantCultureIgnoreCase):
                        value = modifier > 1 ? $"+{Math.Round((modifier - 1) * 100, 2)} {Translation.Unit_PerCent}" : $"-{Math.Round((1 - modifier) * 100, 2)} {Translation.Unit_PerCent}";
                        break;

                    // this is for aiming time of CV planes
                    case { } str when str.Contains("AimingTime", StringComparison.InvariantCultureIgnoreCase):
                        value = modifier > 0 ? $"+{modifier} {Translation.Unit_S}" : $"{modifier} {Translation.Unit_S}";
                        break;

                    // This is the anti detonation stuff
                    case { } str when str.Contains("PMDetonationProb", StringComparison.InvariantCultureIgnoreCase):
                    {
                        var numericValue = (int)(Math.Round(modifier * 100, 2) - 100);
                        value = numericValue > 0 ? $"+{numericValue} {Translation.Unit_PerCent}" : $"{numericValue} {Translation.Unit_PerCent}";
                        break;
                    }

                    // This is Demolition Expert. And also flags. Imagine having similar name for a modifier doing the same thing.
                    // Also applies to repair party bonus.
                    // UPDATE: remember what i said about similar names? Wanna take a guess how they did captain talents?
                    case { } str when str.Contains("Bonus", StringComparison.InvariantCultureIgnoreCase) ||
                                      str.Contains("burnChanceFactor", StringComparison.InvariantCultureIgnoreCase) ||
                                      str.Contains("regenerationHPSpeed", StringComparison.InvariantCultureIgnoreCase) ||
                                      (str.Contains("regenerationRate", StringComparison.InvariantCultureIgnoreCase) && !returnFilter.Equals(ReturnFilter.All)):
                    {
                        value = $"+{Math.Round(modifier * 100, 1)} {Translation.Unit_PerCent}";
                        if (str.Contains("regenerationRate", StringComparison.InvariantCultureIgnoreCase))
                        {
                                value += $"/{Translation.Unit_S}";
                        }

                        break;
                    }

                    // This is Adrenaline Rush
                    case { } str when str.Contains("lastChanceReloadCoefficient", StringComparison.InvariantCultureIgnoreCase):
                        value = $"-{modifier} {Translation.Unit_PerCent}";
                        break;

                    // Something in Last stand. Not sure what make of it tho.
                    case { } str when str.Contains("SGCritRudderTime"):
                        value = $"+{modifier}";
                        break;

                    // Incoming fire alert. Range is in BigWorld Unit
                    case { } str when str.Contains("artilleryAlertMinDistance", StringComparison.InvariantCultureIgnoreCase):
                        value = $"{(modifier * 30) / 1000} {Translation.Unit_KM}";
                        break;

                    // Radar and hydro spotting distances
                    case { } str when str.Contains("distShip", StringComparison.InvariantCultureIgnoreCase) ||
                                      str.Contains("distTorpedo", StringComparison.InvariantCultureIgnoreCase):
                        value = $"{Math.Round(modifier * 30) / 1000} {Translation.Unit_KM}";
                        break;

                    // Speed boost modifier
                    case { } str when str.Equals("boostCoeff", StringComparison.InvariantCultureIgnoreCase):
                        value = $"+{Math.Round(modifier * 100)} {Translation.Unit_PerCent}";
                        break;

                    // fighters modifiers
                    case { } str when str.Contains("fightersNum", StringComparison.InvariantCultureIgnoreCase):
                        value = $"{modifier}";
                        break;

                    case { } str when str.Contains("cruisingSpeed", StringComparison.InvariantCultureIgnoreCase):
                        value = $"{modifier} {Translation.Unit_Knots}";
                        break;

                    case { } str when str.Contains("maxViewDistance", StringComparison.InvariantCultureIgnoreCase):
                        value = $"{modifier} {Translation.Unit_KM}";
                        break;

                    case { } str when str.Contains("concealment", StringComparison.InvariantCultureIgnoreCase):
                        value = $"{modifier} {Translation.Unit_KM}";
                        break;

                    case { } str when str.Contains("dogFightTime", StringComparison.InvariantCultureIgnoreCase):
                        value = $"{modifier} {Translation.Unit_S}";
                        break;

                    case { } str when str.Contains("maxKills", StringComparison.InvariantCultureIgnoreCase):
                        value = $"{modifier}";
                        break;

                    // this is the modifier
                    case { } str when str.Contains("CALLFIGHTERStimeDelayAttack", StringComparison.InvariantCultureIgnoreCase):
                        value = $"-{Math.Round((1 - modifier) * 100)} {Translation.Unit_PerCent}";
                        break;

                    // this is the actual value
                    case { } str when str.Contains("timeDelayAttack", StringComparison.InvariantCultureIgnoreCase):
                        value = $"{modifier} {Translation.Unit_S}";
                        prefix += "CALLFIGHTERS";
                        break;

                    case { } str when str.Contains("radius"):
                        value = $"{Math.Round(modifier * 30 / 1000, 1)} {Translation.Unit_KM}";
                        break;

                    case { } str when str.Contains("lifeTime", StringComparison.InvariantCultureIgnoreCase) ||
                                             str.Contains("timeFromHeaven", StringComparison.InvariantCultureIgnoreCase):
                        value = $"{modifier} {Translation.Unit_S}";
                        break;

                    case { } str when Math.Abs(modifier % 1) > (double.Epsilon * 100) ||
                                             str.Contains("WorkTimeCoeff", StringComparison.InvariantCultureIgnoreCase):
                    {
                        if (modifier > 1)
                        {
                            var modifierValue = (decimal)Math.Round((modifier - 1) * 100, 2);
                            value = $"+{modifierValue} {Translation.Unit_PerCent}";
                        }
                        else
                        {
                            var modifierValue = (decimal)Math.Round((1 - modifier) * 100, 2);
                            value = $"-{modifierValue} {Translation.Unit_PerCent}";
                        }

                        break;
                    }

                    // this is for TRB
                    case { } str when str.Contains("torpedoReloadTime", StringComparison.InvariantCultureIgnoreCase):
                        value = $"{modifier} {Translation.Unit_S}";
                        break;

                    default:
                        // If Modifier is higher than 1000, we can assume it's in meter, so we convert it to Km for display purposes
                        value = modifier > 1000 ? $"+{modifier / 1000} {Translation.Unit_KM}" : $"+{(int)modifier}";
                        break;
                }

                #endregion

                if (returnFilter == ReturnFilter.Value)
                {
                    return value;
                }

                #region Description Localization

                // There is one translation per class, but all values are equal, so we can just choose a random one. I like DDs.
                if (localizerKey.ToUpper().Equals("VISIBILITYDISTCOEFF", StringComparison.InvariantCultureIgnoreCase) ||
                    localizerKey.ToUpper().Equals("AABubbleDamage", StringComparison.InvariantCultureIgnoreCase) ||
                    localizerKey.ToUpper().Equals("AAAuraDamage", StringComparison.InvariantCultureIgnoreCase) ||
                    localizerKey.ToUpper().Equals("GMROTATIONSPEED", StringComparison.InvariantCultureIgnoreCase) ||
                    localizerKey.ToUpper().Equals("dcAlphaDamageMultiplier", StringComparison.InvariantCultureIgnoreCase) ||
                    localizerKey.ToUpper().Equals("ConsumableReloadTime", StringComparison.InvariantCultureIgnoreCase))
                {
                    localizerKey = $"{localizerKey}_DESTROYER";
                }

                if (localizerKey.Equals("talentMaxDistGM", StringComparison.InvariantCultureIgnoreCase))
                {
                    localizerKey = "GMMAXDIST";
                }

                if (localizerKey.Equals("talentConsumablesWorkTime", StringComparison.InvariantCultureIgnoreCase))
                {
                    localizerKey = "ConsumablesWorkTime";
                }

                localizerKey = $"{prefix}{localizerKey}";

                bool found;
                (found, description) = localizer.GetGameLocalization(localizerKey.ToUpper());

                // We need this to deal with the consumable mod of slot 5
                var moduleFallback = "";

                if (localizerKey.Contains("ReloadCoeff", StringComparison.InvariantCultureIgnoreCase) ||
                    localizerKey.Contains("WorkTimeCoeff", StringComparison.InvariantCultureIgnoreCase) ||
                    localizerKey.Contains("AAEXTRABUBBLES", StringComparison.InvariantCultureIgnoreCase) ||
                    localizerKey.Contains("callFightersAdditionalConsumables", StringComparison.InvariantCultureIgnoreCase))
                {
                    moduleFallback = description;
                    (found, description) = localizer.GetGameLocalization($"{localizerKey.ToUpper()}_SKILL");
                }

                if (!found)
                {
                    (found, description) = localizer.GetGameLocalization($"{localizerKey.ToUpper()}_MODERNIZATION");
                }

                if (!found)
                {
                    description = !string.IsNullOrEmpty(moduleFallback) ? moduleFallback : "";
                }

                if (localizerKey.Contains("artilleryAlertMinDistance", StringComparison.InvariantCultureIgnoreCase))
                {
                    description = Translation.ResourceManager.GetString("IncomingFireAlertDesc", Translation.Culture)!;
                }

                if (localizerKey.Contains("timeFromHeaven", StringComparison.InvariantCultureIgnoreCase))
                {
                    description = localizer.GetGameLocalization("PARAMS_MODIFIER_CALLFIGHTERSAPPEARDELAY").Localization;
                }

                if (localizerKey.Contains("regenerationRate", StringComparison.InvariantCultureIgnoreCase))
                {
                    description += $"/{Translation.Unit_S}";
                }

                if (localizerKey.Contains("SHIPSPEEDCOEFF", StringComparison.InvariantCultureIgnoreCase))
                {
                    description = localizer.GetGameLocalization("PARAMS_MODIFIER_SHIPSPEEDCOEFFFORRIBBONS").Localization;
                }

                if (localizerKey.Contains("burnProbabilityBonus", StringComparison.InvariantCultureIgnoreCase))
                {
                    description = localizer.GetGameLocalization("PARAMS_MODIFIER_MAINGAUGEBURNPROBABILITYFORCAPTURE").Localization;
                }

                if (localizerKey.Contains("hpPerHeal", StringComparison.InvariantCultureIgnoreCase))
                {
                    description = Translation.Consumable_HpPerHeal;
                }

                if (localizerKey.Contains("dogFightTime", StringComparison.InvariantCultureIgnoreCase))
                {
                    description = Translation.ModifierConverter_MaxEngagementDuration;
                }

                if (localizerKey.Contains("maxKills", StringComparison.InvariantCultureIgnoreCase))
                {
                    description = Translation.ModifierConverter_MaxKillsAmount;
                }

                if (localizerKey.Contains("cruisingSpeed", StringComparison.InvariantCultureIgnoreCase))
                {
                    description = Translation.ShipStats_Speed;
                }

                if (localizerKey.Contains("maxViewDistance", StringComparison.InvariantCultureIgnoreCase))
                {
                    description = Translation.ShipStats_MaxViewDistance;
                }

                if (localizerKey.Contains("concealment", StringComparison.InvariantCultureIgnoreCase))
                {
                    description = Translation.ShipStats_Concealment;
                }

                if (returnFilter == ReturnFilter.Description)
                {
                    return description;
                }

                #endregion
            }

            if (string.IsNullOrEmpty(description.Trim()))
            {
                return "";
            }

            // Remove [HIDDEN] text from some skills modifiers.
            description = description.Replace("[HIDDEN]", "");
            return value + " " + description.Trim();
        }
    }
}
