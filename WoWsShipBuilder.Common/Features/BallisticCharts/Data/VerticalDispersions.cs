namespace WoWsShipBuilder.Features.BallisticCharts.Data;

public sealed record VerticalDispersions(IEnumerable<ChartsHelper.Point> VerticalDispersionAtImpactAngle, IEnumerable<ChartsHelper.Point> VerticalDispersionOnWater, IEnumerable<ChartsHelper.Point> VerticalDispersionOnPerpendicularToWater);
