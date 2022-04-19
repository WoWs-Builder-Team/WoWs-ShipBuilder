namespace WoWsShipBuilder.Core.DataUI.DataElements;

/// <summary>
/// Record that represent a key value pair. Localization is left to the UI.
/// </summary>
/// <param name="Key">The key of the element.</param>
/// <param name="Value">The value of the element.</param>
public sealed record KeyValueDataElement(string Key, string Value) : IDataElement;
