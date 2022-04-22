using System;

namespace WoWsShipBuilder.DataElements.DataElementAttributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class DataElementTypeAttribute : Attribute
{
    public DataElementTypeAttribute(DataElementTypes type, string argument = "")
    {
        Type = type;
        Argument = argument;
    }

    public DataElementTypes Type { get; }

    public string Argument { get; }
}
