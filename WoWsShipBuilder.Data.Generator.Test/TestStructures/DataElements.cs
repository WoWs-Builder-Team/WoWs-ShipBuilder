using System.Collections.Generic;

namespace WoWsShipBuilder.Core.DataUI.DataElements;

public interface IDataElement
{
}

public interface IDataUi
{
    List<IDataElement> DataElements { get; }

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

public readonly record struct KeyValueDataElement(string Key, string Value) : IDataElement;
public readonly record struct KeyValueUnitDataElement(string Key, string Value, string Unit) : IDataElement;
public readonly record struct TooltipDataElement(string Key, string Value, string Tooltip) : IDataElement;
public readonly record struct ValueDataElement(string Value) : IDataElement;
public sealed record GroupedDataElement(string Key, IEnumerable<IDataElement> Children) : IDataElement;
