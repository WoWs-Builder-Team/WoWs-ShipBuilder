using WoWsShipBuilder.Data.Generator.Utilities;

namespace WoWsShipBuilder.Data.Generator.DataElementGenerator.Model;

internal sealed record RawContainerData(string ContainerName, string Namespace, EquatableArray<SinglePropertyData> Properties);
