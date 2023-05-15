using WoWsShipBuilder.Common.DataContainers;

namespace WoWsShipBuilder.Web.Utility;

public sealed record ChartCacheData(DispersionEllipse? DispPlotShipsCache, VerticalDispersions? VerticalDispersionsCache, Dictionary<double, Ballistic>? BallisticCache);
