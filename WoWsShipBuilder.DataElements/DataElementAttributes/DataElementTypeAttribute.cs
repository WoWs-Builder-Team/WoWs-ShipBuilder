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
    /// Gets or sets the unit localization key for the property marked by this attribute. <br/>
    /// Only valid for <see cref="DataElementTypes.KeyValueUnit"/>.
    /// </summary>
    public string? UnitKey { get; set; }

    /// <summary>
    /// Gets or sets the tooltip localization key for the property marked by this attribute. <br/>
    /// Only valid for <see cref="DataElementTypes.Tooltip"/>.
    /// </summary>
    public string? TooltipKey { get; set; }

    /// <summary>
    /// Gets or sets the group localization key and identifier for the property marked by this attribute. <br/>
    /// Only valid for <see cref="DataElementTypes.Grouped"/>.
    /// </summary>
    public string? GroupKey { get; set; }

    /// <summary>
    /// Gets or sets the property names of the values to be used by the string property marked by this attribute. Requires the value of the property marked by thus attribute to follow the <see cref="string.Format(string,object[])"/> specifications. <br/>
    /// Only valid for <see cref="DataElementTypes.FormattedText"/>.
    /// </summary>
    public string? ValuesPropertyName { get; set; }

    /// <summary>
    /// Gets or sets if the value of the property marked by this attribute is a localization key. <br/>
    /// Only valid for <see cref="DataElementTypes.FormattedText"/>
    /// </summary>
    public bool? IsValueLocalizationKey { get; set; } = false;

    /// <summary>
    /// Gets or sets if the values indicated by <see cref="ValuesPropertyName"/> are localization keys. <br/>
    /// Only valid for <see cref="DataElementTypes.FormattedText"/>
    /// </summary>
    public bool? ArePropertyNameValuesKeys { get; set; } = false;

    /// <summary>
    /// Gets or sets if the value of the property marked by this attribute is an app localization key. <br/>
    /// Only valid for <see cref="DataElementTypes.FormattedText"/>
    /// </summary>
    public bool? IsValueAppLocalization { get; set; }

    /// <summary>
    ///  Gets or sets if the values indicated by <see cref="ValuesPropertyName"/> are app localization keys.<br/>
    /// Only valid for <see cref="DataElementTypes.FormattedText"/>
    /// </summary>
    public bool? IsPropertyNameValuesAppLocalization { get; set; }

}
