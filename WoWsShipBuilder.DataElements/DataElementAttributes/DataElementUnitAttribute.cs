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
