using WoWsShipBuilder.Features.Navigation;

namespace WoWsShipBuilder.Features.AccelerationCharts;

public record ShipBuildContainerSelectionDialogOutput(List<ShipBuildContainer> ShipList, bool ShouldOpenBuildDialog);
