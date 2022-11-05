using System;
using System.Diagnostics;
using WoWsShipBuilder.Core.Localization;

namespace WoWsShipBuilder.Core.DataContainers;

public enum ReturnFilter
{
    All,
    Description,
    Value,
}

public static class ModifierProcessor
{
    private static string GetUiModifierValue(float modifier, string localizerKey, ReturnFilter returnFilter, ILocalizer localizer)
    {
        string value;

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

            // custom modifier to show hp per heal
            case { } str when str.Contains("hpPerHeal", StringComparison.InvariantCultureIgnoreCase):
                value = $"+{(int)modifier}";
                break;

            // Bonus from Depth Charge upgrade. Needs to be put as first entry because it contains the word "bonus".
            case { } str when str.Contains("dcNumPacksBonus", StringComparison.InvariantCultureIgnoreCase):
                value = $"+{(int)modifier}";
                break;

            case { } str when str.Contains("prioritySectorStrengthBonus", StringComparison.InvariantCultureIgnoreCase):
                value = $"+{(int)modifier} %";
                break;

            // this is for Vigilance for BBs
            case { } str when str.Contains("uwCoeffBonus", StringComparison.InvariantCultureIgnoreCase) ||
                                     str.Contains("ignorePTZBonus", StringComparison.InvariantCultureIgnoreCase):
                value = $"+{(int)modifier} %";
                break;

            // This is for IFHE. At the start because of DE sharing similar modifier name
            case { } str when str.Contains("burnChanceFactorHighLevel", StringComparison.InvariantCultureIgnoreCase) ||
                                     str.Contains("burnChanceFactorLowLevel", StringComparison.InvariantCultureIgnoreCase):
                value = $"-{(int)Math.Round(modifier * 100)} %";
                break;

            // this is for HP module
            case { } str when str.Contains("AAMaxHP", StringComparison.InvariantCultureIgnoreCase) ||
                              str.Contains("GSMaxHP", StringComparison.InvariantCultureIgnoreCase) ||
                              str.Contains("SGCritRudderTime", StringComparison.InvariantCultureIgnoreCase):
            {
                var numericValue = (int)(Math.Round(modifier * 100, 2) - 100);
                value = numericValue > 0 ? $"+{numericValue} %" : $"{numericValue} %";
                break;
            }

            // wg doesn't know how math works. -x% drain rate != +x% drain rate
            case { } str when str.Contains("planeForsageDrainRate", StringComparison.InvariantCultureIgnoreCase):
            {
                double numericValue = Math.Round(((1 / modifier) - 1) * 100, 2);
                value = numericValue > 0 ? $"+{numericValue} %" : $"{numericValue} %";
                break;
            }

            // this is for midway leg mod. more accurate numbers
            case { } str when str.Contains("diveBomberMaxSpeedMultiplier", StringComparison.InvariantCultureIgnoreCase) ||
                              str.Contains("diveBomberMinSpeedMultiplier", StringComparison.InvariantCultureIgnoreCase):
                value = modifier > 1 ? $"+{Math.Round((modifier - 1) * 100, 2)} %" : $"-{Math.Round((1 - modifier) * 100, 2)} %";
                break;

            // this is for aiming time of CV planes
            case { } str when str.Contains("AimingTime", StringComparison.InvariantCultureIgnoreCase):
                value = modifier > 0 ? $"+{modifier}{localizer.GetAppLocalization(nameof(Translation.Unit_S)).Localization}" : $"{modifier}{localizer.GetAppLocalization(nameof(Translation.Unit_S)).Localization}";
                break;

            // This is the anti detonation stuff
            case { } str when str.Contains("PMDetonationProb", StringComparison.InvariantCultureIgnoreCase):
            {
                var numericValue = (int)(Math.Round(modifier * 100, 2) - 100);
                value = numericValue > 0 ? $"+{numericValue} %" : $"{numericValue} %";
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
                value = $"+{Math.Round(modifier * 100, 1)}%";
                if (str.Contains("regenerationRate", StringComparison.InvariantCultureIgnoreCase))
                {
                        value += $"/{localizer.GetAppLocalization(nameof(Translation.Unit_S)).Localization}";
                }

                break;
            }

            // This is Adrenaline Rush
            case { } str when str.Contains("lastChanceReloadCoefficient", StringComparison.InvariantCultureIgnoreCase):
                value = $"-{modifier} %";
                break;

            // Something in Last stand. Not sure what make of it tho.
            case { } str when str.Contains("SGCritRudderTime"):
                value = $"+{modifier}";
                break;

            // Incoming fire alert. Range is in BigWorld Unit
            case { } str when str.Contains("artilleryAlertMinDistance", StringComparison.InvariantCultureIgnoreCase):
                value = $"{(modifier * 30) / 1000} {localizer.GetAppLocalization(nameof(Translation.Unit_KM)).Localization}";
                break;

            // Radar and hydro spotting distances
            case { } str when str.Contains("distShip", StringComparison.InvariantCultureIgnoreCase) ||
                              str.Contains("distTorpedo", StringComparison.InvariantCultureIgnoreCase):
                value = $"{Math.Round(modifier * 30) / 1000} {localizer.GetAppLocalization(nameof(Translation.Unit_KM)).Localization}";
                break;

            // Speed boost modifier
            case { } str when str.Equals("boostCoeff", StringComparison.InvariantCultureIgnoreCase):
                value = $"+{Math.Round(modifier * 100)} %";
                break;

            case { } str when str.Contains("fightersNum", StringComparison.InvariantCultureIgnoreCase):
                value = $"{modifier}";
                break;

            // this is the modifier
            case { } str when str.Contains("CALLFIGHTERStimeDelayAttack", StringComparison.InvariantCultureIgnoreCase):
                value = $"-{Math.Round((1 - modifier) * 100)} %";
                break;

            // this is the actual value
            case { } str when str.Contains("timeDelayAttack", StringComparison.InvariantCultureIgnoreCase):
                value = $"{modifier} {Translation.Unit_S}";
                break;
            case { } str when str.Contains("radius"):
                value = $"{Math.Round(modifier * 30 / 1000, 1)} {localizer.GetAppLocalization(nameof(Translation.Unit_KM)).Localization}";
                break;

            case { } str when Math.Abs(modifier % 1) > (double.Epsilon * 100) ||
                                     str.Contains("WorkTimeCoeff", StringComparison.InvariantCultureIgnoreCase) ||
                                     str.Contains("smokeGeneratorLifeTime"):
            {
                if (modifier > 1)
                {
                    var modifierValue = (decimal)Math.Round((modifier - 1) * 100, 2);
                    value = $"+{modifierValue} %";
                }
                else
                {
                    var modifierValue = (decimal)Math.Round((1 - modifier) * 100, 2);
                    value = $"-{modifierValue} %";
                }

                break;
            }

            case { } str when str.Contains("lifeTime", StringComparison.InvariantCultureIgnoreCase) ||
                              str.Contains("timeFromHeaven", StringComparison.InvariantCultureIgnoreCase):
                value = $"{modifier} {localizer.GetAppLocalization(nameof(Translation.Unit_S)).Localization}";
                break;

            default:
                // If Modifier is higher than 1000, we can assume it's in meter, so we convert it to Km for display purposes
                value = modifier > 1000 ? $"+{modifier / 1000} {localizer.GetAppLocalization(nameof(Translation.Unit_KM)).Localization}" : $"+{(int)modifier}";
                break;
        }

        return value;
    }

    private static string GetUiModifierDescription(string localizerKey, ILocalizer localizer)
    {
        string prefix = "PARAMS_MODIFIER_";

        if (localizerKey.Contains("regenerationHPSpeedUnits", StringComparison.InvariantCultureIgnoreCase))
        {
            return "";
        }

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

        if (localizerKey.Equals("timeDelayAttack", StringComparison.InvariantCultureIgnoreCase))
        {
            localizerKey = $"CALLFIGHTERS{localizerKey}";
        }

        localizerKey = $"{prefix}{localizerKey}";

        (bool found, string description) = localizer.GetGameLocalization(localizerKey.ToUpper());

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

        if (localizerKey.Contains("timeFromHeaven", StringComparison.InvariantCultureIgnoreCase))
        {
            description = localizer.GetGameLocalization("PARAMS_MODIFIER_CALLFIGHTERSAPPEARDELAY").Localization;
        }

        if (localizerKey.Contains("regenerationRate", StringComparison.InvariantCultureIgnoreCase))
        {
            description += "/s";
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
           description = localizer.GetAppLocalization(nameof(Translation.Consumable_HpPerHeal)).Localization;
        }

        return description;
    }

    public static string GetUiModifierString(string localizerKey, float modifier, ReturnFilter returnFilter, ILocalizer localizer)
    {
        var value = string.Empty;
        var description = string.Empty;

        if (returnFilter is ReturnFilter.Value or ReturnFilter.All)
        {
            value = GetUiModifierValue(modifier, localizerKey, returnFilter, localizer);
        }

        if (returnFilter is ReturnFilter.Description or ReturnFilter.All)
        {
            description = GetUiModifierDescription(localizerKey, localizer);
        }

        if (string.IsNullOrEmpty(description.Trim()) && returnFilter != ReturnFilter.Value)
        {
            return "";
        }
        else
        {
            // Remove [HIDDEN] text from some skills modifiers.
            description = description.Replace("[HIDDEN]", "");
            return value + " " + description.Trim();
        }
    }

    public static string GetUiModifierString(string localizerKey, double modifier, ReturnFilter returnFilter, ILocalizer localizer)
    {
        return GetUiModifierString(localizerKey, (float)modifier, returnFilter, localizer);
    }
}
