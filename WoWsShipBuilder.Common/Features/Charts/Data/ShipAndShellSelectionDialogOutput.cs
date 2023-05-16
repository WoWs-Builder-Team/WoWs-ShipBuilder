namespace WoWsShipBuilder.Common.Features.Charts.Data;

public sealed record ShipAndShellSelectionDialogOutput(List<ChartsDataWrapper> ShipsToAdd, Dictionary<Guid, ChartsDataWrapper> ShipsToModify, List<Guid> ShipsToRemove, bool OpenBuildDialog);
