using System;
using WoWsShipBuilder.Core.Translations;

namespace WoWsShipBuilder.Core.DataUI.DataElementAttributes;

[AttributeUsage(AttributeTargets.Property)]
public class DataElementTooltip : Attribute
{
    public DataElementTooltip(string tooltipKey)
    {
        TooltipKey = tooltipKey;
    }

    public string TooltipKey { get;}

    public string Localization
    {
        get
        {
            string? localization = Translation.ResourceManager.GetString(TooltipKey);
            return localization != null ? $" {localization}" : string.Empty;
        }
    }
}
