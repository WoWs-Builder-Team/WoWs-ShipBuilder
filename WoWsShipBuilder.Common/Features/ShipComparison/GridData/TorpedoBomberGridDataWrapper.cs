using System.Collections.Immutable;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.Features.DataContainers;

namespace WoWsShipBuilder.Features.ShipComparison.GridData;

public class TorpedoBomberGridDataWrapper : PlaneGridDataWrapper
{
    public TorpedoBomberGridDataWrapper(IReadOnlyCollection<CvAircraftDataContainer>? torpedoBombers)
    {
        this.Type = torpedoBombers?.Select(x => x.PlaneVariant).ToNoSortList() ?? new();
        this.InSquadron = torpedoBombers?.Select(x => x.NumberInSquad).ToNoSortList() ?? new();
        this.PerAttack = torpedoBombers?.Select(x => x.NumberDuringAttack).ToNoSortList() ?? new();
        this.OnDeck = torpedoBombers?.Select(x => x.MaxNumberOnDeck).ToNoSortList() ?? new();
        this.RestorationTime = torpedoBombers?.Select(x => x.RestorationTime).ToNoSortList() ?? new();
        this.CruisingSpeed = torpedoBombers?.Select(x => x.CruisingSpeed).ToNoSortList() ?? new();
        this.MaxSpeed = torpedoBombers?.Select(x => x.MaxSpeed).ToNoSortList() ?? new();
        this.MinSpeed = torpedoBombers?.Select(x => x.MinSpeed).ToNoSortList() ?? new();
        this.EngineBoostDuration = torpedoBombers?.Select(x => x.MaxEngineBoostDuration).ToNoSortList() ?? new();
        this.InitialBoostDuration = torpedoBombers?.Select(x => x.JatoDuration).ToNoSortList() ?? new();
        this.InitialBoostValue = torpedoBombers?.Select(x => x.JatoSpeedMultiplier).ToNoSortList() ?? new();
        this.PlaneHp = torpedoBombers?.Select(x => x.PlaneHp).ToNoSortList() ?? new();
        this.SquadronHp = torpedoBombers?.Select(x => x.SquadronHp).ToNoSortList() ?? new();
        this.AttackGroupHp = torpedoBombers?.Select(x => x.AttackGroupHp).ToNoSortList() ?? new();
        this.DamageDuringAttack = torpedoBombers?.Select(x => x.DamageTakenDuringAttack).ToNoSortList() ?? new();
        this.WeaponsPerPlane = torpedoBombers?.Select(x => x.AmmoPerAttack).ToNoSortList() ?? new();
        this.PreparationTime = torpedoBombers?.Select(x => x.PreparationTime).ToNoSortList() ?? new();
        this.AimingTime = torpedoBombers?.Select(x => x.AimingTime).ToNoSortList() ?? new();
        this.TimeToFullyAimed = torpedoBombers?.Select(x => x.TimeToFullyAimed).ToNoSortList() ?? new();
        this.PostAttackInvulnerability = torpedoBombers?.Select(x => x.PostAttackInvulnerabilityDuration).ToNoSortList() ?? new();
        this.AttackCooldown = torpedoBombers?.Select(x => x.AttackCd).ToNoSortList() ?? new();
        this.Concealment = torpedoBombers?.Select(x => x.ConcealmentFromShips).ToNoSortList() ?? new();
        this.Spotting = torpedoBombers?.Select(x => x.MaxViewDistance).ToNoSortList() ?? new();
        this.AreaChangeAiming = torpedoBombers?.Select(x => x.AimingRateMoving).ToNoSortList() ?? new();
        this.AreaChangePreparation = torpedoBombers?.Select(x => x.AimingPreparationRateMoving).ToNoSortList() ?? new();

        List<TorpedoDataContainer?>? aerialTorpedoes = torpedoBombers?.Select(x => x.Weapon as TorpedoDataContainer).ToNoSortList();

        this.WeaponType = aerialTorpedoes?.Select(x => x?.TorpedoType ?? default!).ToNoSortList() ?? new();
        this.WeaponDamage = aerialTorpedoes?.Select(x => x?.Damage ?? 0).ToNoSortList() ?? new();
        this.WeaponRange = aerialTorpedoes?.Select(x => x?.Range ?? 0).ToNoSortList() ?? new();
        this.WeaponSpeed = aerialTorpedoes?.Select(x => x?.Speed ?? 0).ToNoSortList() ?? new();
        this.WeaponDetectabilityRange = aerialTorpedoes?.Select(x => x?.Detectability ?? 0).ToNoSortList() ?? new();
        this.WeaponArmingDistance = aerialTorpedoes?.Select(x => x?.ArmingDistance ?? 0).ToNoSortList() ?? new();
        this.WeaponReactionTime = aerialTorpedoes?.Select(x => x?.ReactionTime ?? 0).ToNoSortList() ?? new();
        this.WeaponFloodingChance = aerialTorpedoes?.Select(x => x?.FloodingChance ?? 0).ToNoSortList() ?? new();
        this.WeaponBlastRadius = aerialTorpedoes?.Select(x => x?.ExplosionRadius ?? 0).ToNoSortList() ?? new();
        this.WeaponBlastPenetration = aerialTorpedoes?.Select(x => x?.SplashCoeff ?? 0).ToNoSortList() ?? new();
        this.WeaponCanHit = aerialTorpedoes?.Select(x => x?.CanHitClasses ?? ImmutableList<ShipClass>.Empty).ToNoSortList() ?? new();
    }

    public NoSortList<string> WeaponType { get; }

    public NoSortList<decimal> WeaponDamage { get; }

    public NoSortList<decimal> WeaponRange { get; }

    public NoSortList<decimal> WeaponSpeed { get; }

    public NoSortList<decimal> WeaponDetectabilityRange { get; }

    public NoSortList<int> WeaponArmingDistance { get; }

    public NoSortList<decimal> WeaponReactionTime { get; }

    public NoSortList<decimal> WeaponFloodingChance { get; }

    public NoSortList<decimal> WeaponBlastRadius { get; }

    public NoSortList<decimal> WeaponBlastPenetration { get; }

    public NoSortList<ImmutableList<ShipClass>> WeaponCanHit { get; }
}
