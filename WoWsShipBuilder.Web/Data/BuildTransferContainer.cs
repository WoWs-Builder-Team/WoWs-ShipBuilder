namespace WoWsShipBuilder.Web.Data;

public sealed record BuildTransferContainer(string ShipIndex, string BuildString, List<(string, float)>? Modifiers, IEnumerable<int>? ActivatedConsumables);
