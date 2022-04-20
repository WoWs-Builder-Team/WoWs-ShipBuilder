namespace WoWsShipBuilder.Core.DataUI.DataElements;

/// <summary>
/// Record that represent a key value pair, with a measurement unit to be applied to the value.
/// </summary>
/// <param name="Key">The key of the element.</param>
/// <param name="Value">The value of the element.</param>
/// <param name="Unit">The unit to apply to the value.</param>
public readonly record struct KeyValueUnitDataElement(string Key, string Value, string Unit) : IDataElement;
