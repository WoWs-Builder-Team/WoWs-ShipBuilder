using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.Features.DataContainers;

namespace WoWsShipBuilder.Features.ShipComparison.GridData;

public class RocketPlaneGridDataWrapper : PlaneGridDataWrapper
{
    public RocketPlaneGridDataWrapper(IReadOnlyCollection<CvAircraftDataContainer>? rocketPlanes)
    {
        this.Type = rocketPlanes?.Select(x => x.PlaneVariant).ToNoSortList() ?? new();
        this.InSquadron = rocketPlanes?.Select(x => x.NumberInSquad).ToNoSortList() ?? new();
        this.PerAttack = rocketPlanes?.Select(x => x.NumberDuringAttack).ToNoSortList() ?? new();
        this.OnDeck = rocketPlanes?.Select(x => x.MaxNumberOnDeck).ToNoSortList() ?? new();
        this.RestorationTime = rocketPlanes?.Select(x => x.RestorationTime).ToNoSortList() ?? new();
        this.CruisingSpeed = rocketPlanes?.Select(x => x.CruisingSpeed).ToNoSortList() ?? new();
        this.MaxSpeed = rocketPlanes?.Select(x => x.MaxSpeed).ToNoSortList() ?? new();
        this.MinSpeed = rocketPlanes?.Select(x => x.MinSpeed).ToNoSortList() ?? new();
        this.EngineBoostDuration = rocketPlanes?.Select(x => x.MaxEngineBoostDuration).ToNoSortList() ?? new();
        this.InitialBoostDuration = rocketPlanes?.Select(x => x.JatoDuration).ToNoSortList() ?? new();
        this.InitialBoostValue = rocketPlanes?.Select(x => x.JatoSpeedMultiplier).ToNoSortList() ?? new();
        this.PlaneHp = rocketPlanes?.Select(x => x.PlaneHp).ToNoSortList() ?? new();
        this.SquadronHp = rocketPlanes?.Select(x => x.SquadronHp).ToNoSortList() ?? new();
        this.AttackGroupHp = rocketPlanes?.Select(x => x.AttackGroupHp).ToNoSortList() ?? new();
        this.DamageDuringAttack = rocketPlanes?.Select(x => x.DamageTakenDuringAttack).ToNoSortList() ?? new();
        this.WeaponsPerPlane = rocketPlanes?.Select(x => x.AmmoPerAttack).ToNoSortList() ?? new();
        this.PreparationTime = rocketPlanes?.Select(x => x.PreparationTime).ToNoSortList() ?? new();
        this.AimingTime = rocketPlanes?.Select(x => x.AimingTime).ToNoSortList() ?? new();
        this.TimeToFullyAimed = rocketPlanes?.Select(x => x.TimeToFullyAimed).ToNoSortList() ?? new();
        this.PostAttackInvulnerability = rocketPlanes?.Select(x => x.PostAttackInvulnerabilityDuration).ToNoSortList() ?? new();
        this.AttackCooldown = rocketPlanes?.Select(x => x.AttackCd).ToNoSortList() ?? new();
        this.Concealment = rocketPlanes?.Select(x => x.ConcealmentFromShips).ToNoSortList() ?? new();
        this.Spotting = rocketPlanes?.Select(x => x.MaxViewDistance).ToNoSortList() ?? new();
        this.AreaChangeAiming = rocketPlanes?.Select(x => x.AimingRateMoving).ToNoSortList() ?? new();
        this.AreaChangePreparation = rocketPlanes?.Select(x => x.AimingPreparationRateMoving).ToNoSortList() ?? new();

        // Rockets
        List<RocketDataContainer?>? rockets = rocketPlanes?.Select(x => x.Weapon as RocketDataContainer).ToNoSortList();

        this.WeaponType = rockets?.Select(x => x?.RocketType ?? default!).ToNoSortList() ?? new();
        this.WeaponDamage = rockets?.Select(x => x?.Damage ?? 0).ToNoSortList() ?? new();
        this.WeaponSplashRadius = rockets?.Select(x => x?.SplashRadius ?? 0).ToNoSortList() ?? new();
        this.WeaponSplashDamage = rockets?.Select(x => x?.SplashDmg ?? 0).ToNoSortList() ?? new();
        this.WeaponPenetration = rockets?.Select(x => (x?.RocketType == $"ArmamentType_{RocketType.AP}" ? x.PenetrationAp : x?.Penetration) ?? 0).ToNoSortList() ?? new();
        this.WeaponFireChance = rockets?.Select(x => x?.FireChance ?? 0).ToNoSortList() ?? new();
        this.WeaponFuseTimer = rockets?.Select(x => x?.FuseTimer ?? 0).ToNoSortList() ?? new();
        this.WeaponArmingThreshold = rockets?.Select(x => x?.ArmingThreshold ?? 0).ToNoSortList() ?? new();
        this.WeaponRicochetAngles = rockets?.Select(x => x?.RicochetAngles ?? default!).ToNoSortList() ?? new();
        this.WeaponBlastRadius = rockets?.Select(x => x?.ExplosionRadius ?? 0).ToNoSortList() ?? new();
        this.WeaponBlastPenetration = rockets?.Select(x => x?.SplashCoeff ?? 0).ToNoSortList() ?? new();
    }

    public NoSortList<string> WeaponType { get; }

    public NoSortList<decimal> WeaponDamage { get; }

    public NoSortList<decimal> WeaponSplashRadius { get; }

    public NoSortList<decimal> WeaponSplashDamage { get; }

    public NoSortList<int> WeaponPenetration { get; }

    public NoSortList<decimal> WeaponFireChance { get; }

    public NoSortList<decimal> WeaponFuseTimer { get; }

    public NoSortList<int> WeaponArmingThreshold { get; }

    public NoSortList<string> WeaponRicochetAngles { get; }

    public NoSortList<decimal> WeaponBlastRadius { get; }

    public NoSortList<decimal> WeaponBlastPenetration { get; }
}
