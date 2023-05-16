using WoWsShipBuilder.Common.DataContainers;

namespace WoWsShipBuilder.Common.Charts.Data;

public sealed record ChartCacheData(DispersionEllipse? DispPlotShipsCache, VerticalDispersions? VerticalDispersionsCache, Dictionary<double, Ballistic>? BallisticCache);
