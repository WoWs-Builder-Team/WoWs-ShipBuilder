using System.Diagnostics.Metrics;

namespace WoWsShipBuilder.Infrastructure.Metrics;

public class MetricsService
{
    private static readonly double[] DefaultDurationBuckets = [.00125, .0025, .005, .01, .025, .05, .075, .1, .25, .5, .75, 1, 2.5, 5];

    private readonly Meter meter;

    private readonly Counter<int> pageAccessCount;

    private readonly Counter<int> shipViewCount;

    private readonly Counter<int> refCount;

    private readonly Counter<int> savedBuildOperations;

    private readonly Counter<int> buildImports;

    private readonly Counter<int> sharedBuilds;

    private readonly Counter<int> shipStatsActions;

    private readonly Counter<int> shipComparisonActions;

    private readonly Counter<int> ballisticChartsActions;

    private readonly Counter<int> accelerationChartsActions;

    private readonly Counter<int> buildConfigurationDialogUsage;

    private readonly Counter<int> customAccelerationDataUsage;

    private readonly Counter<int> ballisticsChartsTabUsage;

    private readonly Counter<int> betaCodeActivation;

    private readonly Counter<int> captainSkillPopupUsage;

    private readonly Counter<int> consumableActivations;

    public MetricsService(IMeterFactory meterFactory)
    {
        this.meter = meterFactory.Create("Wowssb.Web");
        this.pageAccessCount = this.meter.CreateCounter<int>("page.access.total", description: "Number of page accesses");
        this.shipViewCount = this.meter.CreateCounter<int>("ship.views.total", description: "Number of ship views");
        var histogramAdvice = new InstrumentAdvice<double> { HistogramBucketBoundaries = DefaultDurationBuckets };
        this.ComparisonLoadDuration = this.meter.CreateHistogram("comparison.load.duration.seconds", description: "Load duration for the ship comparison data grid in seconds", advice: histogramAdvice);
        this.ShipViewModelInitDuration = this.meter.CreateHistogram("ship.vm.init.duration.seconds", description: "Init-time of the ship VM in seconds", advice: histogramAdvice);
        this.ComparisonShipCount = this.meter.CreateHistogram<int>("comparison.ship.count", description: "Number of ships shown in the comparison", advice: new() { HistogramBucketBoundaries = Enumerable.Range(1, 15).Select(x => x * 50).ToList() });
        this.refCount = this.meter.CreateCounter<int>("ship.page.refs.total", description: "Number of refs for ship page requests");
        this.savedBuildOperations = this.meter.CreateCounter<int>("saved.build.operations.total", description: "Number of saved build operations");
        this.buildImports = this.meter.CreateCounter<int>("build.imports.total", description: "Number of build imports");
        this.sharedBuilds = this.meter.CreateCounter<int>("shared.builds.total", description: "Number of shared builds");
        this.shipStatsActions = this.meter.CreateCounter<int>("ship.stats.header.actions.total", description: "Number of usage of the actions of the shipstats page (inline links, buttons)");
        this.shipComparisonActions = this.meter.CreateCounter<int>("ship.comparison.actions.total", description: "Number of usage of the actions of the ship comparison page (inline links, buttons)");
        this.ballisticChartsActions = this.meter.CreateCounter<int>("charts.actions.total", description: "Number of usage of the actions of the ballistic charts page (inline links, buttons)");
        this.accelerationChartsActions = this.meter.CreateCounter<int>("acceleration.charts.actions.total", description: "Number of usage of the actions of the acceleration charts page (inline links, buttons)");
        this.buildConfigurationDialogUsage = this.meter.CreateCounter<int>("build.configuration.dialog.usage.total", description: "Number of usage of the build configuration dialog");
        this.customAccelerationDataUsage = this.meter.CreateCounter<int>("custom.acceleration.data.usage.total", description: "Number of usage of the custom acceleration data feature");
        this.ballisticsChartsTabUsage = this.meter.CreateCounter<int>("charts.tab.usage.total", description: "Number of usage of each charts tab");
        this.betaCodeActivation = this.meter.CreateCounter<int>("beta.code.activation.total", description: "Number of times a user activates a beta code");
        this.captainSkillPopupUsage = this.meter.CreateCounter<int>("captain.skill.popup.usage.total", description: "Number of times a user opens the captain skill popup");
        this.consumableActivations = this.meter.CreateCounter<int>("consumable.activations.total", description: "Number of times a user activates a consumable");
    }

    public Histogram<double> ComparisonLoadDuration { get; }

    public Histogram<int> ComparisonShipCount { get; }

    public Histogram<double> ShipViewModelInitDuration { get; }

    public void AddPageAccess(string page)
    {
        this.pageAccessCount.Add(1, new KeyValuePair<string, object?>("page", page));
    }

    public void AddShipView(string shipIndex, string shipName)
    {
        this.shipViewCount.Add(1, new("ship_index", shipIndex), new("ship_name", shipName));
    }

    public void AddReferrer(string referrer, string path)
    {
        this.refCount.Add(1, new("referrer", referrer), new("path", path));
    }

    public void AddSavedBuildOperation(string operation)
    {
        this.savedBuildOperations.Add(1, new KeyValuePair<string, object?>("operation", operation));
    }

    public void AddBuildImport(string source, string type)
    {
        this.buildImports.Add(1, new("source", source), new("type", type));
    }

    public void AddSharedBuild(string type)
    {
        this.sharedBuilds.Add(1, new KeyValuePair<string, object?>("type", type));
    }

    public void AddShipStatsAction(string action)
    {
        this.shipStatsActions.Add(1, new KeyValuePair<string, object?>("action", action));
    }

    public void AddShipComparisonAction(string action)
    {
        this.shipComparisonActions.Add(1, new KeyValuePair<string, object?>("action", action));
    }

    public void AddBallisticChartsAction(string action)
    {
        this.ballisticChartsActions.Add(1, new KeyValuePair<string, object?>("action", action));
    }

    public void AddAccelerationChartsAction(string action)
    {
        this.accelerationChartsActions.Add(1, new KeyValuePair<string, object?>("action", action));
    }

    public void AddBuildConfigurationDialogUsage(string feature)
    {
        this.buildConfigurationDialogUsage.Add(1, new KeyValuePair<string, object?>("feature", feature));
    }

    public void AddCustomAccelerationDataUsage()
    {
        this.customAccelerationDataUsage.Add(1);
    }

    public void AddBallisticsChartsTabUsage(string tab)
    {
        this.ballisticsChartsTabUsage.Add(1, new KeyValuePair<string, object?>("tab", tab));
    }

    public void AddBetaCodeActivation(string code)
    {
        this.betaCodeActivation.Add(1, new KeyValuePair<string, object?>("code", code));
    }

    public void AddCaptainSkillPopupUsage()
    {
        this.captainSkillPopupUsage.Add(1);
    }

    public void AddConsumableActivation()
    {
        this.consumableActivations.Add(1);
    }
}
