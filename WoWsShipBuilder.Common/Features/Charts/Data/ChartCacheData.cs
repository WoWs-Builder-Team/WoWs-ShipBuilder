using WoWsShipBuilder.DataContainers;

namespace WoWsShipBuilder.Features.Charts.Data;

public sealed record ChartCacheData(DispersionEllipse? DispPlotShipsCache, VerticalDispersions? VerticalDispersionsCache, Dictionary<double, Ballistic>? BallisticCache);
