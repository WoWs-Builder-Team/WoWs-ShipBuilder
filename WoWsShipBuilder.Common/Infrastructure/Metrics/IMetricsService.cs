using Prometheus;

namespace WoWsShipBuilder.Infrastructure.Metrics;

public interface IMetricsService
{
    Counter ShipPageCount { get; }

    Counter ShipCount { get; }

    Counter BallisticPageCount { get; }

    Counter AccelerationPageCount { get; }

    Counter ComparisonPageCount { get; }

    Summary ComparisonLoadDuration { get; }

    Summary ShipViewModelInitDuration { get; }

    Counter RefCount { get; }
}
