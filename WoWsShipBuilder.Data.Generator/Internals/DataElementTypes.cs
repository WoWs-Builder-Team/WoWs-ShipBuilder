using System;

namespace WoWsShipBuilder.Data.Generator.Internals;

[Flags]
internal enum DataElementTypes
{
    KeyValue = 1,
    KeyValueUnit = 2,
    Value = 4,
    Grouped = 8,
    Tooltip = 16,
    FormattedText = 32,
}
