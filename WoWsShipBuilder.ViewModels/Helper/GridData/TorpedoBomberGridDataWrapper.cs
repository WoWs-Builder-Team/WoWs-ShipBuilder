using WoWsShipBuilder.Core.DataContainers;

namespace WoWsShipBuilder.ViewModels.Helper.GridData;

public class TorpedoBomberGridDataWrapper
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

        //Aerial torps
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

    //Aerial torps
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
