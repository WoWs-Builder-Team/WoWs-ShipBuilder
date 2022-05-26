using System.Collections.Generic;

namespace WoWsShipBuilder.Core.DataContainers;

public sealed record DepthChargeDamageDistributionChartRecord
{
    public DepthChargeDamageDistributionChartRecord(DepthChargeDataContainer depthCharge)
    {
        DcDmg = depthCharge.Damage;
        SplashRadius = depthCharge.DcSplashRadius;
        PointsOfDmg = depthCharge.PointsOfDmg;
    }

    public int DcDmg { get; }

    public decimal SplashRadius { get; }

    public Dictionary<float, List<float>> PointsOfDmg { get; } = default!;
}
