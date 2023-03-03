using WoWsShipBuilder.Core.Data;

namespace WoWsShipBuilder.Web.Utility;

public record ShipBuildContainerSelectionDialogOutput(List<ShipBuildContainer> ShipList, bool ShouldOpenBuildDialog);
