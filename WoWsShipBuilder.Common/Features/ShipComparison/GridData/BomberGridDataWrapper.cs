using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.Features.DataContainers;

namespace WoWsShipBuilder.Features.ShipComparison.GridData;

public class BomberGridDataWrapper : PlaneGridDataWrapper
{
    public BomberGridDataWrapper(IReadOnlyCollection<CvAircraftDataContainer>? bombers)
    {
        this.Type = bombers?.Select(x => x.PlaneVariant).ToNoSortList() ?? new();
        this.InSquadron = bombers?.Select(x => x.NumberInSquad).ToNoSortList() ?? new();
        this.PerAttack = bombers?.Select(x => x.NumberDuringAttack).ToNoSortList() ?? new();
        this.OnDeck = bombers?.Select(x => x.MaxNumberOnDeck).ToNoSortList() ?? new();
        this.RestorationTime = bombers?.Select(x => x.RestorationTime).ToNoSortList() ?? new();
        this.CruisingSpeed = bombers?.Select(x => x.CruisingSpeed).ToNoSortList() ?? new();
        this.MaxSpeed = bombers?.Select(x => x.MaxSpeed).ToNoSortList() ?? new();
        this.MinSpeed = bombers?.Select(x => x.MinSpeed).ToNoSortList() ?? new();
        this.EngineBoostDuration = bombers?.Select(x => x.MaxEngineBoostDuration).ToNoSortList() ?? new();
        this.InitialBoostDuration = bombers?.Select(x => x.JatoDuration).ToNoSortList() ?? new();
        this.InitialBoostValue = bombers?.Select(x => x.JatoSpeedMultiplier).ToNoSortList() ?? new();
        this.PlaneHp = bombers?.Select(x => x.PlaneHp).ToNoSortList() ?? new();
        this.SquadronHp = bombers?.Select(x => x.SquadronHp).ToNoSortList() ?? new();
        this.AttackGroupHp = bombers?.Select(x => x.AttackGroupHp).ToNoSortList() ?? new();
        this.DamageDuringAttack = bombers?.Select(x => x.DamageTakenDuringAttack).ToNoSortList() ?? new();
        this.WeaponsPerPlane = bombers?.Select(x => x.AmmoPerAttack).ToNoSortList() ?? new();
        this.PreparationTime = bombers?.Select(x => x.PreparationTime).ToNoSortList() ?? new();
        this.AimingTime = bombers?.Select(x => x.AimingTime).ToNoSortList() ?? new();
        this.TimeToFullyAimed = bombers?.Select(x => x.TimeToFullyAimed).ToNoSortList() ?? new();
        this.PostAttackInvulnerability = bombers?.Select(x => x.PostAttackInvulnerabilityDuration).ToNoSortList() ?? new();
        this.AttackCooldown = bombers?.Select(x => x.AttackCd).ToNoSortList() ?? new();
        this.Concealment = bombers?.Select(x => x.ConcealmentFromShips).ToNoSortList() ?? new();
        this.Spotting = bombers?.Select(x => x.MaxViewDistance).ToNoSortList() ?? new();
        this.AreaChangeAiming = bombers?.Select(x => x.AimingRateMoving).ToNoSortList() ?? new();
        this.AreaChangePreparation = bombers?.Select(x => x.AimingPreparationRateMoving).ToNoSortList() ?? new();
        this.InnerEllipse = bombers?.Select(x => x.InnerBombPercentage).ToNoSortList() ?? new();

        List<BombDataContainer?>? bombs = bombers?.Select(x => x.Weapon as BombDataContainer).ToNoSortList();

        this.WeaponType = bombers?.Select(x => x.WeaponType).ToNoSortList() ?? new();
        this.WeaponBombType = bombs?.Select(x => x?.BombType ?? default!).ToNoSortList() ?? new();
        this.WeaponDamage = bombs?.Select(x => x?.Damage ?? 0).ToNoSortList() ?? new();
        this.WeaponSplashRadius = bombs?.Select(x => x?.SplashRadius ?? 0).ToNoSortList() ?? new();
        this.WeaponSplashDamage = bombs?.Select(x => x?.SplashDmg ?? 0).ToNoSortList() ?? new();
        this.WeaponPenetration = bombs?.Select(x => (x?.BombType == $"ArmamentType_{BombType.AP}" ? x.PenetrationAp : x?.Penetration) ?? 0).ToNoSortList() ?? new();
        this.WeaponFireChance = bombs?.Select(x => x?.FireChance ?? 0).ToNoSortList() ?? new();
        this.WeaponBlastRadius = bombs?.Select(x => x?.ExplosionRadius ?? 0).ToNoSortList() ?? new();
        this.WeaponBlastPenetration = bombs?.Select(x => x?.SplashCoeff ?? 0).ToNoSortList() ?? new();
        this.WeaponFuseTimer = bombs?.Select(x => x?.FuseTimer ?? 0).ToNoSortList() ?? new();
        this.WeaponArmingThreshold = bombs?.Select(x => x?.ArmingThreshold ?? 0).ToNoSortList() ?? new();
        this.WeaponRicochetAngles = bombs?.Select(x => x?.RicochetAngles ?? default!).ToNoSortList() ?? new();
    }

    public NoSortList<int> InnerEllipse { get; }

    public NoSortList<string> WeaponType { get; }

    public NoSortList<string> WeaponBombType { get; }

    public NoSortList<decimal> WeaponDamage { get; }

    public NoSortList<decimal> WeaponSplashRadius { get; }

    public NoSortList<decimal> WeaponSplashDamage { get; }

    public NoSortList<int> WeaponPenetration { get; }

    public NoSortList<decimal> WeaponFireChance { get; }

    public NoSortList<decimal> WeaponBlastRadius { get; }

    public NoSortList<decimal> WeaponBlastPenetration { get; }

    public NoSortList<decimal> WeaponFuseTimer { get; }

    public NoSortList<int> WeaponArmingThreshold { get; }

    public NoSortList<string> WeaponRicochetAngles { get; }
}
