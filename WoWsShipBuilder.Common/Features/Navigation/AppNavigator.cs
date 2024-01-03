using System.Collections.Immutable;
using System.ComponentModel;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using WoWsShipBuilder.Infrastructure.Metrics;

namespace WoWsShipBuilder.Features.Navigation;

public class AppNavigator
{
    private const int TransferLimit = 250;

    private const string ShipStatsBaseUrl = "ship?shipIndexes=";
    private const string BallisticChartsBaseUrl = "charts?shipIndex=";
    private const string AccelerationChartsBaseUrl = "acceleration-charts?shipIndexes=";
    private const string ShipComparisonBaseUrl = "comparison?p=";

    private const string ShipStatsMetricLabel = "open-ship-stats";
    private const string BallisticChartsMetricLabel = "open-ballistic-charts";
    private const string AccelerationChartsMetricLabel = "open-acceleration-charts";
    private const string ShipComparisonMetricLabel = "open-ship-comparison";

    private readonly SessionStateCache sessionStateCache;
    private readonly NavigationManager navManager;
    private readonly MetricsService metricsService;
    private readonly ISnackbar snackbar;

    public AppNavigator(SessionStateCache sessionStateCache, NavigationManager navManager, MetricsService metricsService, ISnackbar snackbar)
    {
        this.sessionStateCache = sessionStateCache;
        this.navManager = navManager;
        this.metricsService = metricsService;
        this.snackbar = snackbar;
    }

    public enum AppPage
    {
        ShipStats,
        BallisticCharts,
        AccelerationCharts,
        ShipComparison,
    }

    /// <summary>
    /// Navigates to the specified destination page.
    /// </summary>
    /// <param name="destinationPage">The page to navigate to.</param>
    /// <param name="containerList">The list of ship build containers to carry over.</param>
    /// <param name="leavingPage">Optional. The page the user is leaving. Only needed for metrics.</param>
    public void NavigateTo(AppPage destinationPage, IEnumerable<ShipBuildContainer> containerList, AppPage? leavingPage = null) => this.GoToPage(destinationPage, containerList.ToImmutableList(), null, leavingPage);

    /// <summary>
    /// Navigates to the specified destination page.
    /// </summary>
    /// <param name="destinationPage">The destination page to navigate to.</param>
    /// <param name="container">The ship build container to carry over.</param>
    /// <param name="leavingPage">Optional. The page the user is leaving. Only needed for metrics.</param>
    public void NavigateTo(AppPage destinationPage, ShipBuildContainer container, AppPage? leavingPage = null) => this.GoToPage(destinationPage, ImmutableList.Create(container), null, leavingPage);

    /// <summary>
    /// Navigates to the specified destination page. Carrying over also the selected shell index.
    /// </summary>
    /// <param name="destinationPage">The destination page to navigate to.</param>
    /// <param name="container">The ship build container to carry over.</param>
    /// <param name="shellIndex">The shell index. Only needed when navigating to the ballistic charts.</param>
    /// <param name="leavingPage">Optional. The page the user is leaving. Only needed for metrics.</param>
    public void NavigateTo(AppPage destinationPage, ShipBuildContainer container, string shellIndex, AppPage? leavingPage = null) => this.GoToPage(destinationPage, ImmutableList.Create(container), shellIndex, leavingPage);

    private static string GenerateDestinationUrl(AppPage destinationPage, IEnumerable<ShipBuildContainer> containerList, string? shellIndex)
    {
        var shipIndexes = string.Join(',', containerList.Select(x => x.Ship.Index));
        return destinationPage switch
        {
            AppPage.ShipStats => ShipStatsBaseUrl + shipIndexes,
            AppPage.BallisticCharts => BallisticChartsBaseUrl + shipIndexes + (shellIndex is not null ? $"&shellIndex={shellIndex}" : string.Empty),
            AppPage.AccelerationCharts => AccelerationChartsBaseUrl + shipIndexes,
            AppPage.ShipComparison => ShipComparisonBaseUrl + shipIndexes,
            _ => throw new InvalidEnumArgumentException(),
        };
    }

    private void GoToPage(AppPage destinationPage, ImmutableList<ShipBuildContainer> containerList, string? shellIndex, AppPage? leavingPage)
    {
        int selectionCount = containerList.Count;
        switch (selectionCount)
        {
            case > TransferLimit:
                this.snackbar.Add($"Too many ships. Eligible ships: {selectionCount}. Max limit is {TransferLimit}.", Severity.Error);
                return;
            case 0:
                this.snackbar.Add($"No ships available. Eligible ships: {selectionCount}.", Severity.Error);
                return;
            default:
                this.LogMetrics(destinationPage, leavingPage);
                this.sessionStateCache.SetBuildTransferContainers(containerList);
                this.navManager.NavigateTo(GenerateDestinationUrl(destinationPage, containerList, shellIndex));
                break;
        }

        // cant use await JsRuntime.InvokeAsync<object>("open", NavManager.BaseUri + url, "_blank"); to open in a new tab because builds are not carried over.
    }

    private void LogMetrics(AppPage destinationPage, AppPage? leavingPage)
    {
        string metricLabel = destinationPage switch
        {
            AppPage.ShipStats => ShipStatsMetricLabel,
            AppPage.BallisticCharts => BallisticChartsMetricLabel,
            AppPage.AccelerationCharts => AccelerationChartsMetricLabel,
            AppPage.ShipComparison => ShipComparisonMetricLabel,
            _ => throw new InvalidEnumArgumentException(),
        };

        switch (leavingPage)
        {
            case AppPage.ShipStats:
                this.metricsService.ShipStatsActions.WithLabels(metricLabel).Inc();
                break;
            case AppPage.BallisticCharts:
                this.metricsService.BallisticChartsActions.WithLabels(metricLabel).Inc();
                break;
            case AppPage.AccelerationCharts:
                this.metricsService.AccelerationChartsActions.WithLabels(metricLabel).Inc();
                break;
            case AppPage.ShipComparison:
                this.metricsService.ShipComparisonActions.WithLabels(metricLabel).Inc();
                break;
            default:
                return;
        }
    }
}
