using WoWsShipBuilder.Common.Infrastructure.DataTransfer;

namespace WoWsShipBuilder.Common.Features.AccelerationCharts;

public record ShipBuildContainerSelectionDialogOutput(List<ShipBuildContainer> ShipList, bool ShouldOpenBuildDialog);
