namespace WoWsShipBuilder.Core.DataUI.DataElements;

/// <summary>
/// A record that represent a key value pair with a tooltip text.
/// </summary>
/// <param name="Key">The key of the element.</param>
/// <param name="Value">The value of the element.</param>
/// <param name="Tooltip">The tooltip text to show when hovering over the element.</param>
public readonly record struct TooltipDataElement(string Key, string Value, string Tooltip) : IDataElement;
