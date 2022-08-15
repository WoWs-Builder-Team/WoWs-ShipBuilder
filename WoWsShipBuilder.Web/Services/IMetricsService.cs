using Prometheus;

namespace WoWsShipBuilder.Web.Services;

public interface IMetricsService
{
    Counter ShipPageCount { get; }

    Counter ShipCount { get; }

    Counter BallisticPageCount { get; }

    Summary ShipViewModelInitDuration { get; }
}
