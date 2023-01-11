using WoWsShipBuilder.Core.Data;

namespace WoWsShipBuilder.Web.Utility;

public sealed record ChartsDataWrapper(ShipBuildContainer ShipContainer, Dictionary<string, ChartCacheData> SelectedShells);
