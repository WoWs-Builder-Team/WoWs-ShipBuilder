namespace WoWsShipBuilder.Data.Generator.Attributes;

public static class AttributeGenerator
{

    public const string DataElementTypesEnum = @"
namespace WoWsShipBuilder.DataElements.DataElementAttributes;

[Flags]
public enum DataElementTypes
{
    KeyValue = 1,
    KeyValueUnit = 2,
    Value = 4,
    Grouped = 8,
    Tooltip = 16,
}
";

    public const string DataElementTypeAttribute = @"
using System;

namespace WoWsShipBuilder.DataElements.DataElementAttributes;

[AttributeUsage(AttributeTargets.Property)]
public class DataElementTypeAttribute : Attribute
{
    public DataElementTypeAttribute(DataElementTypes type)
    {
        Type = type;
    }

    public DataElementTypes Type { get; }

    public string? UnitKey { get; set; }

    public string? TooltipKey { get; set; }

    public string? GroupKey { get; set; }

    public string[] LocalizationArguments { get; set; } = Array.Empty<string>();
}
";

    public const string DataElementVisibilityAttribute = @"using System;

namespace WoWsShipBuilder.DataElements.DataElementAttributes;

[AttributeUsage(AttributeTargets.Property)]
public class DataElementVisibilityAttribute : Attribute
{
    public DataElementVisibilityAttribute(bool enableFilterVisibility, string filterMethodName = "")
    {
        EnableFilterVisibility = enableFilterVisibility;
        FilterMethodName = filterMethodName;
    }

    public bool EnableFilterVisibility { get; }

    public string FilterMethodName { get; }
}";
}
