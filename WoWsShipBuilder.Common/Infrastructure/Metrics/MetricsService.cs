using Prometheus;

namespace WoWsShipBuilder.Infrastructure.Metrics;

public class MetricsService
{
    public Counter PageAccessCount { get; } = Prometheus.Metrics.CreateCounter("page_access_total", "Number of page accesses", new CounterConfiguration
    {
        LabelNames = new[] { "page" },
    });

    public Counter ShipViewCount { get; } = Prometheus.Metrics.CreateCounter("ship_views_total", "Number of viewed ships.", new CounterConfiguration
    {
        LabelNames = new[] { "ship_index", "ship_name" },
    });

    public Histogram ComparisonLoadDuration { get; } = Prometheus.Metrics.CreateHistogram("comparison_load_duration_seconds", "Load duration for the ship comparison data grid in seconds");

    public Histogram ShipViewModelInitDuration { get; } = Prometheus.Metrics.CreateHistogram("ship_vm_init_duration_seconds", "Init-time of the ship VM in seconds");

    public Counter RefCount { get; } = Prometheus.Metrics.CreateCounter("ship_page_refs_total", "Number of refs for ship page requests", new CounterConfiguration
    {
        LabelNames = new[] { "referrer", "path" },
    });

    public Counter SavedBuildOperations { get; } = Prometheus.Metrics.CreateCounter("saved_build_operations_total", "Number of saved build operations", new CounterConfiguration
    {
        LabelNames = new[] { "operation" },
    });

    public Counter BuildImports { get; } = Prometheus.Metrics.CreateCounter("build_imports_total", "Number of build imports", new CounterConfiguration
    {
        LabelNames = new[] { "source", "type" },
    });

    public Counter SharedBuilds { get; } = Prometheus.Metrics.CreateCounter("shared_builds_total", "Number of shared builds", new CounterConfiguration
    {
        LabelNames = new[] { "type" },
    });

    public Counter ShipStatsActions { get; } = Prometheus.Metrics.CreateCounter("ship_stats_header_actions_total", "Number of usage of the actions of the shipstats page (inline links, buttons)", new CounterConfiguration
    {
        LabelNames = new[] { "action" },
    });
}
