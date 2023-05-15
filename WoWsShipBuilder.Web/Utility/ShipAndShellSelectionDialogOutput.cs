namespace WoWsShipBuilder.Web.Utility;

public sealed record ShipAndShellSelectionDialogOutput(List<ChartsDataWrapper> ShipsToAdd, Dictionary<Guid, ChartsDataWrapper> ShipsToModify, List<Guid> ShipsToRemove, bool OpenBuildDialog);
