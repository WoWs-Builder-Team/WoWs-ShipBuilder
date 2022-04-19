using System.Collections;
using System.Collections.Generic;

namespace WoWsShipBuilder.Core.DataUI.DataElements;
/// <summary>
/// A record that represent a group of <see cref="IDataElement"/> that share a common group.
/// </summary>
/// <param name="Key">The key of the element.</param>
/// <param name="Children">The enumerable of <see cref="IDataElement"/> that this key contain.</param>
public sealed record NestedDataElement(string Key, IEnumerable<IDataElement> Children) : IDataElement;
