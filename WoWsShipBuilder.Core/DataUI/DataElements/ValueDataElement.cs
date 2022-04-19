namespace WoWsShipBuilder.Core.DataUI.DataElements;

/// <summary>
/// A record that represent a single value.
/// </summary>
/// <param name="Value">The value of the element.</param>
public sealed record ValueDataElement(string Value) : IDataElement;
