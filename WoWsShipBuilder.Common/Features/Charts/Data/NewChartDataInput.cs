namespace WoWsShipBuilder.Common.Features.Charts.Data;

public record NewChartDataInput(string Id, string Label, List<IEnumerable<ChartsHelper.Point>> Datasets, int Index);
