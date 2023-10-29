using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.Features.DataContainers;

namespace WoWsShipBuilder.Features.ShipComparison.GridData;

public class RocketPlaneGridDataWrapper : PlaneGridDataWrapper
{
    public RocketPlaneGridDataWrapper(IReadOnlyCollection<CvAircraftDataContainer>? rocketPlanes)
    {
        this.Type = rocketPlanes?.Select(x => x.PlaneVariant).ToList() ?? new();
        this.InSquadron = rocketPlanes?.Select(x => x.NumberInSquad).ToList() ?? new();
        this.PerAttack = rocketPlanes?.Select(x => x.NumberDuringAttack).ToList() ?? new();
        this.OnDeck = rocketPlanes?.Select(x => x.MaxNumberOnDeck).ToList() ?? new();
        this.RestorationTime = rocketPlanes?.Select(x => x.RestorationTime).ToList() ?? new();
        this.CruisingSpeed = rocketPlanes?.Select(x => x.CruisingSpeed).ToList() ?? new();
        this.MaxSpeed = rocketPlanes?.Select(x => x.MaxSpeed).ToList() ?? new();
        this.MinSpeed = rocketPlanes?.Select(x => x.MinSpeed).ToList() ?? new();
        this.EngineBoostDuration = rocketPlanes?.Select(x => x.MaxEngineBoostDuration).ToList() ?? new();
        this.InitialBoostDuration = rocketPlanes?.Select(x => x.JatoDuration).ToList() ?? new();
        this.InitialBoostValue = rocketPlanes?.Select(x => x.JatoSpeedMultiplier).ToList() ?? new();
        this.PlaneHp = rocketPlanes?.Select(x => x.PlaneHp).ToList() ?? new();
        this.SquadronHp = rocketPlanes?.Select(x => x.SquadronHp).ToList() ?? new();
        this.AttackGroupHp = rocketPlanes?.Select(x => x.AttackGroupHp).ToList() ?? new();
        this.DamageDuringAttack = rocketPlanes?.Select(x => x.DamageTakenDuringAttack).ToList() ?? new();
        this.WeaponsPerPlane = rocketPlanes?.Select(x => x.AmmoPerAttack).ToList() ?? new();
        this.PreparationTime = rocketPlanes?.Select(x => x.PreparationTime).ToList() ?? new();
        this.AimingTime = rocketPlanes?.Select(x => x.AimingTime).ToList() ?? new();
        this.TimeToFullyAimed = rocketPlanes?.Select(x => x.TimeToFullyAimed).ToList() ?? new();
        this.PostAttackInvulnerability = rocketPlanes?.Select(x => x.PostAttackInvulnerabilityDuration).ToList() ?? new();
        this.AttackCooldown = rocketPlanes?.Select(x => x.AttackCd).ToList() ?? new();
        this.Concealment = rocketPlanes?.Select(x => x.ConcealmentFromShips).ToList() ?? new();
        this.Spotting = rocketPlanes?.Select(x => x.MaxViewDistance).ToList() ?? new();
        this.AreaChangeAiming = rocketPlanes?.Select(x => x.AimingRateMoving).ToList() ?? new();
        this.AreaChangePreparation = rocketPlanes?.Select(x => x.AimingPreparationRateMoving).ToList() ?? new();

        // Rockets
        List<RocketDataContainer?>? rockets = rocketPlanes?.Select(x => x.Weapon as RocketDataContainer).ToList();

        this.WeaponType = rockets?.Select(x => x?.RocketType ?? default!).ToList() ?? new();
        this.WeaponDamage = rockets?.Select(x => x?.Damage ?? 0).ToList() ?? new();
        this.WeaponSplashRadius = rockets?.Select(x => x?.SplashRadius ?? 0).ToList() ?? new();
        this.WeaponSplashDamage = rockets?.Select(x => x?.SplashDmg ?? 0).ToList() ?? new();
        this.WeaponPenetration = rockets?.Select(x => (x?.RocketType == $"ArmamentType_{RocketType.AP}" ? x.PenetrationAp : x?.Penetration) ?? 0).ToList() ?? new();
        this.WeaponFireChance = rockets?.Select(x => x?.FireChance ?? 0).ToList() ?? new();
        this.WeaponFuseTimer = rockets?.Select(x => x?.FuseTimer ?? 0).ToList() ?? new();
        this.WeaponArmingThreshold = rockets?.Select(x => x?.ArmingThreshold ?? 0).ToList() ?? new();
        this.WeaponRicochetAngles = rockets?.Select(x => x?.RicochetAngles ?? default!).ToList() ?? new();
        this.WeaponBlastRadius = rockets?.Select(x => x?.ExplosionRadius ?? 0).ToList() ?? new();
        this.WeaponBlastPenetration = rockets?.Select(x => x?.SplashCoeff ?? 0).ToList() ?? new();
    }

    public List<string> WeaponType { get; }

    public List<decimal> WeaponDamage { get; }

    public List<decimal> WeaponSplashRadius { get; }

    public List<decimal> WeaponSplashDamage { get; }

    public List<int> WeaponPenetration { get; }

    public List<decimal> WeaponFireChance { get; }

    public List<decimal> WeaponFuseTimer { get; }

    public List<int> WeaponArmingThreshold { get; }

    public List<string> WeaponRicochetAngles { get; }

    public List<decimal> WeaponBlastRadius { get; }

    public List<decimal> WeaponBlastPenetration { get; }
}
