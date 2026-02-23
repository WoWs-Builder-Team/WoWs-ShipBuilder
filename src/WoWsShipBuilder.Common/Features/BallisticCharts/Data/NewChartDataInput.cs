namespace WoWsShipBuilder.Features.BallisticCharts.Data;

public record NewChartDataInput(string Id, string Label, List<IEnumerable<ChartsHelper.Point>> Datasets, int Index);
