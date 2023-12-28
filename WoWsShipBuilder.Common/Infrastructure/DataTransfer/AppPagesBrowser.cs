using Microsoft.AspNetCore.Components;
using MudBlazor;
using WoWsShipBuilder.Infrastructure.Metrics;

namespace WoWsShipBuilder.Infrastructure.DataTransfer;

public class AppPagesBrowser
{
    private const int TransferLimit = 250;

    private const string ShipStatsBaseUrl = "ship?shipIndexes=";
    private const string BallisticChartsBaseUrl = "charts?shipIndex=";
    private const string AccelerationChartsBaseUrl = "acceleration-charts?shipIndexes=";
    private const string ShipComparisonBaseUrl = "comparison?p=";

    private readonly SessionStateCache sessionStateCache;
    private readonly NavigationManager navManager;
    private readonly MetricsService metricsService;

    public AppPagesBrowser(SessionStateCache sessionStateCache, NavigationManager navManager, MetricsService metricsService)
    {
        this.sessionStateCache = sessionStateCache;
        this.navManager = navManager;
        this.metricsService = metricsService;
    }

    public enum AppPage
    {
        ShipStats,
        BallisticCharts,
        AccelerationCharts,
        ShipComparison,
    }

    private void GoToPage(string destinationPageBaseUrl, List<ShipBuildContainer> containerList, string? shellIndex, ISnackbar? snackbar, AppPage? leavingPage, string metricLabel)
    {
        int selectionCount = containerList.Count;
        switch (selectionCount)
        {
            case > TransferLimit:
                snackbar?.Add($"Too many ships. Eligible ships: {selectionCount}. Max limit is {TransferLimit}.", Severity.Error);
                return;
            case 0:
                snackbar?.Add($"No ships available. Eligible ships: {selectionCount}.", Severity.Error);
                return;
            default:
                this.LogMetrics(leavingPage, destinationPageBaseUrl, metricLabel);
                this.sessionStateCache.SetBuildTransferContainers(containerList);
                this.navManager.NavigateTo(destinationPageBaseUrl + string.Join(',', containerList.Select(x => x.Ship.Index)) + (shellIndex is not null ? $"&shellIndex={shellIndex}" : string.Empty));
                break;
        }

        // cant use await JsRuntime.InvokeAsync<object>("open", NavManager.BaseUri + url, "_blank"); to open in a new tab because builds are not carried over.
    }

    private void LogMetrics(AppPage? leavingPage, string destinationPageBaseUrl, string metricLabel)
    {
        switch (leavingPage)
        {
            case AppPage.ShipStats when destinationPageBaseUrl.Equals(BallisticChartsBaseUrl, StringComparison.Ordinal):
                this.metricsService.ShipStatsActions.WithLabels(metricLabel).Inc();
                break;
            case AppPage.ShipStats when destinationPageBaseUrl.Equals(AccelerationChartsBaseUrl, StringComparison.Ordinal):
                this.metricsService.ShipStatsActions.WithLabels(metricLabel).Inc();
                break;
            case AppPage.ShipStats when destinationPageBaseUrl.Equals(ShipComparisonBaseUrl, StringComparison.Ordinal):
                this.metricsService.ShipStatsActions.WithLabels(metricLabel).Inc();
                break;
            case AppPage.BallisticCharts when destinationPageBaseUrl.Equals(ShipStatsBaseUrl, StringComparison.Ordinal):
                this.metricsService.BallisticChartsActions.WithLabels(metricLabel).Inc();
                break;
            case AppPage.BallisticCharts when destinationPageBaseUrl.Equals(AccelerationChartsBaseUrl, StringComparison.Ordinal):
                this.metricsService.BallisticChartsActions.WithLabels(metricLabel).Inc();
                break;
            case AppPage.BallisticCharts when destinationPageBaseUrl.Equals(ShipComparisonBaseUrl, StringComparison.Ordinal):
                this.metricsService.BallisticChartsActions.WithLabels(metricLabel).Inc();
                break;
            case AppPage.AccelerationCharts when destinationPageBaseUrl.Equals(BallisticChartsBaseUrl, StringComparison.Ordinal):
                this.metricsService.AccelerationChartsActions.WithLabels(metricLabel).Inc();
                break;
            case AppPage.AccelerationCharts when destinationPageBaseUrl.Equals(ShipStatsBaseUrl, StringComparison.Ordinal):
                this.metricsService.AccelerationChartsActions.WithLabels(metricLabel).Inc();
                break;
            case AppPage.AccelerationCharts when destinationPageBaseUrl.Equals(ShipComparisonBaseUrl, StringComparison.Ordinal):
                this.metricsService.AccelerationChartsActions.WithLabels(metricLabel).Inc();
                break;
            case AppPage.ShipComparison when destinationPageBaseUrl.Equals(BallisticChartsBaseUrl, StringComparison.Ordinal):
                this.metricsService.ShipComparisonActions.WithLabels(metricLabel).Inc();
                break;
            case AppPage.ShipComparison when destinationPageBaseUrl.Equals(AccelerationChartsBaseUrl, StringComparison.Ordinal):
                this.metricsService.ShipComparisonActions.WithLabels(metricLabel).Inc();
                break;
            case AppPage.ShipComparison when destinationPageBaseUrl.Equals(ShipStatsBaseUrl, StringComparison.Ordinal):
                this.metricsService.ShipComparisonActions.WithLabels(metricLabel).Inc();
                break;
            default:
                return;
        }
    }

    // to ship stats
    public void GoToShipStats(IEnumerable<ShipBuildContainer> containerList, ISnackbar? userNotification, AppPage? leavingPage, string metricLabel) => this.GoToPage(ShipStatsBaseUrl, containerList.ToList(), null, userNotification, leavingPage, metricLabel);

    public void GoToShipStats(IEnumerable<ShipBuildContainer> containerList, AppPage? leavingPage, string metricLabel) => this.GoToShipStats(containerList, null, leavingPage, metricLabel);

    public void GoToShipStats(IEnumerable<ShipBuildContainer> containerList, ISnackbar? userNotification) => this.GoToShipStats(containerList, userNotification, null, string.Empty);

    public void GoToShipStats(IEnumerable<ShipBuildContainer> containerList) => this.GoToShipStats(containerList, null);

    public void GoToShipStats(ShipBuildContainer container, ISnackbar? userNotification, AppPage? leavingPage, string metricLabel) => this.GoToPage(ShipStatsBaseUrl, new() { container }, null, userNotification, leavingPage, metricLabel);

    public void GoToShipStats(ShipBuildContainer container, AppPage? leavingPage, string metricLabel) => this.GoToShipStats(container, null, leavingPage, metricLabel);

    public void GoToShipStats(ShipBuildContainer container, ISnackbar? userNotification) => this.GoToShipStats(container, userNotification, null, string.Empty);

    public void GoToShipStats(ShipBuildContainer container) => this.GoToShipStats(container, null, string.Empty);

    // to ballistic charts
    public void GoToBallisticCharts(ShipBuildContainer container, string? shellIndex, ISnackbar? userNotification, AppPage? leavingPage, string metricLabel) => this.GoToPage(BallisticChartsBaseUrl, new() { container }, shellIndex, userNotification, leavingPage, metricLabel);

    public void GoToBallisticCharts(ShipBuildContainer container, ISnackbar? userNotification, AppPage? leavingPage, string metricLabel) => this.GoToBallisticCharts(container, null, userNotification, leavingPage, metricLabel);

    public void GoToBallisticCharts(ShipBuildContainer container, string? shellIndex, ISnackbar? userNotification) => this.GoToBallisticCharts(container, shellIndex, userNotification, null, string.Empty);

    public void GoToBallisticCharts(ShipBuildContainer container, ISnackbar? userNotification) => this.GoToBallisticCharts(container, null, userNotification);

    public void GoToBallisticCharts(ShipBuildContainer container, string? shellIndex, AppPage? leavingPage, string metricLabel) => this.GoToBallisticCharts(container, shellIndex, null, leavingPage, metricLabel);

    public void GoToBallisticCharts(ShipBuildContainer container, AppPage? leavingPage, string metricLabel) => this.GoToBallisticCharts(container, null, null, leavingPage, metricLabel);

    public void GoToBallisticCharts(ShipBuildContainer container, string? shellIndex) => this.GoToBallisticCharts(container, shellIndex, null);

    public void GoToBallisticCharts(ShipBuildContainer container) => this.GoToBallisticCharts(container, null, null, null, string.Empty);

    public void GoToBallisticCharts(IEnumerable<ShipBuildContainer> selectionList, ISnackbar? userNotification, AppPage? leavingPage, string metricLabel) => this.GoToPage(BallisticChartsBaseUrl, selectionList.ToList(), null, userNotification, leavingPage, metricLabel);

    public void GoToBallisticCharts(IEnumerable<ShipBuildContainer> selectionList, AppPage? leavingPage, string metricLabel) => this.GoToBallisticCharts(selectionList, null, leavingPage, metricLabel);

    public void GoToBallisticCharts(IEnumerable<ShipBuildContainer> selectionList, ISnackbar? userNotification) => this.GoToBallisticCharts(selectionList, userNotification, null, string.Empty);

    public void GoToBallisticCharts(IEnumerable<ShipBuildContainer> selectionList) => this.GoToBallisticCharts(selectionList, null);

    // to acceleration charts
    public void GoToAccelerationCharts(IEnumerable<ShipBuildContainer> containerList, ISnackbar? userNotification, AppPage? leavingPage, string metricLabel) => this.GoToPage(AccelerationChartsBaseUrl, containerList.ToList(), null, userNotification, leavingPage, metricLabel);

    public void GoToAccelerationCharts(IEnumerable<ShipBuildContainer> containerList, AppPage? leavingPage, string metricLabel) => this.GoToAccelerationCharts(containerList, null, leavingPage, metricLabel);

    public void GoToAccelerationCharts(IEnumerable<ShipBuildContainer> containerList, ISnackbar? userNotification) => this.GoToAccelerationCharts(containerList, userNotification, null, string.Empty);

    public void GoToAccelerationCharts(IEnumerable<ShipBuildContainer> containerList) => this.GoToAccelerationCharts(containerList, null);

    public void GoToAccelerationCharts(ShipBuildContainer container, ISnackbar? userNotification, AppPage? leavingPage, string metricLabel) => this.GoToPage(AccelerationChartsBaseUrl, new() { container }, null, userNotification, leavingPage, metricLabel);

    public void GoToAccelerationCharts(ShipBuildContainer container, AppPage? leavingPage, string metricLabel) => this.GoToAccelerationCharts(container, null, leavingPage, metricLabel);

    public void GoToAccelerationCharts(ShipBuildContainer container, ISnackbar? userNotification) => this.GoToAccelerationCharts(container, userNotification, null, string.Empty);

    public void GoToAccelerationCharts(ShipBuildContainer container) => this.GoToAccelerationCharts(container, null);

    // to ship comparison
    public void GoToShipComparison(IEnumerable<ShipBuildContainer> selectionList, ISnackbar? userNotification, AppPage? leavingPage, string metricLabel) => this.GoToPage(ShipComparisonBaseUrl, selectionList.ToList(), null, userNotification, leavingPage, metricLabel);

    public void GoToShipComparison(IEnumerable<ShipBuildContainer> selectionList, AppPage? leavingPage, string metricLabel) => this.GoToShipComparison(selectionList, null, leavingPage, metricLabel);

    public void GoToShipComparison(IEnumerable<ShipBuildContainer> selectionList, ISnackbar? userNotification) => this.GoToShipComparison(selectionList, userNotification, null, string.Empty);

    public void GoToShipComparison(IEnumerable<ShipBuildContainer> selectionList) => this.GoToShipComparison(selectionList, null);

    public void GoToShipComparison(ShipBuildContainer container, ISnackbar? userNotification, AppPage? leavingPage, string metricLabel) => this.GoToPage(ShipComparisonBaseUrl, new() { container }, null, userNotification, leavingPage, metricLabel);

    public void GoToShipComparison(ShipBuildContainer container, AppPage? leavingPage, string metricLabel) => this.GoToShipComparison(container, null, leavingPage, metricLabel);

    public void GoToShipComparison(ShipBuildContainer container, ISnackbar? userNotification) => this.GoToShipComparison(container, userNotification, null, string.Empty);

    public void GoToShipComparison(ShipBuildContainer container) => this.GoToShipComparison(container, null);
}
