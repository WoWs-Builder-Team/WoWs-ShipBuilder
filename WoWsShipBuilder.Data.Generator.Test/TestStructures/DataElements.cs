using System.Collections.Generic;

namespace WoWsShipBuilder.Core.DataUI.DataElements;

public interface IDataElement
{
}

public interface IDataUi
{
    List<IDataElement> DataElements { get; }
}

public readonly record struct KeyValueDataElement(string Key, string Value) : IDataElement;
public readonly record struct KeyValueUnitDataElement(string Key, string Value, string Unit) : IDataElement;
public readonly record struct TooltipDataElement(string Key, string Value, string Tooltip) : IDataElement;
public readonly record struct ValueDataElement(string Value) : IDataElement;
public sealed record GroupedDataElement(string Key, IEnumerable<IDataElement> Children) : IDataElement;
