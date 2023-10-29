using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.Features.DataContainers;

namespace WoWsShipBuilder.Features.ShipComparison.GridData;

public class TorpedoBomberGridDataWrapper : PlaneGridDataWrapper
{
    public TorpedoBomberGridDataWrapper(IReadOnlyCollection<CvAircraftDataContainer>? torpedoBombers)
    {
        this.Type = torpedoBombers?.Select(x => x.PlaneVariant).ToList() ?? new();
        this.InSquadron = torpedoBombers?.Select(x => x.NumberInSquad).ToList() ?? new();
        this.PerAttack = torpedoBombers?.Select(x => x.NumberDuringAttack).ToList() ?? new();
        this.OnDeck = torpedoBombers?.Select(x => x.MaxNumberOnDeck).ToList() ?? new();
        this.RestorationTime = torpedoBombers?.Select(x => x.RestorationTime).ToList() ?? new();
        this.CruisingSpeed = torpedoBombers?.Select(x => x.CruisingSpeed).ToList() ?? new();
        this.MaxSpeed = torpedoBombers?.Select(x => x.MaxSpeed).ToList() ?? new();
        this.MinSpeed = torpedoBombers?.Select(x => x.MinSpeed).ToList() ?? new();
        this.EngineBoostDuration = torpedoBombers?.Select(x => x.MaxEngineBoostDuration).ToList() ?? new();
        this.InitialBoostDuration = torpedoBombers?.Select(x => x.JatoDuration).ToList() ?? new();
        this.InitialBoostValue = torpedoBombers?.Select(x => x.JatoSpeedMultiplier).ToList() ?? new();
        this.PlaneHp = torpedoBombers?.Select(x => x.PlaneHp).ToList() ?? new();
        this.SquadronHp = torpedoBombers?.Select(x => x.SquadronHp).ToList() ?? new();
        this.AttackGroupHp = torpedoBombers?.Select(x => x.AttackGroupHp).ToList() ?? new();
        this.DamageDuringAttack = torpedoBombers?.Select(x => x.DamageTakenDuringAttack).ToList() ?? new();
        this.WeaponsPerPlane = torpedoBombers?.Select(x => x.AmmoPerAttack).ToList() ?? new();
        this.PreparationTime = torpedoBombers?.Select(x => x.PreparationTime).ToList() ?? new();
        this.AimingTime = torpedoBombers?.Select(x => x.AimingTime).ToList() ?? new();
        this.TimeToFullyAimed = torpedoBombers?.Select(x => x.TimeToFullyAimed).ToList() ?? new();
        this.PostAttackInvulnerability = torpedoBombers?.Select(x => x.PostAttackInvulnerabilityDuration).ToList() ?? new();
        this.AttackCooldown = torpedoBombers?.Select(x => x.AttackCd).ToList() ?? new();
        this.Concealment = torpedoBombers?.Select(x => x.ConcealmentFromShips).ToList() ?? new();
        this.Spotting = torpedoBombers?.Select(x => x.MaxViewDistance).ToList() ?? new();
        this.AreaChangeAiming = torpedoBombers?.Select(x => x.AimingRateMoving).ToList() ?? new();
        this.AreaChangePreparation = torpedoBombers?.Select(x => x.AimingPreparationRateMoving).ToList() ?? new();

        List<TorpedoDataContainer?>? aerialTorpedoes = torpedoBombers?.Select(x => x.Weapon as TorpedoDataContainer).ToList();

        this.WeaponType = aerialTorpedoes?.Select(x => x?.TorpedoType ?? default!).ToList() ?? new();
        this.WeaponDamage = aerialTorpedoes?.Select(x => x?.Damage ?? 0).ToList() ?? new();
        this.WeaponRange = aerialTorpedoes?.Select(x => x?.Range ?? 0).ToList() ?? new();
        this.WeaponSpeed = aerialTorpedoes?.Select(x => x?.Speed ?? 0).ToList() ?? new();
        this.WeaponDetectabilityRange = aerialTorpedoes?.Select(x => x?.Detectability ?? 0).ToList() ?? new();
        this.WeaponArmingDistance = aerialTorpedoes?.Select(x => x?.ArmingDistance ?? 0).ToList() ?? new();
        this.WeaponReactionTime = aerialTorpedoes?.Select(x => x?.ReactionTime ?? 0).ToList() ?? new();
        this.WeaponFloodingChance = aerialTorpedoes?.Select(x => x?.FloodingChance ?? 0).ToList() ?? new();
        this.WeaponBlastRadius = aerialTorpedoes?.Select(x => x?.ExplosionRadius ?? 0).ToList() ?? new();
        this.WeaponBlastPenetration = aerialTorpedoes?.Select(x => x?.SplashCoeff ?? 0).ToList() ?? new();
        this.WeaponCanHit = aerialTorpedoes?.Select(x => x?.CanHitClasses).ToList() ?? new();
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
