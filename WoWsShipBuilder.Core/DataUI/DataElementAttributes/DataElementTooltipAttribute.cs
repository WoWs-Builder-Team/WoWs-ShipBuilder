using System;

namespace WoWsShipBuilder.Core.DataUI.DataElementAttributes;

[AttributeUsage(AttributeTargets.Property)]
public class DataElementTooltipAttribute : Attribute
{
    public DataElementTooltipAttribute(string tooltipKey)
    {
        TooltipKey = tooltipKey;
    }

    public string TooltipKey { get; }
}
