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
    /// Only valid for <see cref="DataElementTypes.KeyValueUnit"/> and <see cref="DataElementTypes.Tooltip"/>.
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
    /// Gets or set the name of the property containing the list of values that will replace the placeholder. Requires the value of the property marked by this attribute to follow the <see cref="string.Format(string,object[])"/> specifications. <br/>
    /// Only valid for <see cref="DataElementTypes.FormattedText"/>.
    /// </summary>
    public string? ValuesPropertyName { get; set; }

    /// <summary>
    /// Gets or sets if the value of the property marked by this attribute is a localization key. <br/>
    /// Only valid for <see cref="DataElementTypes.Value"/>, <see cref="DataElementTypes.KeyValue"/> and <see cref="DataElementTypes.FormattedText"/>
    /// </summary>
    public bool IsValueLocalizationKey { get; set; }

    /// <summary>
    /// Gets or sets if the values indicated by <see cref="ValuesPropertyName"/> are localization keys. <br/>
    /// Only valid for <see cref="DataElementTypes.FormattedText"/>
    /// </summary>
    public bool ArePropertyNameValuesKeys { get; set; }

    /// <summary>
    /// Gets or sets if the value of the property marked by this attribute is an app localization key. <br/>
    /// Only valid for <see cref="DataElementTypes.Value"/>, <see cref="DataElementTypes.KeyValue"/> and <see cref="DataElementTypes.FormattedText"/>
    /// </summary>
    public bool IsValueAppLocalization { get; set; }

    /// <summary>
    /// Gets or sets if the values indicated by <see cref="ValuesPropertyName"/> are app localization keys.<br/>
    /// Only valid for <see cref="DataElementTypes.FormattedText"/>
    /// </summary>
    public bool IsPropertyNameValuesAppLocalization { get; set; }

}
