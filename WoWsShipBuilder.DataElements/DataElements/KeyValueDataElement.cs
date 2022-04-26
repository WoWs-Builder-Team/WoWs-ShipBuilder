namespace WoWsShipBuilder.DataElements.DataElements;

/// <summary>
/// Record that represent a key value pair. Localization is left to the UI.
/// </summary>
/// <param name="Key">The key of the element.</param>
/// <param name="Value">The value of the element.</param>
/// <param name="IsValueKey">If the value is a localizer key.</param>
/// <param name="IsValueAppLocalization">If the value is an app localization key.</param>
public readonly record struct KeyValueDataElement(string Key, string Value, bool IsValueKey, bool IsValueAppLocalization) : IDataElement;
