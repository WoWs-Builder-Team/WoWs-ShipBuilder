namespace WoWsShipBuilder.Data.Generator.Attributes;

public static class AttributeGenerator
{

    public const string DataElementTypesEnum = @"
namespace WoWsShipBuilder.DataElements.DataElementAttributes;

public enum DataElementTypes
{
    KeyValue,
    KeyValueUnit,
    Value,
    Grouped,
    Tooltip,
}
";

    public const string DataElementTypeAttribute = @"
using System;

namespace WoWsShipBuilder.DataElements.DataElementAttributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class DataElementTypeAttribute : Attribute
{
     public DataElementTypeAttribute(DataElementTypes type, string argument = """")
    {
        Type = type;
        Argument = argument;
    }

    public DataElementTypes Type { get; }

    public string Argument { get; }
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
