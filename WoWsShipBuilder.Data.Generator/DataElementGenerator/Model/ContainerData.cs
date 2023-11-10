using WoWsShipBuilder.Data.Generator.Utilities;

namespace WoWsShipBuilder.Data.Generator.DataElementGenerator.Model;

internal sealed record ContainerData(string ContainerName, string Namespace, EquatableArray<PropertyData> Properties);
