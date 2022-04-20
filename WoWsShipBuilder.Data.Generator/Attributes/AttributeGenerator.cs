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
    public DataElementTypeAttribute(DataElementTypes type)
    {
        Type = type;
    }

    public DataElementTypes Type { get; }
}
";

    public const string DataElementTooltipAttribute = @"
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
";

    public const string DataElementGroupAttribute = @"
using System;

namespace WoWsShipBuilder.DataElements.DataElementAttributes;

[AttributeUsage(AttributeTargets.Property)]
public class DataElementGroupAttribute : Attribute
{
    public DataElementGroupAttribute(string groupKey)
    {
        GroupKey = groupKey;
    }

    public string GroupKey { get; }
}
";

    public const string DataElementUnitAttribute = @"
using System;

namespace WoWsShipBuilder.DataElements.DataElementAttributes;

[AttributeUsage(AttributeTargets.Property)]
public class DataElementUnitAttribute : Attribute
{
    public DataElementUnitAttribute(string unitKey)
    {
        UnitKey = unitKey;
    }

    public string UnitKey { get; }
}
";
}
