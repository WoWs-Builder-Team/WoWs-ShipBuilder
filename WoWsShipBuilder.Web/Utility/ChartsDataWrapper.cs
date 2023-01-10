using WoWsShipBuilder.Core.Data;

namespace WoWsShipBuilder.Web.Utility;

public record ChartsDataWrapper(ShipBuildContainer ShipContainer, List<string> SelectedShells);
