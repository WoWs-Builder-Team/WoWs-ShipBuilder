using System;

namespace WoWsShipBuilder.DataElements.DataElementAttributes;

[AttributeUsage(AttributeTargets.Property)]
public class DataElementTypeAttribute : Attribute
{
    public DataElementTypeAttribute(DataElementTypes type)
    {
        Type = type;
    }

    /// <summary>
    /// Gets the type of the DataElement for the property marked by this attribute. />
    /// </summary>
    public DataElementTypes Type { get; }

    /// <summary>
    /// Gets or sets the unit localization key for the property marked by this attribute.
    /// </summary>
    public string? UnitKey { get; set; }

    /// <summary>
    /// Gets or sets the tooltip localization key for the property marked by this attribute.
    /// </summary>
    public string? TooltipKey { get; set; }

    /// <summary>
    /// Gets or sets the group localization key and identifier for the property marked by this attribute.
    /// </summary>
    public string? GroupKey { get; set; }
}
