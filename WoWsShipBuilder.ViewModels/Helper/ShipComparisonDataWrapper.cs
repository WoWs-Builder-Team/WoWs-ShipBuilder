using System;
using System.IO;
using WoWsShipBuilder.Core.Builds;
using WoWsShipBuilder.Core.Data;
using WoWsShipBuilder.Core.DataContainers;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilder.Core.Localization;
using WoWsShipBuilder.DataStructures.Ship;

namespace WoWsShipBuilder.ViewModels.Helper;

public sealed class ShipComparisonDataWrapper
{
    public static ShipComparisonDataWrapper CreateNew(Ship ship, ShipDataContainer shipDataContainer)
    {
        return new(ShipBuildContainer.CreateNew(ship, null, null) with { ShipDataContainer = shipDataContainer });
    }

    public ShipBuildContainer GetShipBuildContainer()
    {
        return shipBuildContainer;
    }

    public ShipComparisonDataWrapper(ShipBuildContainer shipBuildContainer)
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
        var mainBattery = shipBuildContainer.ShipDataContainer.MainBatteryDataContainer;

        MainBatteryCaliber = mainBattery?.GunCaliber;
        MainBatteryBarrelCount = mainBattery?.BarrelsCount;
        MainBatteryBarrelsLayout = mainBattery?.BarrelsLayout;
        MainBatteryRange = mainBattery?.Range;
        MainBatteryTurnTime = mainBattery?.TurnTime;
        MainBatteryTraverseSpeed = mainBattery?.TraverseSpeed;
        MainBatteryReload = mainBattery?.Reload;
        MainBatteryRoF = mainBattery?.RoF;
        MainBatteryHeDpm = mainBattery?.TheoreticalHeDpm;
        MainBatterySapDpm = mainBattery?.TheoreticalSapDpm;
        MainBatteryApDpm = mainBattery?.TheoreticalApDpm;
        MainBatteryHeSalvo = mainBattery?.HeSalvo;
        MainBatterySapSalvo = mainBattery?.SapSalvo;
        MainBatteryApSalvo = mainBattery?.ApSalvo;
        MainBatteryFpm = mainBattery?.PotentialFpm;
        MainBatterySigma = mainBattery?.Sigma;

        //HE shells
        var heShellData = mainBattery?.ShellData.FirstOrDefault(x => x.Type.Equals($"ArmamentType_{ShellType.HE.ShellTypeToString()}"));

        HeMass = heShellData?.Mass;
        HeDamage = heShellData?.Damage;
        HeSplashRadius = heShellData?.SplashRadius;
        HeSplashDamage = heShellData?.SplashDmg;
        HePenetration = heShellData?.Penetration;
        HeSpeed = heShellData?.ShellVelocity;
        HeAirDrag = heShellData?.AirDrag;
        HeShellFireChance = heShellData?.ShellFireChance;
        HeSalvoFireChance = heShellData?.FireChancePerSalvo;
        HeBlastRadius = heShellData?.ExplosionRadius;
        HeBlastPenetration = heShellData?.SplashCoeff;

        //AP shells
        var apShellData = mainBattery?.ShellData.FirstOrDefault(x => x.Type.Equals($"ArmamentType_{ShellType.AP.ShellTypeToString()}"));

        ApMass = apShellData?.Mass;
        ApDamage = apShellData?.Damage;
        ApSplashRadius = apShellData?.SplashRadius;
        ApSplashDamage = apShellData?.SplashDmg;
        ApSpeed = apShellData?.ShellVelocity;
        ApAirDrag = apShellData?.AirDrag;
        ApOvermatch = apShellData?.Overmatch;
        ApRicochet = apShellData?.RicochetAngles;
        ApArmingThreshold = apShellData?.ArmingThreshold;
        ApFuseTimer = apShellData?.FuseTimer;

        //SAP shells
        var sapShellData = mainBattery?.ShellData.FirstOrDefault(x => x.Type.Equals($"ArmamentType_{ShellType.SAP.ShellTypeToString()}"));

        SapMass = sapShellData?.Mass;
        SapDamage = sapShellData?.Damage;
        SapSplashRadius = sapShellData?.SplashRadius;
        SapSplashDamage = sapShellData?.SplashDmg;
        SapPenetration = sapShellData?.Penetration;
        SapSpeed = sapShellData?.ShellVelocity;
        SapAirDrag = sapShellData?.AirDrag;
        SapOvermatch = sapShellData?.Overmatch;
        SapRicochet = sapShellData?.RicochetAngles;

        //TorpLaunchers
        var torpedoArmament = shipBuildContainer.ShipDataContainer.TorpedoArmamentDataContainer;

        TorpedoCount = torpedoArmament?.TorpCount;
        TorpedoLayout = torpedoArmament?.TorpLayout;
        TorpedoTurnTime = torpedoArmament?.TurnTime;
        TorpedoTraverseSpeed = torpedoArmament?.TraverseSpeed;
        TorpedoReload = torpedoArmament?.Reload;
        TorpedoSpread = torpedoArmament?.TorpedoArea;
        TorpedoTimeToSwitch = torpedoArmament?.TimeToSwitch;

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
        List<SecondaryBatteryDataContainer>? secondaryBattery = shipBuildContainer.ShipDataContainer.SecondaryBatteryUiDataContainer.Secondaries;

        SecondaryBatteryCaliber = secondaryBattery?.Select(x => x.GunCaliber).ToList() ?? new();
        SecondaryBatteryBarrelCount = secondaryBattery?.Select(x => x.BarrelsCount).ToList() ?? new();
        SecondaryBatteryBarrelsLayout = secondaryBattery?.Select(x => x.BarrelsLayout).ToList() ?? new();
        SecondaryBatteryRange = secondaryBattery?.Select(x => x.Range).First();
        SecondaryBatteryReload = secondaryBattery?.Select(x => x.Reload).ToList() ?? new();
        SecondaryBatteryRoF = secondaryBattery?.Select(x => x.RoF).ToList() ?? new();
        SecondaryBatteryDpm = secondaryBattery?.Select(x => x.TheoreticalDpm).ToList() ?? new();
        SecondaryBatteryFpm = secondaryBattery?.Select(x => x.PotentialFpm).ToList() ?? new();
        SecondaryBatterySigma = secondaryBattery?.Select(x => x.Sigma).First();

        //Secondary shells
        List<ShellDataContainer?>? secondaryShellData = secondaryBattery?.Select(x => x.Shell).ToList();

        SecondaryType = secondaryShellData?.Select(x => x?.Type).First();
        SecondaryMass = secondaryShellData?.Select(x => x?.Mass ?? 0).ToList() ?? new();
        SecondaryDamage = secondaryShellData?.Select(x => x?.Damage ?? 0).ToList() ?? new();
        SecondarySplashRadius = secondaryShellData?.Select(x => x?.SplashRadius ?? 0).ToList() ?? new();
        SecondarySplashDamage = secondaryShellData?.Select(x => x?.SplashDmg ?? 0).ToList() ?? new();
        SecondaryPenetration = secondaryShellData?.Select(x => x?.Penetration ?? 0).ToList() ?? new();
        SecondarySpeed = secondaryShellData?.Select(x => x?.ShellVelocity ?? 0).ToList() ?? new();
        SecondaryAirDrag = secondaryShellData?.Select(x => x?.AirDrag ?? 0).ToList() ?? new();
        SecondaryHeShellFireChance = secondaryShellData?.Select(x => x?.ShellFireChance ?? 0).ToList() ?? new();
        SecondaryHeBlastRadius = secondaryShellData?.Select(x => x?.ExplosionRadius ?? 0).ToList() ?? new();
        SecondaryHeBlastPenetration = secondaryShellData?.Select(x => x?.SplashCoeff ?? 0).ToList() ?? new();
        SecondarySapOvermatch = secondaryShellData?.Select(x => x?.Overmatch ?? 0).ToList() ?? new();
        SecondarySapRicochet = secondaryShellData?.Select(x => x?.RicochetAngles ?? default!).ToList() ?? new();

        //AA
        var aaData = shipBuildContainer.ShipDataContainer.AntiAirDataContainer;

        LongAaRange = aaData?.LongRangeAura?.Range;
        MediumAaRange = aaData?.MediumRangeAura?.Range;
        ShortAaRange = aaData?.ShortRangeAura?.Range;
        LongAaConstantDamage = aaData?.LongRangeAura?.ConstantDamage;
        MediumAaConstantDamage = aaData?.MediumRangeAura?.ConstantDamage;
        ShortAaConstantDamage = aaData?.ShortRangeAura?.ConstantDamage;
        LongAaHitChance = aaData?.LongRangeAura?.HitChance;
        MediumAaHitChance = aaData?.MediumRangeAura?.HitChance;
        ShortAaHitChance = aaData?.ShortRangeAura?.HitChance;
        Flak = aaData?.LongRangeAura?.Flak;
        FlakDamage = aaData?.LongRangeAura?.FlakDamage;

        //ASW
        var aswAirStrike = shipBuildContainer.ShipDataContainer.AswAirstrikeDataContainer;
        var depthChargeLauncher = shipBuildContainer.ShipDataContainer.DepthChargeLauncherDataContainer;
        var depthCharge = depthChargeLauncher?.DepthCharge ?? aswAirStrike?.Weapon as DepthChargeDataContainer;

        AswDcType = aswAirStrike is not null ? nameof(Translation.ShipStats_AswAirstrike) : depthChargeLauncher is not null ? "DepthCharge" : null;
        AswRange = aswAirStrike?.MaximumDistance;
        AswMaxDropLength = aswAirStrike?.MaximumFlightDistance;
        AswDcReload = depthChargeLauncher?.Reload ?? aswAirStrike?.ReloadTime;
        AswDcUses = depthChargeLauncher?.NumberOfUses ?? aswAirStrike?.NumberOfUses;
        AswPlanesInSquadron = aswAirStrike?.NumberDuringAttack;
        AswBombsPerPlane = aswAirStrike?.BombsPerPlane;
        DcPerAttack = depthChargeLauncher?.BombsPerCharge;
        DcDamage = depthCharge?.Damage;
        DcFireChance = depthCharge?.FireChance;
        DcFloodingChance = depthCharge?.FloodingChance;
        DcSplashRadius = depthCharge?.DcSplashRadius;
        DcSinkSpeed = depthCharge?.SinkSpeed;
        DcDetonationTimer = depthCharge?.DetonationTimer;
        DcDetonationDepth = depthCharge?.DetonationDepth;

        //AirStrike
        var airStrike = shipBuildContainer.ShipDataContainer.AirstrikeDataContainer;
        var airStrikeWeapon = airStrike?.Weapon as BombDataContainer;

        AirStrikePlanesHp = airStrike?.PlaneHp;
        AirStrikeRange = airStrike?.MaximumDistance;
        AirStrikeMaxDropLength = airStrike?.MaximumFlightDistance;
        AirStrikeReload = airStrike?.ReloadTime;
        AirStrikeUses = airStrike?.NumberOfUses;
        AirStrikePlanesInSquadron = airStrike?.NumberDuringAttack;
        AirStrikeBombsPerPlane = airStrike?.BombsPerPlane;
        AirStrikeBombType = airStrikeWeapon?.BombType;
        AirStrikeDamage = airStrikeWeapon?.Damage;
        AirStrikeFireChance = airStrikeWeapon?.FireChance;
        AirStrikeSplashRadius = airStrikeWeapon?.SplashRadius;
        AirStrikeSplashDamage = airStrikeWeapon?.SplashDmg;
        AirStrikePenetration = airStrikeWeapon?.Penetration;
        AirStrikeBlastRadius = airStrikeWeapon?.ExplosionRadius;
        AirStrikeBlastDamage = airStrikeWeapon?.SplashCoeff;

        //Maneuverability
        var maneuverability = shipBuildContainer.ShipDataContainer.ManeuverabilityDataContainer;

        ManeuverabilityMaxSpeed = maneuverability.ManeuverabilityMaxSpeed != 0 ? maneuverability.ManeuverabilityMaxSpeed : maneuverability.ManeuverabilitySubsMaxSpeedOnSurface;
        ManeuverabilityMaxSpeedAtPeriscopeDepth = maneuverability.ManeuverabilitySubsMaxSpeedAtPeriscope;
        ManeuverabilityMaxSpeedAtMaxDepth = maneuverability.ManeuverabilitySubsMaxSpeedAtMaxDepth;
        ManeuverabilityRudderShiftTime = maneuverability.ManeuverabilityRudderShiftTime;
        ManeuverabilityTurningCircle = maneuverability.ManeuverabilityTurningCircle;
        ManeuverabilityTimeToFullAhead = maneuverability.ForwardMaxSpeedTime;
        ManeuverabilityTimeToFullReverse = maneuverability.ReverseMaxSpeedTime;
        ManeuverabilityRudderProtection = maneuverability.RudderBlastProtection;
        ManeuverabilityEngineProtection = maneuverability.EngineBlastProtection;

        //Concealment
        var concealment = shipBuildContainer.ShipDataContainer.ConcealmentDataContainer;

        ConcealmentFromShipsBase = concealment.ConcealmentBySea;
        ConcealmentFromShipsOnFire = concealment.ConcealmentBySeaFire;
        ConcealmentFromShipsSmokeFiring = concealment.ConcealmentBySeaFiringSmoke;
        ConcealmentFromPlanesBase = concealment.ConcealmentByAir;
        ConcealmentFromPlanesOnFire = concealment.ConcealmentByAirFire;
        ConcealmentFromSubsPeriscopeDepth = concealment.ConcealmentBySubPeriscope;

        //Survivability
        var survivability = shipBuildContainer.ShipDataContainer.SurvivabilityDataContainer;

        SurvivabilityShipHp = survivability.HitPoints;
        SurvivabilityFireMaxAmount = survivability.FireAmount;
        SurvivabilityFireDuration = survivability.FireDuration;
        SurvivabilityFireDps = survivability.FireDPS;
        SurvivabilityFireMaxDmg = survivability.FireTotalDamage;
        SurvivabilityFireChanceReduction = survivability.FireReduction;
        SurvivabilityFloodingMaxAmount = survivability.FloodAmount;
        SurvivabilityFloodingDuration = survivability.FloodDuration;
        SurvivabilityFloodingDps = survivability.FloodDPS;
        SurvivabilityFloodingMaxDmg = survivability.FloodTotalDamage;
        SurvivabilityFloodingTorpedoProtection = survivability.FloodTorpedoProtection;

        //Sonar
        var pingerGun = shipBuildContainer.ShipDataContainer.PingerGunDataContainer;

        SonarReloadTime = pingerGun?.Reload;
        SonarTraverseSpeed = pingerGun?.TraverseSpeed;
        SonarTurnTime = pingerGun?.TurnTime;
        SonarRange = pingerGun?.Range;
        SonarWidth = pingerGun?.PingWidth;
        SonarSpeed = pingerGun?.PingSpeed;
        SonarFirstPingDuration = pingerGun?.FirstPingDuration;
        SonarSecondPingDuration = pingerGun?.SecondPingDuration;

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
    public decimal? MainBatteryCaliber { get; }

    public int? MainBatteryBarrelCount { get; }

    public string? MainBatteryBarrelsLayout { get; }

    public decimal? MainBatteryRange { get; }

    public decimal? MainBatteryTurnTime { get; }

    public decimal? MainBatteryTraverseSpeed { get; }

    public decimal? MainBatteryReload { get; }

    public decimal? MainBatteryRoF { get; }

    public string? MainBatteryHeDpm { get; }

    public string? MainBatterySapDpm { get; }

    public string? MainBatteryApDpm { get; }

    public string? MainBatteryHeSalvo { get; }

    public string? MainBatterySapSalvo { get; }

    public string? MainBatteryApSalvo { get; }

    public decimal? MainBatteryFpm { get; }

    public decimal? MainBatterySigma { get; }

    //HE shells
    public decimal? HeMass { get; }

    public decimal? HeDamage { get; }

    public decimal? HeSplashRadius { get; }

    public decimal? HeSplashDamage { get; }

    public decimal? HePenetration { get; }

    public decimal? HeSpeed { get; }

    public decimal? HeAirDrag { get; }

    public decimal? HeShellFireChance { get; }

    public decimal? HeSalvoFireChance { get; }

    public decimal? HeBlastRadius { get; }

    public decimal? HeBlastPenetration { get; }

    //AP shells
    public decimal? ApMass { get; }

    public decimal? ApDamage { get; }

    public decimal? ApSplashRadius { get; }

    public decimal? ApSplashDamage { get; }

    public decimal? ApSpeed { get; }

    public decimal? ApAirDrag { get; }

    public decimal? ApOvermatch { get; }

    public string? ApRicochet { get; }

    public decimal? ApArmingThreshold { get; }

    public decimal? ApFuseTimer { get; }

    //SAP shells
    public decimal? SapMass { get; }

    public decimal? SapDamage { get; }

    public decimal? SapSplashRadius { get; }

    public decimal? SapSplashDamage { get; }

    public decimal? SapPenetration { get; }

    public decimal? SapSpeed { get; }

    public decimal? SapAirDrag { get; }

    public decimal? SapOvermatch { get; }

    public string? SapRicochet { get; }

    //TorpLaunchers
    public int? TorpedoCount { get; }

    public string? TorpedoLayout { get; }

    public decimal? TorpedoTurnTime { get; }

    public decimal? TorpedoTraverseSpeed { get; }

    public decimal? TorpedoReload { get; }

    public string? TorpedoSpread { get; }

    public decimal? TorpedoTimeToSwitch { get; }

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
    public List<decimal> SecondaryBatteryCaliber { get; }

    public List<int> SecondaryBatteryBarrelCount { get; }

    public List<string> SecondaryBatteryBarrelsLayout { get; }

    public decimal? SecondaryBatteryRange { get; }

    public List<decimal> SecondaryBatteryReload { get; }

    public List<decimal> SecondaryBatteryRoF { get; }

    public List<string> SecondaryBatteryDpm { get; }

    public List<decimal> SecondaryBatteryFpm { get; }

    public decimal? SecondaryBatterySigma { get; }

    //Secondary shells
    public string? SecondaryType { get; }

    public List<decimal> SecondaryMass { get; }

    public List<decimal> SecondaryDamage { get; }

    public List<decimal> SecondarySplashRadius { get; }

    public List<decimal> SecondarySplashDamage { get; }

    public List<int> SecondaryPenetration { get; }

    public List<decimal> SecondarySpeed { get; }

    public List<decimal> SecondaryAirDrag { get; }

    public List<decimal> SecondaryHeShellFireChance { get; }

    public List<decimal> SecondaryHeBlastRadius { get; }

    public List<decimal> SecondaryHeBlastPenetration { get; }

    public List<decimal> SecondarySapOvermatch { get; }

    public List<string> SecondarySapRicochet { get; }

    //AA
    public decimal? LongAaRange { get; }

    public decimal? MediumAaRange { get; }

    public decimal? ShortAaRange { get; }

    public decimal? LongAaConstantDamage { get; }

    public decimal? MediumAaConstantDamage { get; }

    public decimal? ShortAaConstantDamage { get; }

    public decimal? LongAaHitChance { get; }

    public decimal? MediumAaHitChance { get; }

    public decimal? ShortAaHitChance { get; }

    public string? Flak { get; }

    public decimal? FlakDamage { get; }

    //ASW
    public string? AswDcType { get; }

    public decimal? AswRange { get; }

    public decimal? AswMaxDropLength { get; }

    public decimal? AswDcReload { get; }

    public int? AswDcUses { get; }

    public int? AswPlanesInSquadron { get; }

    public int? AswBombsPerPlane { get; }

    public decimal? DcPerAttack { get; }

    public decimal? DcDamage { get; }

    public decimal? DcFireChance { get; }

    public decimal? DcFloodingChance { get; }

    public decimal? DcSplashRadius { get; }

    public string? DcSinkSpeed { get; }

    public string? DcDetonationTimer { get; }

    public string? DcDetonationDepth { get; }

    //AirStrike
    public decimal? AirStrikePlanesHp { get; }

    public decimal? AirStrikeRange { get; }

    public decimal? AirStrikeMaxDropLength { get; }

    public decimal? AirStrikeReload { get; }

    public int? AirStrikeUses { get; }

    public int? AirStrikePlanesInSquadron { get; }

    public int? AirStrikeBombsPerPlane { get; }

    public string? AirStrikeBombType { get; }

    public decimal? AirStrikeDamage { get; }

    public decimal? AirStrikeFireChance { get; }

    public decimal? AirStrikeSplashRadius { get; }

    public decimal? AirStrikeSplashDamage { get; }

    public int? AirStrikePenetration { get; }

    public decimal? AirStrikeBlastRadius { get; }

    public decimal? AirStrikeBlastDamage { get; }

    //Maneuverability
    public decimal ManeuverabilityMaxSpeed { get; }

    public decimal ManeuverabilityMaxSpeedAtPeriscopeDepth { get; }

    public decimal ManeuverabilityMaxSpeedAtMaxDepth { get; }

    public decimal ManeuverabilityRudderShiftTime { get; }

    public decimal ManeuverabilityTurningCircle { get; }

    public decimal ManeuverabilityTimeToFullAhead { get; }

    public decimal ManeuverabilityTimeToFullReverse { get; }

    public decimal ManeuverabilityRudderProtection { get; }

    public decimal ManeuverabilityEngineProtection { get; }

    //Concealment
    public decimal ConcealmentFromShipsBase { get; }

    public decimal ConcealmentFromShipsOnFire { get; }

    public decimal ConcealmentFromShipsSmokeFiring { get; }

    public decimal ConcealmentFromPlanesBase { get; }

    public decimal ConcealmentFromPlanesOnFire { get; }

    public decimal ConcealmentFromSubsPeriscopeDepth { get; }

    //Survivability
    public decimal SurvivabilityShipHp { get; }

    public decimal SurvivabilityFireMaxAmount { get; }

    public decimal SurvivabilityFireDuration { get; }

    public decimal SurvivabilityFireDps { get; }

    public decimal SurvivabilityFireMaxDmg { get; }

    public decimal SurvivabilityFireChanceReduction { get; }

    public decimal SurvivabilityFloodingMaxAmount { get; }

    public decimal SurvivabilityFloodingDuration { get; }

    public decimal SurvivabilityFloodingDps { get; }

    public decimal SurvivabilityFloodingMaxDmg { get; }

    public decimal SurvivabilityFloodingTorpedoProtection { get; }

    //Sonar
    public decimal? SonarReloadTime { get; }

    public decimal? SonarTraverseSpeed { get; }

    public decimal? SonarTurnTime { get; }

    public decimal? SonarRange { get; }

    public decimal? SonarWidth { get; }

    public decimal? SonarSpeed { get; }

    public decimal? SonarFirstPingDuration { get; }

    public decimal? SonarSecondPingDuration { get; }

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
