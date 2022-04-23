using System.Collections.Generic;

namespace WoWsShipBuilder.Core.DataUI.DataElements;

public sealed record FormattedTextDataElement(string Text, IEnumerable<string> Values, bool IsTextKey, bool AreValuesKeys) : IDataElement;
