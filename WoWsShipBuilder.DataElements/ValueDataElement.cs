namespace WoWsShipBuilder.DataElements;

/// <summary>
/// A record that represent a single value.
/// </summary>
/// <param name="Value">The value of the element.</param>
/// <param name="ValueTextKind">The <see cref="DataElementTextKind"/> of the value.</param>
public readonly record struct ValueDataElement(string Value, DataElementTextKind ValueTextKind) : IDataElement;
