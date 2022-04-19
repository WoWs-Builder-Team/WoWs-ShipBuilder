using System;

namespace WoWsShipBuilder.Core.DataUI.DataElementAttributes;

[AttributeUsage(AttributeTargets.Property)]
public class DataElementGroupAttribute : Attribute
{
    public DataElementGroupAttribute(string groupKey)
    {
        GroupKey = groupKey;
    }

    public string GroupKey { get; }
}
