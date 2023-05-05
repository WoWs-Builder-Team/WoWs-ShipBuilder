using System;
using System.IO;
using WoWsShipBuilder.Core.Builds;
using WoWsShipBuilder.Core.Data;
using WoWsShipBuilder.Core.DataContainers;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.ViewModels.Helper.GridData;

namespace WoWsShipBuilder.ViewModels.Helper;

public sealed class GridDataWrapper
{
    public ShipBuildContainer GetShipBuildContainer()
    {
        return shipBuildContainer;
    }

    public GridDataWrapper(ShipBuildContainer shipBuildContainer)
    {
        this.shipBuildContainer = shipBuildContainer;

        Ship = shipBuildContainer.Ship;
        ShipDataContainer = shipBuildContainer.ShipDataContainer ?? throw new InvalidDataException("ShipDataContainer is null. ShipDataContainer must not be null.");
        Build = shipBuildContainer.Build;
        Id = shipBuildContainer.Id;

        //base
        ShipIndex = shipBuildContainer.Ship.Index;
        ShipNation = shipBuildContainer.Ship.ShipNation;
        ShipClass = shipBuildContainer.Ship.ShipClass;
        ShipCategory = shipBuildContainer.Ship.ShipCategory;
        ShipTier = shipBuildContainer.Ship.Tier;
        BuildName = shipBuildContainer.Build?.BuildName;

        //Main battery
        MainBattery = this.shipBuildContainer.ShipDataContainer?.MainBatteryDataContainer;
        var mainBattery = shipBuildContainer.ShipDataContainer.MainBatteryDataContainer;

        //HE shells
        HeShell = mainBattery?.ShellData.FirstOrDefault(x => x.Type.Equals($"ArmamentType_{ShellType.HE.ShellTypeToString()}"));

        //AP shells
        ApShell = mainBattery?.ShellData.FirstOrDefault(x => x.Type.Equals($"ArmamentType_{ShellType.AP.ShellTypeToString()}"));

        //SAP shells
        SapShell = mainBattery?.ShellData.FirstOrDefault(x => x.Type.Equals($"ArmamentType_{ShellType.SAP.ShellTypeToString()}"));

        //TorpLaunchers
        var torpedoArmament = shipBuildContainer.ShipDataContainer.TorpedoArmamentDataContainer;
        TorpedoLauncher = torpedoArmament;

        //Torpedoes
        List<TorpedoDataContainer>? torpedoes = torpedoArmament?.Torpedoes;

        TorpedoFullSalvoDamage = new() { torpedoArmament?.FullSalvoDamage, torpedoArmament?.TorpFullSalvoDmg, torpedoArmament?.AltTorpFullSalvoDmg };
        TorpedoType = torpedoes?.Select(x => x.TorpedoType).ToList() ?? new();
        TorpedoDamage = torpedoes?.Select(x => x.Damage).ToList() ?? new();
        TorpedoRange = torpedoes?.Select(x => x.Range).ToList() ?? new();
        TorpedoSpeed = torpedoes?.Select(x => x.Speed).ToList() ?? new();
        TorpedoDetectRange = torpedoes?.Select(x => x.Detectability).ToList() ?? new();
        TorpedoArmingDistance = torpedoes?.Select(x => x.ArmingDistance).ToList() ?? new();
        TorpedoReactionTime = torpedoes?.Select(x => x.ReactionTime).ToList() ?? new();
        TorpedoFloodingChance = torpedoes?.Select(x => x.FloodingChance).ToList() ?? new();
        TorpedoBlastRadius = torpedoes?.Select(x => x.ExplosionRadius).ToList() ?? new();
        TorpedoBlastPenetration = torpedoes?.Select(x => x.SplashCoeff).ToList() ?? new();
        TorpedoCanHit = torpedoes?.Select(x => x.CanHitClasses).ToList() ?? new();

        //Secondaries
        Secondary = new(shipBuildContainer.ShipDataContainer.SecondaryBatteryUiDataContainer.Secondaries);

        //AA
        AntiAirArmament = shipBuildContainer.ShipDataContainer.AntiAirDataContainer;

        //ASW
        var aswAirStrike = shipBuildContainer.ShipDataContainer.AswAirstrikeDataContainer;
        var depthChargeLauncher = shipBuildContainer.ShipDataContainer.DepthChargeLauncherDataContainer;
        Asw = new(aswAirStrike, depthChargeLauncher);

        //AirStrike
        AirStrike = shipBuildContainer.ShipDataContainer.AirstrikeDataContainer;
        AirStrikeWeapon = AirStrike?.Weapon as BombDataContainer;

        //Maneuverability
        var maneuverability = shipBuildContainer.ShipDataContainer.ManeuverabilityDataContainer;
        ManeuverabilityData = new(maneuverability);

        //Concealment
        Concealment = shipBuildContainer.ShipDataContainer.ConcealmentDataContainer;

        //Survivability
        Survivability = shipBuildContainer.ShipDataContainer.SurvivabilityDataContainer;

        //Sonar
        Sonar = shipBuildContainer.ShipDataContainer.PingerGunDataContainer;

        //RocketPlanes
        List<CvAircraftDataContainer>? rocketPlanes = shipBuildContainer.ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Rocket.ProjectileTypeToString())).ToList();

        RocketPlanesType = rocketPlanes?.Select(x => x.PlaneVariant).ToList() ?? new();
        RocketPlanesInSquadron = rocketPlanes?.Select(x => x.NumberInSquad).ToList() ?? new();
        RocketPlanesPerAttack = rocketPlanes?.Select(x => x.NumberDuringAttack).ToList() ?? new();
        RocketPlanesOnDeck = rocketPlanes?.Select(x => x.MaxNumberOnDeck).ToList() ?? new();
        RocketPlanesRestorationTime = rocketPlanes?.Select(x => x.RestorationTime).ToList() ?? new();
        RocketPlanesCruisingSpeed = rocketPlanes?.Select(x => x.CruisingSpeed).ToList() ?? new();
        RocketPlanesMaxSpeed = rocketPlanes?.Select(x => x.MaxSpeed).ToList() ?? new();
        RocketPlanesMinSpeed = rocketPlanes?.Select(x => x.MinSpeed).ToList() ?? new();
        RocketPlanesEngineBoostDuration = rocketPlanes?.Select(x => x.MaxEngineBoostDuration).ToList() ?? new();
        RocketPlanesInitialBoostDuration = rocketPlanes?.Select(x => x.JatoDuration).ToList() ?? new();
        RocketPlanesInitialBoostValue = rocketPlanes?.Select(x => x.JatoSpeedMultiplier).ToList() ?? new();
        RocketPlanesPlaneHp = rocketPlanes?.Select(x => x.PlaneHp).ToList() ?? new();
        RocketPlanesSquadronHp = rocketPlanes?.Select(x => x.SquadronHp).ToList() ?? new();
        RocketPlanesAttackGroupHp = rocketPlanes?.Select(x => x.AttackGroupHp).ToList() ?? new();
        RocketPlanesDamageDuringAttack = rocketPlanes?.Select(x => x.DamageTakenDuringAttack).ToList() ?? new();
        RocketPlanesWeaponsPerPlane = rocketPlanes?.Select(x => x.AmmoPerAttack).ToList() ?? new();
        RocketPlanesPreparationTime = rocketPlanes?.Select(x => x.PreparationTime).ToList() ?? new();
        RocketPlanesAimingTime = rocketPlanes?.Select(x => x.AimingTime).ToList() ?? new();
        RocketPlanesTimeToFullyAimed = rocketPlanes?.Select(x => x.TimeToFullyAimed).ToList() ?? new();
        RocketPlanesPostAttackInvulnerability = rocketPlanes?.Select(x => x.PostAttackInvulnerabilityDuration).ToList() ?? new();
        RocketPlanesAttackCooldown = rocketPlanes?.Select(x => x.AttackCd).ToList() ?? new();
        RocketPlanesConcealment = rocketPlanes?.Select(x => x.ConcealmentFromShips).ToList() ?? new();
        RocketPlanesSpotting = rocketPlanes?.Select(x => x.MaxViewDistance).ToList() ?? new();
        RocketPlanesAreaChangeAiming = rocketPlanes?.Select(x => x.AimingRateMoving).ToList() ?? new();
        RocketPlanesAreaChangePreparation = rocketPlanes?.Select(x => x.AimingPreparationRateMoving).ToList() ?? new();

        //Rockets
        List<RocketDataContainer?>? rockets = rocketPlanes?.Select(x => x.Weapon as RocketDataContainer).ToList();

        RocketPlanesWeaponType = rockets?.Select(x => x?.RocketType ?? default!).ToList() ?? new();
        RocketPlanesWeaponDamage = rockets?.Select(x => x?.Damage ?? 0).ToList() ?? new();
        RocketPlanesWeaponSplashRadius = rockets?.Select(x => x?.SplashRadius ?? 0).ToList() ?? new();
        RocketPlanesWeaponSplashDamage = rockets?.Select(x => x?.SplashDmg ?? 0).ToList() ?? new();
        RocketPlanesWeaponPenetration = rockets?.Select(x => x?.Penetration ?? 0).ToList() ?? new();
        RocketPlanesWeaponFireChance = rockets?.Select(x => x?.FireChance ?? 0).ToList() ?? new();
        RocketPlanesWeaponFuseTimer = rockets?.Select(x => x?.FuseTimer ?? 0).ToList() ?? new();
        RocketPlanesWeaponArmingThreshold = rockets?.Select(x => x?.ArmingThreshold ?? 0).ToList() ?? new();
        RocketPlanesWeaponRicochetAngles = rockets?.Select(x => x?.RicochetAngles ?? default!).ToList() ?? new();
        RocketPlanesWeaponBlastRadius = rockets?.Select(x => x?.ExplosionRadius ?? 0).ToList() ?? new();
        RocketPlanesWeaponBlastPenetration = rockets?.Select(x => x?.SplashCoeff ?? 0).ToList() ?? new();

        //TorpedoBombers
        List<CvAircraftDataContainer>? torpedoBombers = shipBuildContainer.ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Torpedo.ProjectileTypeToString())).ToList();

        TorpedoBombersType = torpedoBombers?.Select(x => x.PlaneVariant).ToList() ?? new();
        TorpedoBombersInSquadron = torpedoBombers?.Select(x => x.NumberInSquad).ToList() ?? new();
        TorpedoBombersPerAttack = torpedoBombers?.Select(x => x.NumberDuringAttack).ToList() ?? new();
        TorpedoBombersOnDeck = torpedoBombers?.Select(x => x.MaxNumberOnDeck).ToList() ?? new();
        TorpedoBombersRestorationTime = torpedoBombers?.Select(x => x.RestorationTime).ToList() ?? new();
        TorpedoBombersCruisingSpeed = torpedoBombers?.Select(x => x.CruisingSpeed).ToList() ?? new();
        TorpedoBombersMaxSpeed = torpedoBombers?.Select(x => x.MaxSpeed).ToList() ?? new();
        TorpedoBombersMinSpeed = torpedoBombers?.Select(x => x.MinSpeed).ToList() ?? new();
        TorpedoBombersEngineBoostDuration = torpedoBombers?.Select(x => x.MaxEngineBoostDuration).ToList() ?? new();
        TorpedoBombersInitialBoostDuration = torpedoBombers?.Select(x => x.JatoDuration).ToList() ?? new();
        TorpedoBombersInitialBoostValue = torpedoBombers?.Select(x => x.JatoSpeedMultiplier).ToList() ?? new();
        TorpedoBombersPlaneHp = torpedoBombers?.Select(x => x.PlaneHp).ToList() ?? new();
        TorpedoBombersSquadronHp = torpedoBombers?.Select(x => x.SquadronHp).ToList() ?? new();
        TorpedoBombersAttackGroupHp = torpedoBombers?.Select(x => x.AttackGroupHp).ToList() ?? new();
        TorpedoBombersDamageDuringAttack = torpedoBombers?.Select(x => x.DamageTakenDuringAttack).ToList() ?? new();
        TorpedoBombersWeaponsPerPlane = torpedoBombers?.Select(x => x.AmmoPerAttack).ToList() ?? new();
        TorpedoBombersPreparationTime = torpedoBombers?.Select(x => x.PreparationTime).ToList() ?? new();
        TorpedoBombersAimingTime = torpedoBombers?.Select(x => x.AimingTime).ToList() ?? new();
        TorpedoBombersTimeToFullyAimed = torpedoBombers?.Select(x => x.TimeToFullyAimed).ToList() ?? new();
        TorpedoBombersPostAttackInvulnerability = torpedoBombers?.Select(x => x.PostAttackInvulnerabilityDuration).ToList() ?? new();
        TorpedoBombersAttackCooldown = torpedoBombers?.Select(x => x.AttackCd).ToList() ?? new();
        TorpedoBombersConcealment = torpedoBombers?.Select(x => x.ConcealmentFromShips).ToList() ?? new();
        TorpedoBombersSpotting = torpedoBombers?.Select(x => x.MaxViewDistance).ToList() ?? new();
        TorpedoBombersAreaChangeAiming = torpedoBombers?.Select(x => x.AimingRateMoving).ToList() ?? new();
        TorpedoBombersAreaChangePreparation = torpedoBombers?.Select(x => x.AimingPreparationRateMoving).ToList() ?? new();

        //Aerial torps
        List<TorpedoDataContainer?>? aerialTorpedoes = torpedoBombers?.Select(x => x.Weapon as TorpedoDataContainer).ToList();

        TorpedoBombersWeaponType = aerialTorpedoes?.Select(x => x?.TorpedoType ?? default!).ToList() ?? new();
        TorpedoBombersWeaponDamage = aerialTorpedoes?.Select(x => x?.Damage ?? 0).ToList() ?? new();
        TorpedoBombersWeaponRange = aerialTorpedoes?.Select(x => x?.Range ?? 0).ToList() ?? new();
        TorpedoBombersWeaponSpeed = aerialTorpedoes?.Select(x => x?.Speed ?? 0).ToList() ?? new();
        TorpedoBombersWeaponDetectabilityRange = aerialTorpedoes?.Select(x => x?.Detectability ?? 0).ToList() ?? new();
        TorpedoBombersWeaponArmingDistance = aerialTorpedoes?.Select(x => x?.ArmingDistance ?? 0).ToList() ?? new();
        TorpedoBombersWeaponReactionTime = aerialTorpedoes?.Select(x => x?.ReactionTime ?? 0).ToList() ?? new();
        TorpedoBombersWeaponFloodingChance = aerialTorpedoes?.Select(x => x?.FloodingChance ?? 0).ToList() ?? new();
        TorpedoBombersWeaponBlastRadius = aerialTorpedoes?.Select(x => x?.ExplosionRadius ?? 0).ToList() ?? new();
        TorpedoBombersWeaponBlastPenetration = aerialTorpedoes?.Select(x => x?.SplashCoeff ?? 0).ToList() ?? new();
        TorpedoBombersWeaponCanHit = aerialTorpedoes?.Select(x => x?.CanHitClasses).ToList() ?? new();

        //Bombers
        List<CvAircraftDataContainer>? bombers = shipBuildContainer.ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Bomb.ProjectileTypeToString()) || x.WeaponType.Equals(ProjectileType.SkipBomb.ProjectileTypeToString())).ToList();

        BombersType = bombers?.Select(x => x.PlaneVariant).ToList() ?? new();
        BombersInSquadron = bombers?.Select(x => x.NumberInSquad).ToList() ?? new();
        BombersPerAttack = bombers?.Select(x => x.NumberDuringAttack).ToList() ?? new();
        BombersOnDeck = bombers?.Select(x => x.MaxNumberOnDeck).ToList() ?? new();
        BombersRestorationTime = bombers?.Select(x => x.RestorationTime).ToList() ?? new();
        BombersCruisingSpeed = bombers?.Select(x => x.CruisingSpeed).ToList() ?? new();
        BombersMaxSpeed = bombers?.Select(x => x.MaxSpeed).ToList() ?? new();
        BombersMinSpeed = bombers?.Select(x => x.MinSpeed).ToList() ?? new();
        BombersEngineBoostDuration = bombers?.Select(x => x.MaxEngineBoostDuration).ToList() ?? new();
        BombersInitialBoostDuration = bombers?.Select(x => x.JatoDuration).ToList() ?? new();
        BombersInitialBoostValue = bombers?.Select(x => x.JatoSpeedMultiplier).ToList() ?? new();
        BombersPlaneHp = bombers?.Select(x => x.PlaneHp).ToList() ?? new();
        BombersSquadronHp = bombers?.Select(x => x.SquadronHp).ToList() ?? new();
        BombersAttackGroupHp = bombers?.Select(x => x.AttackGroupHp).ToList() ?? new();
        BombersDamageDuringAttack = bombers?.Select(x => x.DamageTakenDuringAttack).ToList() ?? new();
        BombersWeaponsPerPlane = bombers?.Select(x => x.AmmoPerAttack).ToList() ?? new();
        BombersPreparationTime = bombers?.Select(x => x.PreparationTime).ToList() ?? new();
        BombersAimingTime = bombers?.Select(x => x.AimingTime).ToList() ?? new();
        BombersTimeToFullyAimed = bombers?.Select(x => x.TimeToFullyAimed).ToList() ?? new();
        BombersPostAttackInvulnerability = bombers?.Select(x => x.PostAttackInvulnerabilityDuration).ToList() ?? new();
        BombersAttackCooldown = bombers?.Select(x => x.AttackCd).ToList() ?? new();
        BombersConcealment = bombers?.Select(x => x.ConcealmentFromShips).ToList() ?? new();
        BombersSpotting = bombers?.Select(x => x.MaxViewDistance).ToList() ?? new();
        BombersAreaChangeAiming = bombers?.Select(x => x.AimingRateMoving).ToList() ?? new();
        BombersAreaChangePreparation = bombers?.Select(x => x.AimingPreparationRateMoving).ToList() ?? new();
        BombersInnerEllipse = bombers?.Select(x => x.InnerBombPercentage).ToList() ?? new();

        //Bombs
        List<BombDataContainer?>? bombs = bombers?.Select(x => x.Weapon as BombDataContainer).ToList();

        BombersWeaponType = bombers?.Select(x => x.WeaponType).ToList() ?? new();
        BombersWeaponBombType = bombs?.Select(x => x?.BombType ?? default!).ToList() ?? new();
        BombersWeaponDamage = bombs?.Select(x => x?.Damage ?? 0).ToList() ?? new();
        BombersWeaponSplashRadius = bombs?.Select(x => x?.SplashRadius ?? 0).ToList() ?? new();
        BombersWeaponSplashDamage = bombs?.Select(x => x?.SplashDmg ?? 0).ToList() ?? new();
        BombersWeaponPenetration = bombs?.Select(x => x?.Penetration ?? 0).ToList() ?? new();
        BombersWeaponFireChance = bombs?.Select(x => x?.FireChance ?? 0).ToList() ?? new();
        BombersWeaponBlastRadius = bombs?.Select(x => x?.ExplosionRadius ?? 0).ToList() ?? new();
        BombersWeaponBlastPenetration = bombs?.Select(x => x?.SplashCoeff ?? 0).ToList() ?? new();
        BombersWeaponFuseTimer = bombs?.Select(x => x?.FuseTimer ?? 0).ToList() ?? new();
        BombersWeaponArmingThreshold = bombs?.Select(x => x?.ArmingThreshold ?? 0).ToList() ?? new();
        BombersWeaponRicochetAngles = bombs?.Select(x => x?.RicochetAngles ?? default!).ToList() ?? new();
    }

    private readonly ShipBuildContainer shipBuildContainer;

    public Ship Ship { get; }

    public ShipDataContainer ShipDataContainer { get; }

    public Build? Build { get; }

    public Guid Id { get; }

    //base
    public string ShipIndex { get; }

    public Nation ShipNation { get; }

    public ShipClass ShipClass { get; }

    public ShipCategory ShipCategory { get; }

    public int ShipTier { get; }

    public string? BuildName { get; }

    //Main battery
    public MainBatteryDataContainer? MainBattery { get; }

    //HE shells
    public ShellDataContainer? HeShell { get; }

    //AP shells
    public ShellDataContainer? ApShell { get; }

    //SAP shells
    public ShellDataContainer? SapShell { get; }

    //TorpLaunchers
    public TorpedoArmamentDataContainer? TorpedoLauncher { get; set; }

    //Torpedoes
    public List<string?> TorpedoFullSalvoDamage { get; }

    public List<string> TorpedoType { get; }

    public List<decimal> TorpedoDamage { get; }

    public List<decimal> TorpedoRange { get; }

    public List<decimal> TorpedoSpeed { get; }

    public List<decimal> TorpedoDetectRange { get; }

    public List<int> TorpedoArmingDistance { get; }

    public List<decimal> TorpedoReactionTime { get; }

    public List<decimal> TorpedoFloodingChance { get; }

    public List<decimal> TorpedoBlastRadius { get; }

    public List<decimal> TorpedoBlastPenetration { get; }

    public List<List<ShipClass>?> TorpedoCanHit { get; }

    //Secondaries
    public SecondaryGridDataWrapper Secondary { get; }

    //AA
    public AntiAirDataContainer? AntiAirArmament { get; }

    //ASW
    public AswGridDataWrapper Asw { get; }

    //AirStrike
    public AirstrikeDataContainer? AirStrike { get; }

    public BombDataContainer? AirStrikeWeapon { get; }

    //Maneuverability
    public ManeuverabilityGridDataWrapper ManeuverabilityData { get; }

    //Concealment

    public ConcealmentDataContainer Concealment { get; }

    //Survivability
    public SurvivabilityDataContainer Survivability { get; }

    //Sonar
    public PingerGunDataContainer? Sonar { get; }

    //RocketPlanes
    public List<string> RocketPlanesType { get; }

    public List<int> RocketPlanesInSquadron { get; }

    public List<int> RocketPlanesPerAttack { get; }

    public List<int> RocketPlanesOnDeck { get; }

    public List<decimal> RocketPlanesRestorationTime { get; }

    public List<decimal> RocketPlanesCruisingSpeed { get; }

    public List<decimal> RocketPlanesMaxSpeed { get; }

    public List<decimal> RocketPlanesMinSpeed { get; }

    public List<decimal> RocketPlanesEngineBoostDuration { get; }

    public List<decimal> RocketPlanesInitialBoostDuration { get; }

    public List<decimal> RocketPlanesInitialBoostValue { get; }

    public List<int> RocketPlanesPlaneHp { get; }

    public List<int> RocketPlanesSquadronHp { get; }

    public List<int> RocketPlanesAttackGroupHp { get; }

    public List<int> RocketPlanesDamageDuringAttack { get; }

    public List<int> RocketPlanesWeaponsPerPlane { get; }

    public List<decimal> RocketPlanesPreparationTime { get; }

    public List<decimal> RocketPlanesAimingTime { get; }

    public List<decimal> RocketPlanesTimeToFullyAimed { get; }

    public List<decimal> RocketPlanesPostAttackInvulnerability { get; }

    public List<decimal> RocketPlanesAttackCooldown { get; }

    public List<decimal> RocketPlanesConcealment { get; }

    public List<decimal> RocketPlanesSpotting { get; }

    public List<string> RocketPlanesAreaChangeAiming { get; }

    public List<string> RocketPlanesAreaChangePreparation { get; }

    public List<string> RocketPlanesWeaponType { get; }

    public List<decimal> RocketPlanesWeaponDamage { get; }

    public List<decimal> RocketPlanesWeaponSplashRadius { get; }

    public List<decimal> RocketPlanesWeaponSplashDamage { get; }

    public List<int> RocketPlanesWeaponPenetration { get; }

    public List<decimal> RocketPlanesWeaponFireChance { get; }

    public List<decimal> RocketPlanesWeaponFuseTimer { get; }

    public List<int> RocketPlanesWeaponArmingThreshold { get; }

    public List<string> RocketPlanesWeaponRicochetAngles { get; }

    public List<decimal> RocketPlanesWeaponBlastRadius { get; }

    public List<decimal> RocketPlanesWeaponBlastPenetration { get; }

    //TorpedoBombers
    public List<string> TorpedoBombersType { get; }

    public List<int> TorpedoBombersInSquadron { get; }

    public List<int> TorpedoBombersPerAttack { get; }

    public List<int> TorpedoBombersOnDeck { get; }

    public List<decimal> TorpedoBombersRestorationTime { get; }

    public List<decimal> TorpedoBombersCruisingSpeed { get; }

    public List<decimal> TorpedoBombersMaxSpeed { get; }

    public List<decimal> TorpedoBombersMinSpeed { get; }

    public List<decimal> TorpedoBombersEngineBoostDuration { get; }

    public List<decimal> TorpedoBombersInitialBoostDuration { get; }

    public List<decimal> TorpedoBombersInitialBoostValue { get; }

    public List<int> TorpedoBombersPlaneHp { get; }

    public List<int> TorpedoBombersSquadronHp { get; }

    public List<int> TorpedoBombersAttackGroupHp { get; }

    public List<int> TorpedoBombersDamageDuringAttack { get; }

    public List<int> TorpedoBombersWeaponsPerPlane { get; }

    public List<decimal> TorpedoBombersPreparationTime { get; }

    public List<decimal> TorpedoBombersAimingTime { get; }

    public List<decimal> TorpedoBombersTimeToFullyAimed { get; }

    public List<decimal> TorpedoBombersPostAttackInvulnerability { get; }

    public List<decimal> TorpedoBombersAttackCooldown { get; }

    public List<decimal> TorpedoBombersConcealment { get; }

    public List<decimal> TorpedoBombersSpotting { get; }

    public List<string> TorpedoBombersAreaChangeAiming { get; }

    public List<string> TorpedoBombersAreaChangePreparation { get; }

    //Aerial torps
    public List<string> TorpedoBombersWeaponType { get; }

    public List<decimal> TorpedoBombersWeaponDamage { get; }

    public List<decimal> TorpedoBombersWeaponRange { get; }

    public List<decimal> TorpedoBombersWeaponSpeed { get; }

    public List<decimal> TorpedoBombersWeaponDetectabilityRange { get; }

    public List<int> TorpedoBombersWeaponArmingDistance { get; }

    public List<decimal> TorpedoBombersWeaponReactionTime { get; }

    public List<decimal> TorpedoBombersWeaponFloodingChance { get; }

    public List<decimal> TorpedoBombersWeaponBlastRadius { get; }

    public List<decimal> TorpedoBombersWeaponBlastPenetration { get; }

    public List<List<ShipClass>?> TorpedoBombersWeaponCanHit { get; }

    //Bombers
    public List<string> BombersType { get; }

    public List<int> BombersInSquadron { get; }

    public List<int> BombersPerAttack { get; }

    public List<int> BombersOnDeck { get; }

    public List<decimal> BombersRestorationTime { get; }

    public List<decimal> BombersCruisingSpeed { get; }

    public List<decimal> BombersMaxSpeed { get; }

    public List<decimal> BombersMinSpeed { get; }

    public List<decimal> BombersEngineBoostDuration { get; }

    public List<decimal> BombersInitialBoostDuration { get; }

    public List<decimal> BombersInitialBoostValue { get; }

    public List<int> BombersPlaneHp { get; }

    public List<int> BombersSquadronHp { get; }

    public List<int> BombersAttackGroupHp { get; }

    public List<int> BombersDamageDuringAttack { get; }

    public List<int> BombersWeaponsPerPlane { get; }

    public List<decimal> BombersPreparationTime { get; }

    public List<decimal> BombersAimingTime { get; }

    public List<decimal> BombersTimeToFullyAimed { get; }

    public List<decimal> BombersPostAttackInvulnerability { get; }

    public List<decimal> BombersAttackCooldown { get; }

    public List<decimal> BombersConcealment { get; }

    public List<decimal> BombersSpotting { get; }

    public List<string> BombersAreaChangeAiming { get; }

    public List<string> BombersAreaChangePreparation { get; }

    public List<int> BombersInnerEllipse { get; }

    //Bombs
    public List<string> BombersWeaponType { get; }

    public List<string> BombersWeaponBombType { get; }

    public List<decimal> BombersWeaponDamage { get; }

    public List<decimal> BombersWeaponSplashRadius { get; }

    public List<decimal> BombersWeaponSplashDamage { get; }

    public List<int> BombersWeaponPenetration { get; }

    public List<decimal> BombersWeaponFireChance { get; }

    public List<decimal> BombersWeaponBlastRadius { get; }

    public List<decimal> BombersWeaponBlastPenetration { get; }

    public List<decimal> BombersWeaponFuseTimer { get; }

    public List<int> BombersWeaponArmingThreshold { get; }

    public List<string> BombersWeaponRicochetAngles { get; }
}
