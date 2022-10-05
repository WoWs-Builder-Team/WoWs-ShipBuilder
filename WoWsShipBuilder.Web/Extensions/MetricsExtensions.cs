using WoWsShipBuilder.Web.Services;

namespace WoWsShipBuilder.Web.Extensions;

public static class MetricsExtensions
{
    public static void IncreaseShipCount(this IMetricsService metricsService, string shipIndex, string localizedIndex)
    {
        metricsService.ShipCount.WithLabels(shipIndex, localizedIndex).Inc();
    }
}
