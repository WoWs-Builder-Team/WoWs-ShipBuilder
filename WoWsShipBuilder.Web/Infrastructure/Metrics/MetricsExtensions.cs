﻿using WoWsShipBuilder.Infrastructure;
using WoWsShipBuilder.Infrastructure.Metrics;

namespace WoWsShipBuilder.Web.Infrastructure.Metrics;

public static class MetricsExtensions
{
    public static void IncreaseShipCount(this IMetricsService metricsService, string shipIndex, string localizedIndex)
    {
        metricsService.ShipCount.WithLabels(shipIndex, localizedIndex).Inc();
    }
}