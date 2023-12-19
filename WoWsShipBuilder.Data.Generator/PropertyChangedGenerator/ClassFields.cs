using WoWsShipBuilder.Data.Generator.Utilities;

namespace WoWsShipBuilder.Data.Generator.PropertyChangedGenerator;

internal sealed record ClassFields(string ClassName, EquatableArray<FieldData> Fields);
