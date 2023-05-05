using WoWsShipBuilder.Core.DataContainers;
using WoWsShipBuilder.Core.Localization;

namespace WoWsShipBuilder.ViewModels.Helper.GridData;

public class AswGridDataWrapper
{
    public AswGridDataWrapper(AirstrikeDataContainer? aswAirStrike, DepthChargesLauncherDataContainer? depthChargeLauncher)
    {
        var depthCharge = depthChargeLauncher?.DepthCharge ?? aswAirStrike?.Weapon as DepthChargeDataContainer;

        DcType = aswAirStrike is not null ? nameof(Translation.ShipStats_AswAirstrike) : depthChargeLauncher is not null ? nameof(Translation.DepthCharge) : null;
        Range = aswAirStrike?.MaximumDistance;
        MaxDropLength = aswAirStrike?.MaximumFlightDistance;
        DcReload = depthChargeLauncher?.Reload ?? aswAirStrike?.ReloadTime;
        DcUses = depthChargeLauncher?.NumberOfUses ?? aswAirStrike?.NumberOfUses;
        PlanesInSquadron = aswAirStrike?.NumberDuringAttack;
        BombsPerPlane = aswAirStrike?.BombsPerPlane;
        DcPerAttack = depthChargeLauncher?.BombsPerCharge;
        DcDamage = depthCharge?.Damage;
        DcFireChance = depthCharge?.FireChance;
        DcFloodingChance = depthCharge?.FloodingChance;
        DcSplashRadius = depthCharge?.DcSplashRadius;
        DcSinkSpeed = depthCharge?.SinkSpeed;
        DcDetonationTimer = depthCharge?.DetonationTimer;
        DcDetonationDepth = depthCharge?.DetonationDepth;
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
