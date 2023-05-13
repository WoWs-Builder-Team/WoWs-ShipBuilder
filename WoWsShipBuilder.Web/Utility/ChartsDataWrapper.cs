using WoWsShipBuilder.Core.Data;

namespace WoWsShipBuilder.Web.Utility;

public sealed record ChartsDataWrapper(ShipBuildContainer ShipBuildContainer, Dictionary<string, ChartCacheData> SelectedShells);
