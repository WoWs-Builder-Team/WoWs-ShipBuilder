using WoWsShipBuilder.Features.DataTransfer;

namespace WoWsShipBuilder.Features.AccelerationCharts;

public record ShipBuildContainerSelectionDialogOutput(List<ShipBuildContainer> ShipList, bool ShouldOpenBuildDialog);
