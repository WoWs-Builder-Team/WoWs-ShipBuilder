using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.Features.DataContainers;

namespace WoWsShipBuilder.Features.ShipComparison.GridData;

public class BomberGridDataWrapper : PlaneGridDataWrapper
{
    public BomberGridDataWrapper(IReadOnlyCollection<CvAircraftDataContainer>? bombers)
    {
        this.Type = bombers?.Select(x => x.PlaneVariant).ToList() ?? new();
        this.InSquadron = bombers?.Select(x => x.NumberInSquad).ToList() ?? new();
        this.PerAttack = bombers?.Select(x => x.NumberDuringAttack).ToList() ?? new();
        this.OnDeck = bombers?.Select(x => x.MaxNumberOnDeck).ToList() ?? new();
        this.RestorationTime = bombers?.Select(x => x.RestorationTime).ToList() ?? new();
        this.CruisingSpeed = bombers?.Select(x => x.CruisingSpeed).ToList() ?? new();
        this.MaxSpeed = bombers?.Select(x => x.MaxSpeed).ToList() ?? new();
        this.MinSpeed = bombers?.Select(x => x.MinSpeed).ToList() ?? new();
        this.EngineBoostDuration = bombers?.Select(x => x.MaxEngineBoostDuration).ToList() ?? new();
        this.InitialBoostDuration = bombers?.Select(x => x.JatoDuration).ToList() ?? new();
        this.InitialBoostValue = bombers?.Select(x => x.JatoSpeedMultiplier).ToList() ?? new();
        this.PlaneHp = bombers?.Select(x => x.PlaneHp).ToList() ?? new();
        this.SquadronHp = bombers?.Select(x => x.SquadronHp).ToList() ?? new();
        this.AttackGroupHp = bombers?.Select(x => x.AttackGroupHp).ToList() ?? new();
        this.DamageDuringAttack = bombers?.Select(x => x.DamageTakenDuringAttack).ToList() ?? new();
        this.WeaponsPerPlane = bombers?.Select(x => x.AmmoPerAttack).ToList() ?? new();
        this.PreparationTime = bombers?.Select(x => x.PreparationTime).ToList() ?? new();
        this.AimingTime = bombers?.Select(x => x.AimingTime).ToList() ?? new();
        this.TimeToFullyAimed = bombers?.Select(x => x.TimeToFullyAimed).ToList() ?? new();
        this.PostAttackInvulnerability = bombers?.Select(x => x.PostAttackInvulnerabilityDuration).ToList() ?? new();
        this.AttackCooldown = bombers?.Select(x => x.AttackCd).ToList() ?? new();
        this.Concealment = bombers?.Select(x => x.ConcealmentFromShips).ToList() ?? new();
        this.Spotting = bombers?.Select(x => x.MaxViewDistance).ToList() ?? new();
        this.AreaChangeAiming = bombers?.Select(x => x.AimingRateMoving).ToList() ?? new();
        this.AreaChangePreparation = bombers?.Select(x => x.AimingPreparationRateMoving).ToList() ?? new();
        this.InnerEllipse = bombers?.Select(x => x.InnerBombPercentage).ToList() ?? new();

        List<BombDataContainer?>? bombs = bombers?.Select(x => x.Weapon as BombDataContainer).ToList();

        this.WeaponType = bombers?.Select(x => x.WeaponType).ToList() ?? new();
        this.WeaponBombType = bombs?.Select(x => x?.BombType ?? default!).ToList() ?? new();
        this.WeaponDamage = bombs?.Select(x => x?.Damage ?? 0).ToList() ?? new();
        this.WeaponSplashRadius = bombs?.Select(x => x?.SplashRadius ?? 0).ToList() ?? new();
        this.WeaponSplashDamage = bombs?.Select(x => x?.SplashDmg ?? 0).ToList() ?? new();
        this.WeaponPenetration = bombs?.Select(x => (x?.BombType == $"ArmamentType_{BombType.AP}" ? x.PenetrationAp : x?.Penetration) ?? 0).ToList() ?? new();
        this.WeaponFireChance = bombs?.Select(x => x?.FireChance ?? 0).ToList() ?? new();
        this.WeaponBlastRadius = bombs?.Select(x => x?.ExplosionRadius ?? 0).ToList() ?? new();
        this.WeaponBlastPenetration = bombs?.Select(x => x?.SplashCoeff ?? 0).ToList() ?? new();
        this.WeaponFuseTimer = bombs?.Select(x => x?.FuseTimer ?? 0).ToList() ?? new();
        this.WeaponArmingThreshold = bombs?.Select(x => x?.ArmingThreshold ?? 0).ToList() ?? new();
        this.WeaponRicochetAngles = bombs?.Select(x => x?.RicochetAngles ?? default!).ToList() ?? new();
    }

    public List<int> InnerEllipse { get; }

    public List<string> WeaponType { get; }

    public List<string> WeaponBombType { get; }

    public List<decimal> WeaponDamage { get; }

    public List<decimal> WeaponSplashRadius { get; }

    public List<decimal> WeaponSplashDamage { get; }

    public List<int> WeaponPenetration { get; }

    public List<decimal> WeaponFireChance { get; }

    public List<decimal> WeaponBlastRadius { get; }

    public List<decimal> WeaponBlastPenetration { get; }

    public List<decimal> WeaponFuseTimer { get; }

    public List<int> WeaponArmingThreshold { get; }

    public List<string> WeaponRicochetAngles { get; }
}
