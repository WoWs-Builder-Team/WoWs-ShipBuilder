using System;

namespace WoWsShipBuilder.Core.DataUI.DataElementAttributes;

[AttributeUsage(AttributeTargets.Property)]
public class DataElementType : Attribute
{
    public DataElementType(DataElementEnum type)
    {
        Type = type;
    }

    public DataElementEnum Type { get; }
}
