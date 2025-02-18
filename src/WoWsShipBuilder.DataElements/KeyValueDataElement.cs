namespace WoWsShipBuilder.DataElements;

/// <summary>
/// Record that represent a key value pair. Localization is left to the UI.
/// </summary>
/// <param name="Key">The key of the element.</param>
/// <param name="Value">The value of the element.</param>
/// <param name="ValueTextKind">The <see cref="DataElementTextKind"/> of the <see cref="Value"/>.</param>
public readonly record struct KeyValueDataElement(string Key, string Value, DataElementTextKind ValueTextKind) : IDataElement;
