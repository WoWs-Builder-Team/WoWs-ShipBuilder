using WoWsShipBuilder.Common.Infrastructure;

namespace WoWsShipBuilder.Web.Utility;

public record ShipBuildContainerSelectionDialogOutput(List<ShipBuildContainer> ShipList, bool ShouldOpenBuildDialog);
