using WoWsShipBuilder.Features.DataContainers;
using WoWsShipBuilder.Infrastructure.Localization.Resources;

namespace WoWsShipBuilder.Features.ShipComparison.GridData;

public class AswGridDataWrapper
{
    public AswGridDataWrapper(AirstrikeDataContainer? aswAirStrike, DepthChargesLauncherDataContainer? depthChargeLauncher)
    {
        var depthCharge = depthChargeLauncher?.DepthCharge ?? aswAirStrike?.Weapon as DepthChargeDataContainer;

        this.DcType = aswAirStrike switch
        {
            not null => nameof(Translation.ShipStats_AswAirstrike),
            null when depthChargeLauncher is not null => nameof(Translation.DepthCharge),
            _ => null,
        };

        this.Range = aswAirStrike?.MaximumDistance;
        this.MaxDropLength = aswAirStrike?.MaximumFlightDistance;
        this.DcReload = depthChargeLauncher?.Reload ?? aswAirStrike?.ReloadTime;
        this.DcUses = depthChargeLauncher?.NumberOfUses ?? aswAirStrike?.NumberOfUses;
        this.PlanesInSquadron = aswAirStrike?.NumberDuringAttack;
        this.BombsPerPlane = aswAirStrike?.BombsPerPlane;
        this.DcPerAttack = depthChargeLauncher?.BombsPerCharge;
        this.DcDamage = depthCharge?.Damage;
        this.DcFireChance = depthCharge?.FireChance;
        this.DcFloodingChance = depthCharge?.FloodingChance;
        this.DcSplashRadius = depthCharge?.DcSplashRadius;
        this.DcSinkSpeed = depthCharge?.SinkSpeed;
        this.DcDetonationTimer = depthCharge?.DetonationTimer;
        this.DcDetonationDepth = depthCharge?.DetonationDepth;
    }

    public string? DcType { get; }

    public decimal? Range { get; }

    public decimal? MaxDropLength { get; }

    public decimal? DcReload { get; }

    public int? DcUses { get; }

    public int? PlanesInSquadron { get; }

    public int? BombsPerPlane { get; }

    public decimal? DcPerAttack { get; }

    public decimal? DcDamage { get; }

    public decimal? DcFireChance { get; }

    public decimal? DcFloodingChance { get; }

    public decimal? DcSplashRadius { get; }

    public string? DcSinkSpeed { get; }

    public string? DcDetonationTimer { get; }

    public string? DcDetonationDepth { get; }
}
