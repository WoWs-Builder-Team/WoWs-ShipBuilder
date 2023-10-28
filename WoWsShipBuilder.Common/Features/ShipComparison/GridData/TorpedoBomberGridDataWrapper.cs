using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.Features.DataContainers;

namespace WoWsShipBuilder.Features.ShipComparison.GridData;

public class TorpedoBomberGridDataWrapper : PlaneGridDataWrapper
{
    public TorpedoBomberGridDataWrapper(IReadOnlyCollection<CvAircraftDataContainer>? torpedoBombers)
    {
        Type = torpedoBombers?.Select(x => x.PlaneVariant).ToList() ?? new();
        InSquadron = torpedoBombers?.Select(x => x.NumberInSquad).ToList() ?? new();
        PerAttack = torpedoBombers?.Select(x => x.NumberDuringAttack).ToList() ?? new();
        OnDeck = torpedoBombers?.Select(x => x.MaxNumberOnDeck).ToList() ?? new();
        RestorationTime = torpedoBombers?.Select(x => x.RestorationTime).ToList() ?? new();
        CruisingSpeed = torpedoBombers?.Select(x => x.CruisingSpeed).ToList() ?? new();
        MaxSpeed = torpedoBombers?.Select(x => x.MaxSpeed).ToList() ?? new();
        MinSpeed = torpedoBombers?.Select(x => x.MinSpeed).ToList() ?? new();
        EngineBoostDuration = torpedoBombers?.Select(x => x.MaxEngineBoostDuration).ToList() ?? new();
        InitialBoostDuration = torpedoBombers?.Select(x => x.JatoDuration).ToList() ?? new();
        InitialBoostValue = torpedoBombers?.Select(x => x.JatoSpeedMultiplier).ToList() ?? new();
        PlaneHp = torpedoBombers?.Select(x => x.PlaneHp).ToList() ?? new();
        SquadronHp = torpedoBombers?.Select(x => x.SquadronHp).ToList() ?? new();
        AttackGroupHp = torpedoBombers?.Select(x => x.AttackGroupHp).ToList() ?? new();
        DamageDuringAttack = torpedoBombers?.Select(x => x.DamageTakenDuringAttack).ToList() ?? new();
        WeaponsPerPlane = torpedoBombers?.Select(x => x.AmmoPerAttack).ToList() ?? new();
        PreparationTime = torpedoBombers?.Select(x => x.PreparationTime).ToList() ?? new();
        AimingTime = torpedoBombers?.Select(x => x.AimingTime).ToList() ?? new();
        TimeToFullyAimed = torpedoBombers?.Select(x => x.TimeToFullyAimed).ToList() ?? new();
        PostAttackInvulnerability = torpedoBombers?.Select(x => x.PostAttackInvulnerabilityDuration).ToList() ?? new();
        AttackCooldown = torpedoBombers?.Select(x => x.AttackCd).ToList() ?? new();
        Concealment = torpedoBombers?.Select(x => x.ConcealmentFromShips).ToList() ?? new();
        Spotting = torpedoBombers?.Select(x => x.MaxViewDistance).ToList() ?? new();
        AreaChangeAiming = torpedoBombers?.Select(x => x.AimingRateMoving).ToList() ?? new();
        AreaChangePreparation = torpedoBombers?.Select(x => x.AimingPreparationRateMoving).ToList() ?? new();

        List<TorpedoDataContainer?>? aerialTorpedoes = torpedoBombers?.Select(x => x.Weapon as TorpedoDataContainer).ToList();

        WeaponType = aerialTorpedoes?.Select(x => x?.TorpedoType ?? default!).ToList() ?? new();
        WeaponDamage = aerialTorpedoes?.Select(x => x?.Damage ?? 0).ToList() ?? new();
        WeaponRange = aerialTorpedoes?.Select(x => x?.Range ?? 0).ToList() ?? new();
        WeaponSpeed = aerialTorpedoes?.Select(x => x?.Speed ?? 0).ToList() ?? new();
        WeaponDetectabilityRange = aerialTorpedoes?.Select(x => x?.Detectability ?? 0).ToList() ?? new();
        WeaponArmingDistance = aerialTorpedoes?.Select(x => x?.ArmingDistance ?? 0).ToList() ?? new();
        WeaponReactionTime = aerialTorpedoes?.Select(x => x?.ReactionTime ?? 0).ToList() ?? new();
        WeaponFloodingChance = aerialTorpedoes?.Select(x => x?.FloodingChance ?? 0).ToList() ?? new();
        WeaponBlastRadius = aerialTorpedoes?.Select(x => x?.ExplosionRadius ?? 0).ToList() ?? new();
        WeaponBlastPenetration = aerialTorpedoes?.Select(x => x?.SplashCoeff ?? 0).ToList() ?? new();
        WeaponCanHit = aerialTorpedoes?.Select(x => x?.CanHitClasses).ToList() ?? new();
    }

    public List<string> WeaponType { get; }

    public List<decimal> WeaponDamage { get; }

    public List<decimal> WeaponRange { get; }

    public List<decimal> WeaponSpeed { get; }

    public List<decimal> WeaponDetectabilityRange { get; }

    public List<int> WeaponArmingDistance { get; }

    public List<decimal> WeaponReactionTime { get; }

    public List<decimal> WeaponFloodingChance { get; }

    public List<decimal> WeaponBlastRadius { get; }

    public List<decimal> WeaponBlastPenetration { get; }

    public List<List<ShipClass>?> WeaponCanHit { get; }
}
