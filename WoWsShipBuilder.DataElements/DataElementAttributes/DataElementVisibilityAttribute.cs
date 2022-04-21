using System;

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
}
