namespace WoWsShipBuilder.Web.Utility;

public record ShipAndShellSelectionDialogOutput(List<ChartsDataWrapper> ShipsToAdd, Dictionary<Guid, ChartsDataWrapper> ShipsToModify, List<Guid> ShipsToRemove);
