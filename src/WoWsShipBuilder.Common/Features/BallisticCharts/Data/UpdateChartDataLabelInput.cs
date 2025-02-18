namespace WoWsShipBuilder.Features.BallisticCharts.Data;

public record UpdateChartDataLabelInput(string Id, string NewId, string NewLabel, IEnumerable<ChartsHelper.Point> Datasets);
