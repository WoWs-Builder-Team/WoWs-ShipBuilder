using Prometheus;

namespace WoWsShipBuilder.Infrastructure.Metrics;

public class MetricsService
{
    private static readonly double[] DefaultDurationBuckets = { .0025, .005, .01, .025, .05, .075, .1, .25, .5, .75, 1, 2.5, 5 };

    public Counter PageAccessCount { get; } = Prometheus.Metrics.CreateCounter("page_access_total", "Number of page accesses", new CounterConfiguration
    {
        LabelNames = new[] { "page" },
    });

    public Counter ShipViewCount { get; } = Prometheus.Metrics.CreateCounter("ship_views_total", "Number of viewed ships.", new CounterConfiguration
    {
        LabelNames = new[] { "ship_index", "ship_name" },
    });

    public Histogram ComparisonLoadDuration { get; } = Prometheus.Metrics.CreateHistogram("comparison_load_duration_seconds", "Load duration for the ship comparison data grid in seconds", new HistogramConfiguration
    {
        Buckets = DefaultDurationBuckets,
    });

    public Histogram ComparisonShipCount { get; } = Prometheus.Metrics.CreateHistogram("comparison_ship_count", "Number of ships shown in the comparison", new HistogramConfiguration
    {
        Buckets = Histogram.LinearBuckets(50, 50, 15),
    });

    public Histogram ShipViewModelInitDuration { get; } = Prometheus.Metrics.CreateHistogram("ship_vm_init_duration_seconds", "Init-time of the ship VM in seconds", new HistogramConfiguration
    {
        Buckets = DefaultDurationBuckets,
    });

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

    public Counter ShipComparisonActions { get; } = Prometheus.Metrics.CreateCounter("ship_comparison_actions_total", "Number of usage of the actions of the ship comparison page (inline links, buttons)", new CounterConfiguration
    {
        LabelNames = new[] { "action" },
    });

    public Counter BuildConfigurationDialogUsage { get; } = Prometheus.Metrics.CreateCounter("build_configuration_dialog_usage_total", "Number of usage of the build configuration dialog", new CounterConfiguration
    {
        LabelNames = new[] { "feature" },
    });

    public Counter CustomAccelerationDataUsage { get; } = Prometheus.Metrics.CreateCounter("custom_acceleration_data_usage_total", "Number of usage of the custom acceleration data feature");

    public Counter ChartsTabUsage { get; } = Prometheus.Metrics.CreateCounter("charts_tab_usage_total", "Number of usage of each charts tab", new CounterConfiguration
    {
        LabelNames = new[] { "tab" },
    });

    public Counter BetaCodeActivation { get; } = Prometheus.Metrics.CreateCounter("beta_code_activation_total", "Number of times a user activates a beta code", new CounterConfiguration
    {
        LabelNames = new[] { "code" },
    });

    public Counter CaptainSkillPopupUsage { get; } = Prometheus.Metrics.CreateCounter("captain_skill_popup_usage_total", "Number of times a user opens the captain skill popup");

    public Counter ConsumableActivations { get; } = Prometheus.Metrics.CreateCounter("consumable_activations_total", "Number of times a user activates a consumable");
}
