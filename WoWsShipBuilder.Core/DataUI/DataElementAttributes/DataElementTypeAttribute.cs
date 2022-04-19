using System;

namespace WoWsShipBuilder.Core.DataUI.DataElementAttributes;

[AttributeUsage(AttributeTargets.Property)]
public class DataElementTypeAttribute : Attribute
{
    public DataElementTypeAttribute(DataElementTypes type)
    {
        Type = type;
    }

    public DataElementTypes Type { get; }
}
