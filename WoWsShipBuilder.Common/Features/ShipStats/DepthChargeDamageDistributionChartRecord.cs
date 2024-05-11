using System.Collections.Immutable;

namespace WoWsShipBuilder.Features.ShipStats;

public sealed record DepthChargeDamageDistributionChartRecord(int DcDmg, decimal SplashRadius, ImmutableDictionary<float, ImmutableList<float>> PointsOfDmg);
