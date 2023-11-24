using WoWsShipBuilder.DataStructures.Modifiers;
using WoWsShipBuilder.Infrastructure.Localization;
using WoWsShipBuilder.Infrastructure.Localization.Resources;

namespace WoWsShipBuilder.Infrastructure.Utility;

public enum ReturnFilter
{
    All,
    Description,
    Value,
}

public static class ModifierProcessor
{
    private static string GetUiModifierValue(Modifier modifier, ILocalizer localizer)
    {
        var displayValue = modifier.ToDisplayValue();
        switch (modifier.Unit)
        {
            case Unit.None:
                break;
            case Unit.MPS:
                displayValue += $" {localizer.GetAppLocalization(nameof(Translation.Unit_MPS)).Localization}";
                break;
            case Unit.KG:
                displayValue += $" {localizer.GetAppLocalization(nameof(Translation.Unit_KG)).Localization}";
                break;
            case Unit.MM:
                displayValue += $" {localizer.GetAppLocalization(nameof(Translation.Unit_MM)).Localization}";
                break;
            case Unit.Percent:
                displayValue += $" {localizer.GetAppLocalization(nameof(Translation.Unit_PerCent)).Localization}";
                break;
            case Unit.Degree:
                displayValue += $" {localizer.GetAppLocalization(nameof(Translation.Unit_Degree)).Localization}";
                break;
            case Unit.S:
                displayValue += $" {localizer.GetAppLocalization(nameof(Translation.Unit_S)).Localization}";
                break;
            case Unit.PercentPerS:
                displayValue += $" {localizer.GetAppLocalization(nameof(Translation.Unit_PerCent)).Localization}/{localizer.GetAppLocalization(nameof(Translation.Unit_S)).Localization}";
                break;
            case Unit.KM:
                displayValue += $" {localizer.GetAppLocalization(nameof(Translation.Unit_KM)).Localization}";
                break;
            case Unit.DegreePerSecond:
                displayValue += $" {localizer.GetAppLocalization(nameof(Translation.Unit_DegreePerSecond)).Localization}";
                break;
            case Unit.DPS:
                displayValue += $" {localizer.GetAppLocalization(nameof(Translation.Unit_DPS)).Localization}";
                break;
            case Unit.HP:
                displayValue += $" {localizer.GetAppLocalization(nameof(Translation.Unit_HP)).Localization}";
                break;
            case Unit.Knots:
                displayValue += $" {localizer.GetAppLocalization(nameof(Translation.Unit_Knots)).Localization}";
                break;
            case Unit.M:
                displayValue += $" {localizer.GetAppLocalization(nameof(Translation.Unit_M)).Localization}";
                break;
            case Unit.ShotsPerMinute:
                displayValue += $" {localizer.GetAppLocalization(nameof(Translation.Unit_ShotsPerMinute)).Localization}";
                break;
            case Unit.FPM:
                displayValue += $" {localizer.GetAppLocalization(nameof(Translation.Unit_FPM)).Localization}";
                break;
            default:
                throw new ArgumentOutOfRangeException(null, $@"Modifier {modifier.Name} has an invalid or not assigned unit");
        }

        return displayValue;
    }

    private static string GetUiModifierDescription(Modifier modifier, ILocalizer localizer)
    {
        if (!string.IsNullOrEmpty(modifier.GameLocalizationKey))
        {
            return localizer.GetGameLocalization(modifier.GameLocalizationKey).Localization;
        }

        if (!string.IsNullOrEmpty(modifier.AppLocalizationKey))
        {
            return localizer.GetAppLocalization(modifier.AppLocalizationKey).Localization;
        }

        return string.Empty;
    }

    public static string GetUiModifierString(Modifier modifier, ReturnFilter returnFilter, ILocalizer localizer)
    {
        var value = string.Empty;
        var description = string.Empty;

        if (returnFilter is ReturnFilter.Value or ReturnFilter.All)
        {
            value = GetUiModifierValue(modifier, localizer);
        }

        if (returnFilter is ReturnFilter.Description or ReturnFilter.All)
        {
            description = GetUiModifierDescription(modifier, localizer);
        }

        if (string.IsNullOrEmpty(description.Trim()) && returnFilter != ReturnFilter.Value)
        {
            return "";
        }

        // Remove [HIDDEN] text from some skills modifiers.
        description = description.Replace("[HIDDEN]", "");
        return value + " " + description.Trim();
    }
}
