using System;

namespace WoWsShipBuilder.DataElements.DataElementAttributes;

[AttributeUsage(AttributeTargets.Property)]
public class DataElementTooltipAttribute : Attribute
{
    public DataElementTooltipAttribute(string tooltipKey)
    {
        TooltipKey = tooltipKey;
    }

    public string TooltipKey { get; }
}
