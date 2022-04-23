using System.Collections.Generic;

namespace WoWsShipBuilder.Core.DataUI.DataElements;

public interface IDataElement
{
}

public abstract record DataContainerBase
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

public readonly record struct KeyValueDataElement(string Key, string Value, bool IsValueKey, bool IsValueAppLocalization) : IDataElement;
public readonly record struct KeyValueUnitDataElement(string Key, string Value, string Unit) : IDataElement;
public readonly record struct TooltipDataElement(string Key, string Value, string Tooltip) : IDataElement;
public readonly record struct ValueDataElement(string Value, bool IsValueKey, bool IsValueAppLocalization) : IDataElement;
public sealed record FormattedTextDataElement(string Text, IEnumerable<string> Values, bool IsTextKey, bool IsTextAppLocalization, bool AreValuesKeys, bool AreValuesAppLocalization) : IDataElement;

public sealed record GroupedDataElement(string Key, IEnumerable<IDataElement> Children) : IDataElement;
public record ProjectileDataContainer : DataContainerBase;
