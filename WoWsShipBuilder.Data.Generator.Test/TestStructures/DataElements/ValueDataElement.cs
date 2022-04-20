﻿namespace WoWsShipBuilder.Core.DataUI.DataElements;

/// <summary>
/// A record that represent a single value.
/// </summary>
/// <param name="Value">The value of the element.</param>
public readonly record struct ValueDataElement(string Value) : IDataElement;
