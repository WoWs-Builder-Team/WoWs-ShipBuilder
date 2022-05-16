namespace WoWsShipBuilder.DataElements.DataElements;

/// <summary>
/// A record that represent a single value.
/// </summary>
/// <param name="Value">The value of the element.</param>
/// <param name="IsValueKey">If the value is a localizer key.</param>
/// <param name="IsValueAppLocalization">If the value is an app localization key.</param>
public readonly record struct ValueDataElement(string Value, bool IsValueKey, bool IsValueAppLocalization) : IDataElement;
