using Microsoft.AspNetCore.Components;
using MudBlazor;

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

    public AppPagesBrowser(SessionStateCache sessionStateCache, NavigationManager navManager)
    {
        this.sessionStateCache = sessionStateCache;
        this.navManager = navManager;
    }

    private void GoToPage(string basePageUrl, List<ShipBuildContainer> selectionList, ISnackbar? snackbar = null, string? shellIndex = null)
    {
        int selectionCount = selectionList.Count;
        switch (selectionCount)
        {
            case > TransferLimit:
                snackbar?.Add($"Too many ships. Eligible ships: {selectionCount}. Max limit is {TransferLimit}.", Severity.Error);
                return;
            case 0:
                snackbar?.Add($"No ships available. Eligible ships: {selectionCount}.", Severity.Error);
                return;
            default:
                this.sessionStateCache.SetBuildTransferContainers(selectionList);
                this.navManager.NavigateTo(basePageUrl + string.Join(',', selectionList.Select(x => x.Ship.Index)) + (shellIndex is not null ? $"&shellIndex={shellIndex}" : string.Empty));
                break;
        }

        // cant use await JsRuntime.InvokeAsync<object>("open", NavManager.BaseUri + url, "_blank"); to open in a new tab because builds are not carried over.
    }

    public void GoToShipStats(ShipBuildContainer container, ISnackbar? snackbar = null) => this.GoToShipStats(new List<ShipBuildContainer> { container }, snackbar);

    public void GoToShipStats(IEnumerable<ShipBuildContainer> selectionList, ISnackbar? snackbar = null) => this.GoToPage(ShipStatsBaseUrl, selectionList.ToList(), snackbar);

    public void GoToBallisticCharts(ShipBuildContainer container, ISnackbar? snackbar = null, string? shellIndex = null) => this.GoToPage(BallisticChartsBaseUrl, new() { container }, snackbar, shellIndex);

    public void GoToBallisticCharts(IEnumerable<ShipBuildContainer> selectionList, ISnackbar? snackbar = null) => this.GoToPage(BallisticChartsBaseUrl, selectionList.ToList(), snackbar);

    public void GoToAccelerationCharts(ShipBuildContainer container, ISnackbar? snackbar = null) => this.GoToAccelerationCharts(new List<ShipBuildContainer> { container }, snackbar);

    public void GoToAccelerationCharts(IEnumerable<ShipBuildContainer> selectionList, ISnackbar? snackbar = null) => this.GoToPage(AccelerationChartsBaseUrl, selectionList.ToList(), snackbar);

    public void GoToShipComparison(ShipBuildContainer container, ISnackbar? snackbar = null) => this.GoToShipComparison(new List<ShipBuildContainer> { container }, snackbar);

    public void GoToShipComparison(IEnumerable<ShipBuildContainer> selectionList, ISnackbar? snackbar = null) => this.GoToPage(ShipComparisonBaseUrl, selectionList.ToList(), snackbar);
}
