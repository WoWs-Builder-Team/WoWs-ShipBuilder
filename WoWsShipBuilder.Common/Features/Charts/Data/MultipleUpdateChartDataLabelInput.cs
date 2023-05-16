namespace WoWsShipBuilder.Common.Features.Charts.Data;

public record MultipleUpdateChartDataLabelInput(string Id, int Index, string NewId, string NewLabel, List<IEnumerable<ChartsHelper.Point>> Datasets);
