using System.Collections.Generic;

namespace WoWsShipBuilder.DataElements;

/// <summary>
/// A record that represent a formatted text data element.
/// </summary>
/// <param name="Text">The text to format. Need to respect <see cref="string.Format(string,object[])"/> specifications.</param>
/// <param name="Arguments">The enumerable that will be used by <see cref="string.Format(string,object[])"/>.</param>
/// <param name="ValueTextKind">The <see cref="DataElementTextKind"/> of the text.</param>
/// <param name="ArgumentsTextKind">The <see cref="DataElementTextKind"/> of the arguments.</param>
public sealed record FormattedTextDataElement(string Text, IEnumerable<string> Arguments, DataElementTextKind ValueTextKind = DataElementTextKind.Plain, DataElementTextKind ArgumentsTextKind = DataElementTextKind.Plain) : IDataElement;
