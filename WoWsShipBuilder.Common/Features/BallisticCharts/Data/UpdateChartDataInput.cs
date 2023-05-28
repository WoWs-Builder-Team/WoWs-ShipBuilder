namespace WoWsShipBuilder.Features.BallisticCharts.Data;

public record UpdateChartDataInput(string Id, IEnumerable<ChartsHelper.Point> Datasets);
