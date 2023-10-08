namespace WoWsShipBuilder.Data.Generator.DataElementGenerator.Model;

internal sealed record PropertyDisplayOptions(string? UnitKey, string LocalizationKey, string? TooltipKey, string? GroupKey, bool TreatValueAsLocalizationKey, bool TreatValueAsAppLocalizationKey);
