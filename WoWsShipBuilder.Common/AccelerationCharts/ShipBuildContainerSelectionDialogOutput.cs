using WoWsShipBuilder.Common.Infrastructure.DataTransfer;

namespace WoWsShipBuilder.Common.AccelerationCharts;

public record ShipBuildContainerSelectionDialogOutput(List<ShipBuildContainer> ShipList, bool ShouldOpenBuildDialog);
