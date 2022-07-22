using System.Collections.Generic;

namespace WoWsShipBuilder.Core.DataContainers;

public sealed record DepthChargeDamageDistributionChartRecord(int DcDmg, decimal SplashRadius, Dictionary<float, List<float>> PointsOfDmg = default!);
