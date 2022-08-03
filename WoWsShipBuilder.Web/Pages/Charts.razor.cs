﻿using MudBlazor;
using WoWsShipBuilder.Core.DataContainers;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Localization;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.Web.Components;
using WoWsShipBuilder.Web.Dialogs;
using WoWsShipBuilder.Web.Utility;

namespace WoWsShipBuilder.Web.Pages;

public partial class Charts
{
    private Breakpoint Breakpoint { get; set; }

    private const string HorizontalDispersionId = "horizontal-dispersion";
    private const string VerticalDispersionId = "vertical-dispersion";
    private const string TrajectoryId = "trajectory";
    private const string PenetrationId = "penetration";
    private const string FlightTimeId = "flight-time";
    private const string ImpactVelocityId = "impact-velocity";
    private const string ImpactAngleId = "impact-angle";

    private string shellIndexFromUrl = string.Empty;
    private string[] shipIndexesFromUrl = { };

    private bool processing;
    private double range = 10;
    private double aimingRange = 10;
    private int shotsNumber = 100;
    private bool verticalDispPlot;
    private double dispPlotScaling = 1.0;
    private int counter;
    private ChartsHelper.EllipsePlanes selectedVertDispersionPlane = ChartsHelper.EllipsePlanes.RealPlane;
    private ChartsHelper.EllipsePlanes selectedDispPlotPlane = ChartsHelper.EllipsePlanes.HorizontalPlane;
    private DispersionPlot.FusoPositions selectedFusoPosition = DispersionPlot.FusoPositions.DontShow;

    private MudTabs chartsTabs = default!;
    private readonly Dictionary<string, ChartsRenderingDataWrapper> displayedShips = new();
    private readonly List<DispersionEllipse> dispPlotShips = new();
    private readonly Dictionary<string, (IEnumerable<ChartsHelper.Point> verticalDispersionAtImpactAngle, IEnumerable<ChartsHelper.Point> verticalDispersionOnWater, IEnumerable<ChartsHelper.Point> verticalDispersionOnPerpendicularToWater)> verticalDispersionsCache = new();
    private readonly Dictionary<string, Dictionary<double, Ballistic>> ballisticCache = new();
    private readonly List<ChartsHelper.EllipsePlanes> ellipsePlanesList = Enum.GetValues<ChartsHelper.EllipsePlanes>().ToList();
    private readonly List<DispersionPlot.FusoPositions> fusoPositionsList = Enum.GetValues<DispersionPlot.FusoPositions>().ToList();

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        MetricsService.BallisticPageCount.Inc();

        if (NavManager.TryGetQueryString("shellIndex", out string shellIndex))
        {
            shellIndexFromUrl = shellIndex;
        }
        if (NavManager.TryGetQueryString("shipIndex", out string shipIndex))
        {
            shipIndexesFromUrl = shipIndex.Split(',');
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            SetProcessing(true);
            Breakpoint = await BreakpointService.GetBreakpoint();
            await SetupChartsAsync();
            if (shipIndexesFromUrl.Any())
            {
                List<ChartsRenderingDataWrapper> selectedShipList = new();

                if (shellIndexFromUrl.Any())
                {
                    string shipIndex = shipIndexesFromUrl.First();
                    var shipSummary = AppData.ShipSummaryList!.First(x => x.Index.Equals(shipIndex));
                    var ship = await WebAppDataService.GetShipFromSummary(shipSummary);

                    var gun = ship!.MainBatteryModuleList.Values.First(x => x.Guns.First().AmmoList.Contains(shellIndexFromUrl));
                    Dictionary<string, ArtilleryData> artilleryData = new();
                    IEnumerable<string> shellIndexes = ship.MainBatteryModuleList.SelectMany(turretModule => turretModule.Value.Guns.SelectMany(g => g.AmmoList)).Distinct();
                    foreach (string shellIndex in shellIndexes)
                    {
                        if (shellIndex.Equals(shellIndexFromUrl))
                        {
                            var shell = await WebAppDataService.GetProjectile<ArtilleryShell>(shellIndexFromUrl);
                            ArtilleryData data = new(shell, gun.DispersionValues, true);
                            artilleryData.Add(shellIndexFromUrl, data);
                        }
                        else
                        {
                            artilleryData.Add(shellIndex, new());
                        }
                    }
                    ChartsRenderingDataWrapper wrapper = new(shipIndex, artilleryData, decimal.ToDouble(gun.MaxRange), decimal.ToDouble(gun.Sigma));

                    selectedShipList.Add(wrapper);
                }
                else
                {
                    List<ChartsRenderingDataWrapper> shipList = new();
                    foreach (string shipIndex in shipIndexesFromUrl)
                    {
                        var shipSummary = AppData.ShipSummaryList!.Single(x => x.Index.Equals(shipIndex));
                        var ship = await WebAppDataService.GetShipFromSummary(shipSummary);
                        Dictionary<string, ArtilleryData> artilleryData = new();
                        if (ship!.MainBatteryModuleList != null && ship.MainBatteryModuleList.Count > 0)
                        {
                            IEnumerable<string> shellIndexes = ship.MainBatteryModuleList.SelectMany(turretModule => turretModule.Value.Guns.SelectMany(gun => gun.AmmoList)).Distinct();
                            foreach (string shellIndex in shellIndexes)
                            {
                                artilleryData.Add(shellIndex, new());
                            }
                        }

                        ChartsRenderingDataWrapper wrapper = new(shipIndex, artilleryData);
                        shipList.Add(wrapper);
                    }

                    DialogOptions options = new()
                    {
                        MaxWidth = MaxWidth.Small,
                        FullWidth = true,
                    };

                    var parameters = new DialogParameters
                    {
                        ["shipList"] = shipList,
                    };

                    var dialog = DialogService.Show<ShellSelectionDialog>("ShellSelectionDialog", parameters, options);
                    var result = await dialog.Result;
                    if (result is null || result.Cancelled)
                    {
                        SetProcessing(false);
                        return;
                    }
                    selectedShipList = ((IEnumerable<ChartsRenderingDataWrapper>)result.Data).ToList();
                }
                await UpdateShipsBatchAsync(selectedShipList);
            }
            SetProcessing(false);
        }
    }

    private async Task SetupChartsAsync()
    {
        string km = Localizer.GetAppLocalization(nameof(Translation.Unit_KM)).Localization;
        string m = Localizer.GetAppLocalization(nameof(Translation.Unit_M)).Localization;
        string rangeString = Localizer.GetAppLocalization(nameof(Translation.ShipStats_Range)).Localization;
        string s = Localizer.GetAppLocalization(nameof(Translation.Unit_S)).Localization;
        string mm = Localizer.GetAppLocalization(nameof(Translation.Unit_MM)).Localization;
        string degree = Localizer.GetAppLocalization(nameof(Translation.Unit_Degree)).Localization;
        string mps = Localizer.GetAppLocalization(nameof(Translation.Unit_MPS)).Localization;

        double aspectRatio = 3;
        if (Breakpoint is Breakpoint.Sm or Breakpoint.Xs)
        {
            aspectRatio = 1.2;
        }

        await ChartJsInterop.SetupGlobalChartConfigAsync(aspectRatio);

        await ChartJsInterop.CreateChartAsync(HorizontalDispersionId, Localizer.GetAppLocalization(nameof(Translation.DispersionGraphWindow_Horizontal)).Localization, rangeString, Localizer.GetAppLocalization(nameof(Translation.ShipStats_Dispersion)).Localization, km, m);
        await ChartJsInterop.CreateChartAsync(VerticalDispersionId, Localizer.GetAppLocalization(nameof(Translation.DispersionGraphWindow_Vertical)).Localization, rangeString, Localizer.GetAppLocalization(nameof(Translation.ShipStats_Dispersion)).Localization, km, m);
        await ChartJsInterop.CreateChartAsync(TrajectoryId, Localizer.GetAppLocalization(nameof(Translation.DispersionGraphWindow_ShellsPath)).Localization, rangeString, Localizer.GetAppLocalization(Translation.ChartsWeb_Height).Localization, km, m);
        await ChartJsInterop.CreateChartAsync(PenetrationId, Localizer.GetAppLocalization(nameof(Translation.ShipStats_Penetration)).Localization, rangeString, Localizer.GetAppLocalization(nameof(Translation.ShipStats_Penetration)).Localization, km, mm);
        await ChartJsInterop.CreateChartAsync(FlightTimeId, Localizer.GetAppLocalization(nameof(Translation.DispersionGraphWindow_FlightTime)).Localization, rangeString, Localizer.GetAppLocalization(nameof(Translation.DispersionGraphWindow_FlightTime)).Localization, km, s);
        await ChartJsInterop.CreateChartAsync(ImpactVelocityId, Localizer.GetAppLocalization(nameof(Translation.DispersionGraphWindow_ImpactVelocity)).Localization, rangeString, Localizer.GetAppLocalization(nameof(Translation.DispersionGraphWindow_ImpactVelocity)).Localization, km, mps);
        await ChartJsInterop.CreateChartAsync(ImpactAngleId, Localizer.GetAppLocalization(nameof(Translation.DispersionGraphWindow_ImpactAngle)).Localization, rangeString, Localizer.GetAppLocalization(nameof(Translation.DispersionGraphWindow_ImpactAngle)).Localization, km, degree);
        await ChartJsInterop.ChangeSuggestedMaxAsync(TrajectoryId, range / 4 * 1000);
    }

    private async Task UpdateShipsBatchAsync(List<ChartsRenderingDataWrapper> shipList)
    {
        List<string> chartIds = GetChartsIdList();

    //IEnumerable<Point> is a single dataset. The middle List contains all datasets of a certain chart. The outermost List contains the list of datasets of each chart.
        List<List<IEnumerable<ChartsHelper.Point>>> data = new();
        List<int> indexes = new();
        List<string> labelNames = new();
        List<string> newLabels = new();

        foreach (var selectedShip in shipList)
        {
            foreach ((string shellIndex, (var shell, var dispersionValues, bool shellSelected)) in selectedShip.ArtilleryDataDictionary)
            {
                if (!shellSelected) continue;

                var labelName = $"{selectedShip.ShipIndex} - {shellIndex}";
                labelNames.Add(labelName);

                if (displayedShips.ContainsKey(labelName)) continue;

                Dictionary<double, Ballistic> ballisticSeries = GetBallistic(labelName, shell, selectedShip.MaxRange);

                List<IEnumerable<ChartsHelper.Point>> shipData = new()
                {
                    ChartsHelper.CreateHorizontalDispersionSeries(dispersionValues, selectedShip.MaxRange),
                    GetVerticalDispersion(labelName, dispersionValues, selectedShip.MaxRange, ballisticSeries),
                    ChartsHelper.CreateTrajectoryDataset(ballisticSeries, range),
                    ChartsHelper.CreateBallisticChartDataset(ballisticSeries, ChartsHelper.BallisticParameter.Penetration),
                    ChartsHelper.CreateBallisticChartDataset(ballisticSeries, ChartsHelper.BallisticParameter.FlightTime),
                    ChartsHelper.CreateBallisticChartDataset(ballisticSeries, ChartsHelper.BallisticParameter.ImpactVelocity),
                    ChartsHelper.CreateBallisticChartDataset(ballisticSeries, ChartsHelper.BallisticParameter.ImpactAngle),
                };
                data.Add(shipData);
                newLabels.Add(labelName);
                indexes.Add(counter);
                counter++;

                displayedShips.Add(labelName, selectedShip);
                dispPlotShips.Add(DispersionPlotHelper.CalculateDispersionPlotParameters(labelName, dispersionValues, shell, selectedShip.MaxRange, aimingRange * 1000, selectedShip.Sigma, shotsNumber));
            }
        }

        List<string> localizedLabels = newLabels.Select(x => $"{Localizer.GetGameLocalization($"{x[..x.IndexOf(" - ", StringComparison.Ordinal)]}_FULL").Localization} - {Localizer.GetGameLocalization(x[(x.IndexOf(" - ", StringComparison.Ordinal) + 3)..]).Localization}").ToList();
        await ChartJsInterop.BatchAddDataAsync(chartIds, localizedLabels, data, indexes);

        List<string> removedShips = displayedShips.Where(x => !labelNames.Contains(x.Key)).Select(x => x.Key).ToList();
        foreach (string labelName in removedShips)
        {
            displayedShips.Remove(labelName);
            dispPlotShips.Remove(dispPlotShips.First(x => x.Name.Equals(labelName)));
        }

        List<string> removedShipsLocalizedLabels = removedShips.Select(x => $"{Localizer.GetGameLocalization($"{x[..x.IndexOf(" - ", StringComparison.Ordinal)]}_FULL").Localization} - {Localizer.GetGameLocalization(x[(x.IndexOf(" - ", StringComparison.Ordinal) + 3)..]).Localization}").ToList();
        await ChartJsInterop.BatchRemoveDataAsync(chartIds, removedShipsLocalizedLabels);
    }

    private Dictionary<double, Ballistic> GetBallistic(string label, ArtilleryShell shell, double maxRange)
    {
        if (ballisticCache.ContainsKey(label))
        {
            return ballisticCache[label];
        }
        Dictionary<double, Ballistic> ballisticSeries = BallisticHelper.CalculateBallistic(shell, maxRange);
        ballisticCache.Add(label, ballisticSeries);
        return ballisticSeries;
    }

    private IEnumerable<ChartsHelper.Point> GetVerticalDispersion(string label, Dispersion dispersionValues, double maxRange, Dictionary<double, Ballistic> ballisticSeries)
    {
        (IEnumerable<ChartsHelper.Point> verticalDispersionAtImpactAngle, IEnumerable<ChartsHelper.Point> verticalDispersionOnWater, IEnumerable<ChartsHelper.Point> verticalDispersionOnPerpendicularToWater) verticalDispersions;
        if (verticalDispersionsCache.ContainsKey(label))
        {
            verticalDispersions = verticalDispersionsCache[label];
        }
        else
        {
            verticalDispersions = ChartsHelper.CreateVerticalDispersionSeries(dispersionValues, maxRange, ballisticSeries);
            verticalDispersionsCache.Add(label, verticalDispersions);
        }
        return ChartsHelper.SelectVerticalDispersionDataset(verticalDispersions, selectedVertDispersionPlane);
    }

    private async Task ChangeVerticalDispPlaneAsync()
    {
        SetProcessing(true);
        List<IEnumerable<ChartsHelper.Point>> selectedVerticals = displayedShips.Select(item => ChartsHelper.SelectVerticalDispersionDataset(verticalDispersionsCache[item.Key], selectedVertDispersionPlane)).ToList();
        List<string> localizedLabels = displayedShips.Keys.Select(x => $"{Localizer.GetGameLocalization($"{x[..x.IndexOf(" - ", StringComparison.Ordinal)]}_FULL").Localization} - {Localizer.GetGameLocalization(x[(x.IndexOf(" - ", StringComparison.Ordinal) + 3)..]).Localization}").ToList();
        await ChartJsInterop.BatchUpdateDataAsync(VerticalDispersionId, localizedLabels, selectedVerticals);
        SetProcessing(false);
    }

    private async Task UpdateTrajectoryAsync(double selectedRange)
    {
        SetProcessing(true);
        range = selectedRange;
        List<IEnumerable<ChartsHelper.Point>> trajectoryData = displayedShips.Select(item => ChartsHelper.CreateTrajectoryDataset(ballisticCache[item.Key], range)).ToList();
        List<string> localizedLabels = displayedShips.Keys.Select(x => $"{Localizer.GetGameLocalization($"{x[..x.IndexOf(" - ", StringComparison.Ordinal)]}_FULL").Localization} - {Localizer.GetGameLocalization(x[(x.IndexOf(" - ", StringComparison.Ordinal) + 3)..]).Localization}").ToList();
        await ChartJsInterop.BatchUpdateDataAsync(TrajectoryId, localizedLabels, trajectoryData);
        await ChartJsInterop.ChangeSuggestedMaxAsync(TrajectoryId, aimingRange / 4 * 1000);
        SetProcessing(false);
    }

    private async Task EditChartsAsync()
    {
        SetProcessing(true);
        var selectedShipList = await GetSelectedShipsAsync();
        if (selectedShipList is null || selectedShipList.Cancelled)
        {
            SetProcessing(false);
            return;
        }

        await UpdateShipsBatchAsync(((IEnumerable<ChartsRenderingDataWrapper>)selectedShipList.Data).ToList());
        SetProcessing(false);
    }

    private async Task<DialogResult?> GetSelectedShipsAsync()
    {
        DialogOptions options = new()
        {
            MaxWidth = MaxWidth.ExtraLarge,
        };
        var parameters = new DialogParameters
        {
            ["shipList"] = displayedShips.OrderBy(x => Localizer.GetGameLocalization($"{x.Key[..x.Key.IndexOf(" - ", StringComparison.Ordinal)]}_FULL").Localization).Select(x => x.Value).DistinctBy(x => x.ShipIndex).ToList(),
        };
        var dialog = DialogService.Show<ShipAndShellSelectionDialog>("ShipAndShellSelectionDialog", parameters, options);
        return await dialog.Result;
    }

    private static List<string> GetChartsIdList()
    {
        List<string> chartIds = new()
        {
            HorizontalDispersionId,
            VerticalDispersionId,
            TrajectoryId,
            PenetrationId,
            FlightTimeId,
            ImpactVelocityId,
            ImpactAngleId,
        };
        return chartIds;
    }

    private string GetPositionStyleForSelector()
    {
        return Breakpoint is Breakpoint.Xs or Breakpoint.Sm ? "margin-right: auto !important;" : "position: absolute;";
    }

    //Convenience method to call StateHasChanged after setting processing to false. Needed for chrome to correctly update the Ship/Shell selection button.
    private void SetProcessing(bool newProcessing)
    {
        processing = newProcessing;
        if (!newProcessing)
        {
            StateHasChanged();
        }
    }

    private void UpdateDispersionPlotParameters()
    {
        List<DispersionEllipse> data = dispPlotShips.Select(dispersionEllipse => DispersionPlotHelper.CalculateDispersionPlotParameters(dispersionEllipse.Name, dispersionEllipse.DispersionData, dispersionEllipse.Shell, dispersionEllipse.MaxRange, aimingRange * 1000, dispersionEllipse.Sigma, shotsNumber)).ToList();
        dispPlotShips.Clear();
        dispPlotShips.AddRange(data);
    }

    private void UpdateDispersionPlotRange(double selectedValue)
    {
        aimingRange = selectedValue;
        UpdateDispersionPlotParameters();
    }

    private void UpdateDispersionPlotShotsNumber(int selectedValue)
    {
        shotsNumber = selectedValue;
        UpdateDispersionPlotParameters();
    }

    private void UpdateDispersionPlotScaling(double selectedValue)
    {
        dispPlotScaling = Math.Round(selectedValue, 1);
    }

    private void SetVerticalDispersionPlot(bool active)
    {
        verticalDispPlot = active;
    }

    private bool SetFullWidth()
    {
        return Breakpoint is Breakpoint.Xs or Breakpoint.Sm;
    }
}
