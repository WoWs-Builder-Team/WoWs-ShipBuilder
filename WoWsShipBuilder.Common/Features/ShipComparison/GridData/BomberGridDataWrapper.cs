using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.Features.DataContainers;

namespace WoWsShipBuilder.Features.ShipComparison.GridData;

public class BomberGridDataWrapper : PlaneGridDataWrapper
{
    public BomberGridDataWrapper(IReadOnlyCollection<CvAircraftDataContainer>? bombers)
    {
        Type = bombers?.Select(x => x.PlaneVariant).ToList() ?? new();
        InSquadron = bombers?.Select(x => x.NumberInSquad).ToList() ?? new();
        PerAttack = bombers?.Select(x => x.NumberDuringAttack).ToList() ?? new();
        OnDeck = bombers?.Select(x => x.MaxNumberOnDeck).ToList() ?? new();
        RestorationTime = bombers?.Select(x => x.RestorationTime).ToList() ?? new();
        CruisingSpeed = bombers?.Select(x => x.CruisingSpeed).ToList() ?? new();
        MaxSpeed = bombers?.Select(x => x.MaxSpeed).ToList() ?? new();
        MinSpeed = bombers?.Select(x => x.MinSpeed).ToList() ?? new();
        EngineBoostDuration = bombers?.Select(x => x.MaxEngineBoostDuration).ToList() ?? new();
        InitialBoostDuration = bombers?.Select(x => x.JatoDuration).ToList() ?? new();
        InitialBoostValue = bombers?.Select(x => x.JatoSpeedMultiplier).ToList() ?? new();
        PlaneHp = bombers?.Select(x => x.PlaneHp).ToList() ?? new();
        SquadronHp = bombers?.Select(x => x.SquadronHp).ToList() ?? new();
        AttackGroupHp = bombers?.Select(x => x.AttackGroupHp).ToList() ?? new();
        DamageDuringAttack = bombers?.Select(x => x.DamageTakenDuringAttack).ToList() ?? new();
        WeaponsPerPlane = bombers?.Select(x => x.AmmoPerAttack).ToList() ?? new();
        PreparationTime = bombers?.Select(x => x.PreparationTime).ToList() ?? new();
        AimingTime = bombers?.Select(x => x.AimingTime).ToList() ?? new();
        TimeToFullyAimed = bombers?.Select(x => x.TimeToFullyAimed).ToList() ?? new();
        PostAttackInvulnerability = bombers?.Select(x => x.PostAttackInvulnerabilityDuration).ToList() ?? new();
        AttackCooldown = bombers?.Select(x => x.AttackCd).ToList() ?? new();
        Concealment = bombers?.Select(x => x.ConcealmentFromShips).ToList() ?? new();
        Spotting = bombers?.Select(x => x.MaxViewDistance).ToList() ?? new();
        AreaChangeAiming = bombers?.Select(x => x.AimingRateMoving).ToList() ?? new();
        AreaChangePreparation = bombers?.Select(x => x.AimingPreparationRateMoving).ToList() ?? new();
        InnerEllipse = bombers?.Select(x => x.InnerBombPercentage).ToList() ?? new();

        List<BombDataContainer?>? bombs = bombers?.Select(x => x.Weapon as BombDataContainer).ToList();

        WeaponType = bombers?.Select(x => x.WeaponType).ToList() ?? new();
        WeaponBombType = bombs?.Select(x => x?.BombType ?? default!).ToList() ?? new();
        WeaponDamage = bombs?.Select(x => x?.Damage ?? 0).ToList() ?? new();
        WeaponSplashRadius = bombs?.Select(x => x?.SplashRadius ?? 0).ToList() ?? new();
        WeaponSplashDamage = bombs?.Select(x => x?.SplashDmg ?? 0).ToList() ?? new();
        WeaponPenetration = bombs?.Select(x => (x?.BombType == $"ArmamentType_{BombType.AP}" ? x.PenetrationAp : x?.Penetration) ?? 0).ToList() ?? new();
        WeaponFireChance = bombs?.Select(x => x?.FireChance ?? 0).ToList() ?? new();
        WeaponBlastRadius = bombs?.Select(x => x?.ExplosionRadius ?? 0).ToList() ?? new();
        WeaponBlastPenetration = bombs?.Select(x => x?.SplashCoeff ?? 0).ToList() ?? new();
        WeaponFuseTimer = bombs?.Select(x => x?.FuseTimer ?? 0).ToList() ?? new();
        WeaponArmingThreshold = bombs?.Select(x => x?.ArmingThreshold ?? 0).ToList() ?? new();
        WeaponRicochetAngles = bombs?.Select(x => x?.RicochetAngles ?? default!).ToList() ?? new();
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
