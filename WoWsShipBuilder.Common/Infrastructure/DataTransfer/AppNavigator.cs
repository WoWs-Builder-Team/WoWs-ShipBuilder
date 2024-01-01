﻿using System.ComponentModel;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using WoWsShipBuilder.Infrastructure.Metrics;

namespace WoWsShipBuilder.Infrastructure.DataTransfer;

public class AppNavigator
{
    private const int TransferLimit = 250;

    private const string ShipStatsBaseUrl = "ship?shipIndexes=";
    private const string BallisticChartsBaseUrl = "charts?shipIndex=";
    private const string AccelerationChartsBaseUrl = "acceleration-charts?shipIndexes=";
    private const string ShipComparisonBaseUrl = "comparison?p=";

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

    private void GoToPage(AppPage destinationPage, List<ShipBuildContainer> containerList, string? shellIndex, AppPage? leavingPage, string? metricLabel)
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
                this.LogMetrics(leavingPage, metricLabel);
                this.sessionStateCache.SetBuildTransferContainers(containerList);
                this.navManager.NavigateTo(this.GenerateDestinationUrl(destinationPage, containerList, shellIndex));
                break;
        }

        // cant use await JsRuntime.InvokeAsync<object>("open", NavManager.BaseUri + url, "_blank"); to open in a new tab because builds are not carried over.
    }

    private string GenerateDestinationUrl(AppPage destinationPage, IEnumerable<ShipBuildContainer> containerList, string? shellIndex)
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

    private void LogMetrics(AppPage? leavingPage, string? metricLabel)
    {
        if (string.IsNullOrEmpty(metricLabel))
        {
            return;
        }

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

    public void NavigateTo(AppPage destinationPage, IEnumerable<ShipBuildContainer> containerList, AppPage? leavingPage = null, string? metricLabel = null) => this.GoToPage(destinationPage, containerList.ToList(), null, leavingPage, metricLabel);

    public void NavigateTo(AppPage destinationPage, ShipBuildContainer container, AppPage? leavingPage = null, string? metricLabel = null) => this.GoToPage(destinationPage, new() { container }, null, leavingPage, metricLabel);

    public void NavigateTo(AppPage destinationPage, ShipBuildContainer container, string shellIndex, AppPage? leavingPage = null, string? metricLabel = null) => this.GoToPage(destinationPage, new() { container }, shellIndex, leavingPage, metricLabel);
}
