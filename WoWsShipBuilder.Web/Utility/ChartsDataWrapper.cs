using WoWsShipBuilder.Core.Data;

namespace WoWsShipBuilder.Web.Utility;

public sealed record ChartsDataWrapper(ShipBuildContainer ShipContainer, List<string> SelectedShellsIndexes);
