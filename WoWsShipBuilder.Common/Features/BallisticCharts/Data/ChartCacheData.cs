namespace WoWsShipBuilder.Features.BallisticCharts.Data;

public sealed record ChartCacheData(DispersionEllipse? DispPlotShipsCache, VerticalDispersions? VerticalDispersionsCache, Dictionary<double, Ballistic>? BallisticCache);
