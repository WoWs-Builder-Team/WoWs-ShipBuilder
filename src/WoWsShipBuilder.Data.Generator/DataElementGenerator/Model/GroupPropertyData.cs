using WoWsShipBuilder.Data.Generator.Utilities;

namespace WoWsShipBuilder.Data.Generator.DataElementGenerator.Model;

internal sealed record GroupPropertyData(string GroupName, EquatableArray<SinglePropertyData> Properties, int DeclarationIndex);

