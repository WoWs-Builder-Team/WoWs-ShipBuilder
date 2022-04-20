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
