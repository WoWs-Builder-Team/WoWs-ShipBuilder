using System.Collections.Generic;

namespace WoWsShipBuilder.DataElements;

public abstract class DataContainerBase
{
    public List<IDataElement> DataElements { get; } = new();

    protected static bool ShouldAdd(object? value)
    {
        return value switch
        {
            string strValue => !string.IsNullOrEmpty(strValue),
            decimal decValue => decValue != 0,
            (decimal min, decimal max) => min > 0 || max > 0,
            int intValue => intValue != 0,
            _ => false,
        };
    }
}
