using WoWsShipBuilder.Core.DataContainers;
using WoWsShipBuilder.Core.Extensions;

namespace WoWsShipBuilder.ViewModels.Helper.GridData;

public class RocketPlaneGridDataWrapper
{
    public RocketPlaneGridDataWrapper(IReadOnlyCollection<CvAircraftDataContainer>? rocketPlanes)
    {
        Type = rocketPlanes?.Select(x => x.PlaneVariant).ToList() ?? new();
        InSquadron = rocketPlanes?.Select(x => x.NumberInSquad).ToList() ?? new();
        PerAttack = rocketPlanes?.Select(x => x.NumberDuringAttack).ToList() ?? new();
        OnDeck = rocketPlanes?.Select(x => x.MaxNumberOnDeck).ToList() ?? new();
        RestorationTime = rocketPlanes?.Select(x => x.RestorationTime).ToList() ?? new();
        CruisingSpeed = rocketPlanes?.Select(x => x.CruisingSpeed).ToList() ?? new();
        MaxSpeed = rocketPlanes?.Select(x => x.MaxSpeed).ToList() ?? new();
        MinSpeed = rocketPlanes?.Select(x => x.MinSpeed).ToList() ?? new();
        EngineBoostDuration = rocketPlanes?.Select(x => x.MaxEngineBoostDuration).ToList() ?? new();
        InitialBoostDuration = rocketPlanes?.Select(x => x.JatoDuration).ToList() ?? new();
        InitialBoostValue = rocketPlanes?.Select(x => x.JatoSpeedMultiplier).ToList() ?? new();
        PlaneHp = rocketPlanes?.Select(x => x.PlaneHp).ToList() ?? new();
        SquadronHp = rocketPlanes?.Select(x => x.SquadronHp).ToList() ?? new();
        AttackGroupHp = rocketPlanes?.Select(x => x.AttackGroupHp).ToList() ?? new();
        DamageDuringAttack = rocketPlanes?.Select(x => x.DamageTakenDuringAttack).ToList() ?? new();
        WeaponsPerPlane = rocketPlanes?.Select(x => x.AmmoPerAttack).ToList() ?? new();
        PreparationTime = rocketPlanes?.Select(x => x.PreparationTime).ToList() ?? new();
        AimingTime = rocketPlanes?.Select(x => x.AimingTime).ToList() ?? new();
        TimeToFullyAimed = rocketPlanes?.Select(x => x.TimeToFullyAimed).ToList() ?? new();
        PostAttackInvulnerability = rocketPlanes?.Select(x => x.PostAttackInvulnerabilityDuration).ToList() ?? new();
        AttackCooldown = rocketPlanes?.Select(x => x.AttackCd).ToList() ?? new();
        Concealment = rocketPlanes?.Select(x => x.ConcealmentFromShips).ToList() ?? new();
        Spotting = rocketPlanes?.Select(x => x.MaxViewDistance).ToList() ?? new();
        AreaChangeAiming = rocketPlanes?.Select(x => x.AimingRateMoving).ToList() ?? new();
        AreaChangePreparation = rocketPlanes?.Select(x => x.AimingPreparationRateMoving).ToList() ?? new();

        //Rockets
        List<RocketDataContainer?>? rockets = rocketPlanes?.Select(x => x.Weapon as RocketDataContainer).ToList();

        WeaponType = rockets?.Select(x => x?.RocketType ?? default!).ToList() ?? new();
        WeaponDamage = rockets?.Select(x => x?.Damage ?? 0).ToList() ?? new();
        WeaponSplashRadius = rockets?.Select(x => x?.SplashRadius ?? 0).ToList() ?? new();
        WeaponSplashDamage = rockets?.Select(x => x?.SplashDmg ?? 0).ToList() ?? new();
        WeaponPenetration = rockets?.Select(x => x?.Penetration ?? 0).ToList() ?? new();
        WeaponFireChance = rockets?.Select(x => x?.FireChance ?? 0).ToList() ?? new();
        WeaponFuseTimer = rockets?.Select(x => x?.FuseTimer ?? 0).ToList() ?? new();
        WeaponArmingThreshold = rockets?.Select(x => x?.ArmingThreshold ?? 0).ToList() ?? new();
        WeaponRicochetAngles = rockets?.Select(x => x?.RicochetAngles ?? default!).ToList() ?? new();
        WeaponBlastRadius = rockets?.Select(x => x?.ExplosionRadius ?? 0).ToList() ?? new();
        WeaponBlastPenetration = rockets?.Select(x => x?.SplashCoeff ?? 0).ToList() ?? new();
    }

    public List<string> Type { get; }

    public List<int> InSquadron { get; }

    public List<int> PerAttack { get; }

    public List<int> OnDeck { get; }

    public List<decimal> RestorationTime { get; }

    public List<decimal> CruisingSpeed { get; }

    public List<decimal> MaxSpeed { get; }

    public List<decimal> MinSpeed { get; }

    public List<decimal> EngineBoostDuration { get; }

    public List<decimal> InitialBoostDuration { get; }

    public List<decimal> InitialBoostValue { get; }

    public List<int> PlaneHp { get; }

    public List<int> SquadronHp { get; }

    public List<int> AttackGroupHp { get; }

    public List<int> DamageDuringAttack { get; }

    public List<int> WeaponsPerPlane { get; }

    public List<decimal> PreparationTime { get; }

    public List<decimal> AimingTime { get; }

    public List<decimal> TimeToFullyAimed { get; }

    public List<decimal> PostAttackInvulnerability { get; }

    public List<decimal> AttackCooldown { get; }

    public List<decimal> Concealment { get; }

    public List<decimal> Spotting { get; }

    public List<string> AreaChangeAiming { get; }

    public List<string> AreaChangePreparation { get; }

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
