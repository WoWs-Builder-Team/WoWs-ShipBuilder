using WoWsShipBuilder.Core.DataContainers;

namespace WoWsShipBuilder.Web.Utility;

public sealed record ChartsCacheValue(DispersionEllipse? DispPlotShipsCache, VerticalDispersions? VerticalDispersionsCache, Dictionary<double, Ballistic>? BallisticCache);
