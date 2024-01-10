using System.Collections.Immutable;
using WoWsShipBuilder.DataElements.DataElementAttributes;
using WoWsShipBuilder.DataStructures.Modifiers;
using WoWsShipBuilder.DataStructures.Projectile;
using WoWsShipBuilder.Infrastructure.ApplicationData;

namespace WoWsShipBuilder.Features.DataContainers;

[DataContainer]
public partial class DepthChargeDataContainer : ProjectileDataContainer
{
    [DataElementType(DataElementTypes.KeyValue)]
    public int Damage { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "MPS")]
    public string SinkSpeed { get; set; } = default!;

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "S")]
    public string DetonationTimer { get; set; } = default!;

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "M")]
    public string DetonationDepth { get; set; } = default!;

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "M")]
    public decimal DcSplashRadius { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "PerCent")]
    public decimal FireChance { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "PerCent")]
    public decimal FloodingChance { get; set; }

    public Dictionary<float, List<float>> PointsOfDmg { get; set; } = default!;

    public static DepthChargeDataContainer FromChargesName(string name, ImmutableList<Modifier> modifiers)
    {
        var depthCharge = AppData.FindProjectile<DepthCharge>(name);
        decimal damage = modifiers.ApplyModifiers("DepthChargeDataContainer.Damage", (decimal)depthCharge.Damage);
        decimal minSpeed = (decimal)(depthCharge.SinkingSpeed * (1 - depthCharge.SinkingSpeedRng)) * Constants.KnotsToMps;
        decimal maxSpeed = (decimal)(depthCharge.SinkingSpeed * (1 + depthCharge.SinkingSpeedRng)) * Constants.KnotsToMps;
        decimal minTimer = (decimal)(depthCharge.DetonationTimer - depthCharge.DetonationTimerRng);
        decimal maxTimer = (decimal)(depthCharge.DetonationTimer + depthCharge.DetonationTimerRng);
        decimal minDetDepth = minSpeed * minTimer / 2;
        decimal maxDetDepth = maxSpeed * maxTimer / 2;

        var depthChargeDataContainer = new DepthChargeDataContainer
        {
            Damage = (int)Math.Round(damage, 0),
            FireChance = Math.Round((decimal)depthCharge.FireChance * 100, 2),
            FloodingChance = Math.Round((decimal)depthCharge.FloodChance * 100, 2),
            DcSplashRadius = Math.Round((decimal)depthCharge.ExplosionRadius, 2),
            SinkSpeed = $"{Math.Round(minSpeed, 1)} ~ {Math.Round(maxSpeed, 1)}",
            DetonationTimer = $"{Math.Round(minTimer, 1)} ~ {Math.Round(maxTimer, 1)}",
            DetonationDepth = $"{Math.Round(minDetDepth)} ~ {Math.Round(maxDetDepth)}",
            PointsOfDmg = depthCharge.PointsOfDamage.ToDictionary(x => x.Key, x => x.Value.ToList()),
        };

        depthChargeDataContainer.UpdateDataElements();

        return depthChargeDataContainer;
    }
}
