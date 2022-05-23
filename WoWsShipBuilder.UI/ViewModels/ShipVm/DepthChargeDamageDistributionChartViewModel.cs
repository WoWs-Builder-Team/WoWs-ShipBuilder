using System.Collections.Generic;
using WoWsShipBuilder.Core.DataContainers;

namespace WoWsShipBuilder.UI.ViewModels.ShipVm;

public sealed record DepthChargeDamageDistributionChartViewModel
{
    public DepthChargeDamageDistributionChartViewModel(DepthChargeDataContainer depthCharge)
    {
        DcDmg = depthCharge.Damage;
        SplashRadius = depthCharge.DcSplashRadius;
        PointsOfDmg = depthCharge.PointsOfDmg;
    }

    public int DcDmg { get; }

    public decimal SplashRadius { get; }

    public Dictionary<float, List<float>> PointsOfDmg { get; } = default!;
}
