namespace WoWsShipBuilder.Features.Charts.Data;

public record UpdateChartDataInput(string Id, IEnumerable<ChartsHelper.Point> Datasets);
