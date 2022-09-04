using Prometheus;

namespace WoWsShipBuilder.Web.Services;

public class MetricsService : IMetricsService
{
    public Counter ShipPageCount { get; } = Metrics.CreateCounter("ship_page_requests_total", "Number of ship page requests");

    public Counter ShipCount { get; } = Metrics.CreateCounter("ship_requests_total", "Number of viewed ships.", new CounterConfiguration
    {
        LabelNames = new[] { "ship_index" },
    });

    public Counter BallisticPageCount { get; } = Metrics.CreateCounter("ballistics_page_requests_total", "Number of ballistics page requests");

    public Summary ShipViewModelInitDuration { get; } = Metrics.CreateSummary("ship_vm_init_time_average10s", "Average init-time of the ship VM in seconds over the last 10 seconds.", new SummaryConfiguration
    {
        MaxAge = TimeSpan.FromSeconds(10),
    });

    public Counter RefCount { get; } = Metrics.CreateCounter("ship_page_refs_total", "Number of refs for ship page requests", new CounterConfiguration
    {
        LabelNames = new[] { "referrer", "path" },
    });
}
