namespace WoWsShipBuilder.Features.ShipStats;

public sealed record DepthChargeDamageDistributionChartRecord(int DcDmg, decimal SplashRadius, Dictionary<float, List<float>> PointsOfDmg = default!);
