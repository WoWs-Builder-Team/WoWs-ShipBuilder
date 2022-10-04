using System;
using WoWsShipBuilder.Core.Builds;
using WoWsShipBuilder.Core.DataContainers;
using WoWsShipBuilder.Core.Localization;

namespace WoWsShipBuilder.ViewModels.Helper;

public sealed record ShipComparisonDataWrapper(Ship Ship, ShipDataContainer ShipDataContainer, Build? Build = null)
{
    public ShipComparisonDataWrapper(Ship ship, ShipDataContainer shipDataContainer, Build? build, Guid id)
        : this(ship, shipDataContainer, build)
    {
        Id = id;
    }

    public Guid Id { get; } = Guid.NewGuid();

    //base
    public string ShipIndex => Ship.Index;
    public Nation ShipNation => Ship.ShipNation;
    public ShipClass ShipClass => Ship.ShipClass;
    public ShipCategory ShipCategory => Ship.ShipCategory;
    public int ShipTier => Ship.Tier;
    public string? BuildName => Build?.BuildName;

    //Main battery
    public decimal? MainBatteryCaliber { get; } = ShipDataContainer.MainBatteryDataContainer?.GunCaliber;
    public int? MainBatteryBarrelCount { get; } = ShipDataContainer.MainBatteryDataContainer?.BarrelsCount;
    public string? MainBatteryBarrelsLayout { get; } = ShipDataContainer.MainBatteryDataContainer?.BarrelsLayout;
    public decimal? MainBatteryRange { get; } = ShipDataContainer.MainBatteryDataContainer?.Range;
    public decimal? MainBatteryTurnTime { get; } = ShipDataContainer.MainBatteryDataContainer?.TurnTime;
    public decimal? MainBatteryTraverseSpeed { get; } = ShipDataContainer.MainBatteryDataContainer?.TraverseSpeed;
    public decimal? MainBatteryReload { get; } = ShipDataContainer.MainBatteryDataContainer?.Reload;
    public decimal? MainBatteryRoF { get; } = ShipDataContainer.MainBatteryDataContainer?.RoF;
    public string? MainBatteryHeDpm { get; } = ShipDataContainer.MainBatteryDataContainer?.TheoreticalHeDpm;
    public string? MainBatterySapDpm { get; } = ShipDataContainer.MainBatteryDataContainer?.TheoreticalSapDpm;
    public string? MainBatteryApDpm { get; } = ShipDataContainer.MainBatteryDataContainer?.TheoreticalApDpm;
    public string? MainBatteryHeSalvo { get; } = ShipDataContainer.MainBatteryDataContainer?.HeSalvo;
    public string? MainBatterySapSalvo { get; } = ShipDataContainer.MainBatteryDataContainer?.SapSalvo;
    public string? MainBatteryApSalvo { get; } = ShipDataContainer.MainBatteryDataContainer?.ApSalvo;
    public decimal? MainBatteryFpm { get; } = ShipDataContainer.MainBatteryDataContainer?.PotentialFpm;
    public decimal? MainBatterySigma { get; } = ShipDataContainer.MainBatteryDataContainer?.Sigma;

    //HE shells
    public decimal? HeMass { get; } = ShipDataContainer.MainBatteryDataContainer?.ShellData.FirstOrDefault(x => x.Type.Equals($"ArmamentType_{ShellType.HE}"))?.Mass;
    public decimal? HeDamage { get; } = ShipDataContainer.MainBatteryDataContainer?.ShellData.FirstOrDefault(x => x.Type.Equals($"ArmamentType_{ShellType.HE}"))?.Damage;
    public decimal? HeSplashRadius { get; } = ShipDataContainer.MainBatteryDataContainer?.ShellData.FirstOrDefault(x => x.Type.Equals($"ArmamentType_{ShellType.HE}"))?.SplashRadius;
    public decimal? HeSplashDamage { get; } = ShipDataContainer.MainBatteryDataContainer?.ShellData.FirstOrDefault(x => x.Type.Equals($"ArmamentType_{ShellType.HE}"))?.SplashDmg;
    public decimal? HePenetration { get; } = ShipDataContainer.MainBatteryDataContainer?.ShellData.FirstOrDefault(x => x.Type.Equals($"ArmamentType_{ShellType.HE}"))?.Penetration;
    public decimal? HeSpeed { get; } = ShipDataContainer.MainBatteryDataContainer?.ShellData.FirstOrDefault(x => x.Type.Equals($"ArmamentType_{ShellType.HE}"))?.ShellVelocity;
    public decimal? HeAirDrag { get; } = ShipDataContainer.MainBatteryDataContainer?.ShellData.FirstOrDefault(x => x.Type.Equals($"ArmamentType_{ShellType.HE}"))?.AirDrag;
    public decimal? HeShellFireChance { get; } = ShipDataContainer.MainBatteryDataContainer?.ShellData.FirstOrDefault(x => x.Type.Equals($"ArmamentType_{ShellType.HE}"))?.ShellFireChance;
    public decimal? HeSalvoFireChance { get; } = ShipDataContainer.MainBatteryDataContainer?.ShellData.FirstOrDefault(x => x.Type.Equals($"ArmamentType_{ShellType.HE}"))?.FireChancePerSalvo;
    public decimal? HeBlastRadius { get; } = ShipDataContainer.MainBatteryDataContainer?.ShellData.FirstOrDefault(x => x.Type.Equals($"ArmamentType_{ShellType.HE}"))?.ExplosionRadius;
    public decimal? HeBlastPenetration { get; } = ShipDataContainer.MainBatteryDataContainer?.ShellData.FirstOrDefault(x => x.Type.Equals($"ArmamentType_{ShellType.HE}"))?.SplashCoeff;

    //AP shells
    public decimal? ApMass { get; } = ShipDataContainer.MainBatteryDataContainer?.ShellData.FirstOrDefault(x => x.Type.Equals($"ArmamentType_{ShellType.AP}"))?.Mass;
    public decimal? ApDamage { get; } = ShipDataContainer.MainBatteryDataContainer?.ShellData.FirstOrDefault(x => x.Type.Equals($"ArmamentType_{ShellType.AP}"))?.Damage;
    public decimal? ApSplashRadius { get; } = ShipDataContainer.MainBatteryDataContainer?.ShellData.FirstOrDefault(x => x.Type.Equals($"ArmamentType_{ShellType.AP}"))?.SplashRadius;
    public decimal? ApSplashDamage { get; } = ShipDataContainer.MainBatteryDataContainer?.ShellData.FirstOrDefault(x => x.Type.Equals($"ArmamentType_{ShellType.AP}"))?.SplashDmg;
    public decimal? ApSpeed { get; } = ShipDataContainer.MainBatteryDataContainer?.ShellData.FirstOrDefault(x => x.Type.Equals($"ArmamentType_{ShellType.AP}"))?.ShellVelocity;
    public decimal? ApAirDrag { get; } = ShipDataContainer.MainBatteryDataContainer?.ShellData.FirstOrDefault(x => x.Type.Equals($"ArmamentType_{ShellType.AP}"))?.AirDrag;
    public decimal? ApOvermatch { get; } = ShipDataContainer.MainBatteryDataContainer?.ShellData.FirstOrDefault(x => x.Type.Equals($"ArmamentType_{ShellType.AP}"))?.Overmatch;
    public string? ApRicochet { get; } = ShipDataContainer.MainBatteryDataContainer?.ShellData.FirstOrDefault(x => x.Type.Equals($"ArmamentType_{ShellType.AP}"))?.RicochetAngles;
    public decimal? ApArmingThreshold { get; } = ShipDataContainer.MainBatteryDataContainer?.ShellData.FirstOrDefault(x => x.Type.Equals($"ArmamentType_{ShellType.AP}"))?.ArmingThreshold;
    public decimal? ApFuseTimer { get; } = ShipDataContainer.MainBatteryDataContainer?.ShellData.FirstOrDefault(x => x.Type.Equals($"ArmamentType_{ShellType.AP}"))?.FuseTimer;

    //SAP shells
    public decimal? SapMass { get; } = ShipDataContainer.MainBatteryDataContainer?.ShellData.FirstOrDefault(x => x.Type.Equals($"ArmamentType_{ShellType.SAP}"))?.Mass;
    public decimal? SapDamage { get; } = ShipDataContainer.MainBatteryDataContainer?.ShellData.FirstOrDefault(x => x.Type.Equals($"ArmamentType_{ShellType.SAP}"))?.Damage;
    public decimal? SapSplashRadius { get; } = ShipDataContainer.MainBatteryDataContainer?.ShellData.FirstOrDefault(x => x.Type.Equals($"ArmamentType_{ShellType.SAP}"))?.SplashRadius;
    public decimal? SapSplashDamage { get; } = ShipDataContainer.MainBatteryDataContainer?.ShellData.FirstOrDefault(x => x.Type.Equals($"ArmamentType_{ShellType.SAP}"))?.SplashDmg;
    public decimal? SapPenetration { get; } = ShipDataContainer.MainBatteryDataContainer?.ShellData.FirstOrDefault(x => x.Type.Equals($"ArmamentType_{ShellType.SAP}"))?.Penetration;
    public decimal? SapSpeed { get; } = ShipDataContainer.MainBatteryDataContainer?.ShellData.FirstOrDefault(x => x.Type.Equals($"ArmamentType_{ShellType.SAP}"))?.ShellVelocity;
    public decimal? SapAirDrag { get; } = ShipDataContainer.MainBatteryDataContainer?.ShellData.FirstOrDefault(x => x.Type.Equals($"ArmamentType_{ShellType.SAP}"))?.AirDrag;
    public decimal? SapOvermatch { get; } = ShipDataContainer.MainBatteryDataContainer?.ShellData.FirstOrDefault(x => x.Type.Equals($"ArmamentType_{ShellType.SAP}"))?.Overmatch;
    public string? SapRicochet { get; } = ShipDataContainer.MainBatteryDataContainer?.ShellData.FirstOrDefault(x => x.Type.Equals($"ArmamentType_{ShellType.SAP}"))?.RicochetAngles;

    //TorpLaunchers
    public int? TorpedoCount { get; } = ShipDataContainer.TorpedoArmamentDataContainer?.TorpCount;
    public string? TorpedoLayout { get; } = ShipDataContainer.TorpedoArmamentDataContainer?.TorpLayout;
    public decimal? TorpedoTurnTime { get; } = ShipDataContainer.TorpedoArmamentDataContainer?.TurnTime;
    public decimal? TorpedoTraverseSpeed { get; } = ShipDataContainer.TorpedoArmamentDataContainer?.TraverseSpeed;
    public decimal? TorpedoReload { get; } = ShipDataContainer.TorpedoArmamentDataContainer?.Reload;
    public string? TorpedoSpread { get; } = ShipDataContainer.TorpedoArmamentDataContainer?.TorpedoArea;
    public decimal? TorpedoTimeToSwitch { get; } = ShipDataContainer.TorpedoArmamentDataContainer?.TimeToSwitch;

    //Torpedoes
    public List<string?> TorpedoFullSalvoDamage { get; } = new() {ShipDataContainer.TorpedoArmamentDataContainer?.FullSalvoDamage, ShipDataContainer.TorpedoArmamentDataContainer?.TorpFullSalvoDmg, ShipDataContainer.TorpedoArmamentDataContainer?.AltTorpFullSalvoDmg};
    public List<string> TorpedoType { get; } = ShipDataContainer.TorpedoArmamentDataContainer?.Torpedoes.Select(x => x.TorpedoType).ToList() ?? new();
    public List<decimal> TorpedoDamage { get; } = ShipDataContainer.TorpedoArmamentDataContainer?.Torpedoes.Select(x => x.Damage).ToList() ?? new();
    public List<decimal> TorpedoRange { get; } = ShipDataContainer.TorpedoArmamentDataContainer?.Torpedoes.Select(x => x.Range).ToList() ?? new();
    public List<decimal> TorpedoSpeed { get; } = ShipDataContainer.TorpedoArmamentDataContainer?.Torpedoes.Select(x => x.Speed).ToList() ?? new();
    public List<decimal> TorpedoDetectRange { get; } = ShipDataContainer.TorpedoArmamentDataContainer?.Torpedoes.Select(x => x.Detectability).ToList() ?? new();
    public List<int> TorpedoArmingDistance { get; } = ShipDataContainer.TorpedoArmamentDataContainer?.Torpedoes.Select(x => x.ArmingDistance).ToList() ?? new();
    public List<decimal> TorpedoReactionTime { get; } = ShipDataContainer.TorpedoArmamentDataContainer?.Torpedoes.Select(x => x.ReactionTime).ToList() ?? new();
    public List<decimal> TorpedoFloodingChance { get; } = ShipDataContainer.TorpedoArmamentDataContainer?.Torpedoes.Select(x => x.FloodingChance).ToList() ?? new();
    public List<decimal> TorpedoBlastRadius { get; } = ShipDataContainer.TorpedoArmamentDataContainer?.Torpedoes.Select(x => x.ExplosionRadius).ToList() ?? new();
    public List<decimal> TorpedoBlastPenetration { get; } = ShipDataContainer.TorpedoArmamentDataContainer?.Torpedoes.Select(x => x.SplashCoeff).ToList() ?? new();
    public List<List<ShipClass>?> TorpedoCanHit { get; } = ShipDataContainer.TorpedoArmamentDataContainer?.Torpedoes.Select(x => x.CanHitClasses).ToList() ?? new();

    //Secondaries
    public List<decimal> SecondaryBatteryCaliber { get; } = ShipDataContainer.SecondaryBatteryUiDataContainer.Secondaries?.Select(x=> x.GunCaliber).ToList() ?? new();
    public List<int> SecondaryBatteryBarrelCount { get; } = ShipDataContainer.SecondaryBatteryUiDataContainer.Secondaries?.Select(x=> x.BarrelsCount).ToList() ?? new();
    public List<string> SecondaryBatteryBarrelsLayout { get; } = ShipDataContainer.SecondaryBatteryUiDataContainer.Secondaries?.Select(x=> x.BarrelsLayout).ToList() ?? new();
    public decimal? SecondaryBatteryRange { get; } = ShipDataContainer.SecondaryBatteryUiDataContainer.Secondaries?.Select(x=> x.Range).First();
    public List<decimal> SecondaryBatteryReload { get; } = ShipDataContainer.SecondaryBatteryUiDataContainer.Secondaries?.Select(x=> x.Reload).ToList() ?? new();
    public List<decimal> SecondaryBatteryRoF { get; } = ShipDataContainer.SecondaryBatteryUiDataContainer.Secondaries?.Select(x=> x.RoF).ToList() ?? new();
    public List<string> SecondaryBatteryDpm { get; } = ShipDataContainer.SecondaryBatteryUiDataContainer.Secondaries?.Select(x=> x.TheoreticalDpm).ToList() ?? new();
    public List<decimal> SecondaryBatteryFpm { get; } = ShipDataContainer.SecondaryBatteryUiDataContainer.Secondaries?.Select(x=> x.PotentialFpm).ToList() ?? new();
    public decimal? SecondaryBatterySigma { get; } = ShipDataContainer.SecondaryBatteryUiDataContainer.Secondaries?.Select(x=> x.Sigma).First();

    //Secondary shells
    public string? SecondaryType { get; } = ShipDataContainer.SecondaryBatteryUiDataContainer.Secondaries?.Select(x=> x.Shell).Select(x => x?.Type).First();
    public List<decimal> SecondaryMass { get; } = ShipDataContainer.SecondaryBatteryUiDataContainer.Secondaries?.Select(x=> x.Shell).Select(x => x?.Mass ?? 0).ToList() ?? new();
    public List<decimal> SecondaryDamage { get; } = ShipDataContainer.SecondaryBatteryUiDataContainer.Secondaries?.Select(x=> x.Shell).Select(x => x?.Damage ?? 0).ToList() ?? new();
    public List<decimal> SecondarySplashRadius { get; } = ShipDataContainer.SecondaryBatteryUiDataContainer.Secondaries?.Select(x=> x.Shell).Select(x => x?.SplashRadius ?? 0).ToList() ?? new();
    public List<decimal> SecondarySplashDamage { get; } = ShipDataContainer.SecondaryBatteryUiDataContainer.Secondaries?.Select(x=> x.Shell).Select(x => x?.SplashDmg ?? 0).ToList() ?? new();
    public List<int> SecondaryPenetration { get; } = ShipDataContainer.SecondaryBatteryUiDataContainer.Secondaries?.Select(x=> x.Shell).Select(x => x?.Penetration ?? 0).ToList() ?? new();
    public List<decimal> SecondarySpeed { get; } = ShipDataContainer.SecondaryBatteryUiDataContainer.Secondaries?.Select(x=> x.Shell).Select(x => x?.ShellVelocity ?? 0).ToList() ?? new();
    public List<decimal> SecondaryAirDrag { get; } = ShipDataContainer.SecondaryBatteryUiDataContainer.Secondaries?.Select(x=> x.Shell).Select(x => x?.AirDrag ?? 0).ToList() ?? new();
    public List<decimal> SecondaryHeShellFireChance { get; } = ShipDataContainer.SecondaryBatteryUiDataContainer.Secondaries?.Select(x=> x.Shell).Select(x => x?.ShellFireChance ?? 0).ToList() ?? new();
    public List<decimal> SecondaryHeBlastRadius { get; } = ShipDataContainer.SecondaryBatteryUiDataContainer.Secondaries?.Select(x=> x.Shell).Select(x => x?.ExplosionRadius ?? 0).ToList() ?? new();
    public List<decimal> SecondaryHeBlastPenetration { get; } = ShipDataContainer.SecondaryBatteryUiDataContainer.Secondaries?.Select(x=> x.Shell).Select(x => x?.SplashCoeff ?? 0).ToList() ?? new();
    public List<decimal> SecondarySapOvermatch { get; } = ShipDataContainer.SecondaryBatteryUiDataContainer.Secondaries?.Select(x=> x.Shell).Select(x => x?.Overmatch ?? 0).ToList() ?? new();
    public List<string> SecondarySapRicochet { get; } = ShipDataContainer.SecondaryBatteryUiDataContainer.Secondaries?.Select(x=> x.Shell).Select(x => x?.RicochetAngles ?? default!).ToList() ?? new();

    //AA
    public decimal? LongAaRange { get; } = ShipDataContainer.AntiAirDataContainer?.LongRangeAura?.Range;
    public decimal? MediumAaRange { get; } = ShipDataContainer.AntiAirDataContainer?.MediumRangeAura?.Range;
    public decimal? ShortAaRange { get; } = ShipDataContainer.AntiAirDataContainer?.ShortRangeAura?.Range;
    public decimal? LongAaConstantDamage { get; } = ShipDataContainer.AntiAirDataContainer?.LongRangeAura?.ConstantDamage;
    public decimal? MediumAaConstantDamage { get; } = ShipDataContainer.AntiAirDataContainer?.MediumRangeAura?.ConstantDamage;
    public decimal? ShortAaConstantDamage { get; } = ShipDataContainer.AntiAirDataContainer?.ShortRangeAura?.ConstantDamage;
    public decimal? LongAaHitChance { get; } = ShipDataContainer.AntiAirDataContainer?.LongRangeAura?.HitChance;
    public decimal? MediumAaHitChance { get; } = ShipDataContainer.AntiAirDataContainer?.MediumRangeAura?.HitChance;
    public decimal? ShortAaHitChance { get; } = ShipDataContainer.AntiAirDataContainer?.ShortRangeAura?.HitChance;
    public string? Flak { get; } = ShipDataContainer.AntiAirDataContainer?.LongRangeAura?.Flak;
    public decimal? FlakDamage { get; } = ShipDataContainer.AntiAirDataContainer?.LongRangeAura?.FlakDamage;

    //ASW
    public string? AswDcType { get; } = ShipDataContainer.AswAirstrikeDataContainer is not null ? nameof(Translation.ShipStats_AswAirstrike) : ShipDataContainer.DepthChargeLauncherDataContainer is not null ? "DepthCharge" : null;
    public decimal? AswRange { get; } = ShipDataContainer.AswAirstrikeDataContainer?.MaximumDistance;
    public decimal? AswMaxDropLength { get; } = ShipDataContainer.AswAirstrikeDataContainer?.MaximumFlightDistance;
    public decimal? AswDcReload { get; } = ShipDataContainer.DepthChargeLauncherDataContainer?.Reload ?? ShipDataContainer.AswAirstrikeDataContainer?.ReloadTime;
    public int? AswDcUses { get; } = ShipDataContainer.DepthChargeLauncherDataContainer?.NumberOfUses ?? ShipDataContainer.AswAirstrikeDataContainer?.NumberOfUses;
    public int? AswPlanesInSquadron { get; } = ShipDataContainer.AswAirstrikeDataContainer?.NumberDuringAttack;
    public int? AswBombsPerPlane { get; } = ShipDataContainer.AswAirstrikeDataContainer?.BombsPerPlane;
    public decimal? DcPerAttack { get; } = ShipDataContainer.DepthChargeLauncherDataContainer?.BombsPerCharge;
    public decimal? DcDamage { get; } = ShipDataContainer.DepthChargeLauncherDataContainer?.DepthCharge?.Damage ?? (ShipDataContainer.AswAirstrikeDataContainer?.Weapon as DepthChargeDataContainer)?.Damage;
    public decimal? DcFireChance { get; } = ShipDataContainer.DepthChargeLauncherDataContainer?.DepthCharge?.FireChance ?? (ShipDataContainer.AswAirstrikeDataContainer?.Weapon as DepthChargeDataContainer)?.FireChance;
    public decimal? DcFloodingChance { get; } = ShipDataContainer.DepthChargeLauncherDataContainer?.DepthCharge?.FloodingChance ?? (ShipDataContainer.AswAirstrikeDataContainer?.Weapon as DepthChargeDataContainer)?.FloodingChance;
    public decimal? DcSplashRadius { get; } = ShipDataContainer.DepthChargeLauncherDataContainer?.DepthCharge?.DcSplashRadius ?? (ShipDataContainer.AswAirstrikeDataContainer?.Weapon as DepthChargeDataContainer)?.DcSplashRadius;
    public string? DcSinkSpeed { get; } = ShipDataContainer.DepthChargeLauncherDataContainer?.DepthCharge?.SinkSpeed ?? (ShipDataContainer.AswAirstrikeDataContainer?.Weapon as DepthChargeDataContainer)?.SinkSpeed;
    public string? DcDetonationTimer { get; } = ShipDataContainer.DepthChargeLauncherDataContainer?.DepthCharge?.DetonationTimer ?? (ShipDataContainer.AswAirstrikeDataContainer?.Weapon as DepthChargeDataContainer)?.DetonationTimer;
    public string? DcDetonationDepth { get; } = ShipDataContainer.DepthChargeLauncherDataContainer?.DepthCharge?.DetonationDepth ?? (ShipDataContainer.AswAirstrikeDataContainer?.Weapon as DepthChargeDataContainer)?.DetonationDepth;

    //AirStrike
    public decimal? AirStrikePlanesHp { get; } = ShipDataContainer.AirstrikeDataContainer?.PlaneHp;
    public decimal? AirStrikeRange { get; } = ShipDataContainer.AirstrikeDataContainer?.MaximumDistance;
    public decimal? AirStrikeMaxDropLength { get; } = ShipDataContainer.AirstrikeDataContainer?.MaximumFlightDistance;
    public decimal? AirStrikeReload { get; } = ShipDataContainer.AirstrikeDataContainer?.ReloadTime;
    public int? AirStrikeUses { get; } = ShipDataContainer.AirstrikeDataContainer?.NumberOfUses;
    public int? AirStrikePlanesInSquadron { get; } = ShipDataContainer.AirstrikeDataContainer?.NumberDuringAttack;
    public int? AirStrikeBombsPerPlane { get; } = ShipDataContainer.AirstrikeDataContainer?.BombsPerPlane;
    public string? AirStrikeBombType { get; } = (ShipDataContainer.AirstrikeDataContainer?.Weapon as BombDataContainer)?.BombType;
    public decimal? AirStrikeDamage { get; } = (ShipDataContainer.AirstrikeDataContainer?.Weapon as BombDataContainer)?.Damage;
    public decimal? AirStrikeFireChance { get; } = (ShipDataContainer.AirstrikeDataContainer?.Weapon as BombDataContainer)?.FireChance;
    public decimal? AirStrikeSplashRadius { get; } = (ShipDataContainer.AirstrikeDataContainer?.Weapon as BombDataContainer)?.SplashRadius;
    public decimal? AirStrikeSplashDamage { get; } = (ShipDataContainer.AirstrikeDataContainer?.Weapon as BombDataContainer)?.SplashDmg;
    public int? AirStrikePenetration { get; } = (ShipDataContainer.AirstrikeDataContainer?.Weapon as BombDataContainer)?.Penetration;
    public decimal? AirStrikeBlastRadius { get; } = (ShipDataContainer.AirstrikeDataContainer?.Weapon as BombDataContainer)?.ExplosionRadius;
    public decimal? AirStrikeBlastDamage { get; } = (ShipDataContainer.AirstrikeDataContainer?.Weapon as BombDataContainer)?.SplashCoeff;

    //Maneuverability
    public decimal ManeuverabilityMaxSpeed { get; } = ShipDataContainer.ManeuverabilityDataContainer.ManeuverabilityMaxSpeed != 0 ? ShipDataContainer.ManeuverabilityDataContainer.ManeuverabilityMaxSpeed : ShipDataContainer.ManeuverabilityDataContainer.ManeuverabilitySubsMaxSpeedOnSurface;
    public decimal ManeuverabilityMaxSpeedAtPeriscopeDepth { get; } = ShipDataContainer.ManeuverabilityDataContainer.ManeuverabilitySubsMaxSpeedAtPeriscope;
    public decimal ManeuverabilityMaxSpeedAtMaxDepth { get; } = ShipDataContainer.ManeuverabilityDataContainer.ManeuverabilitySubsMaxSpeedAtMaxDepth;
    public decimal ManeuverabilityRudderShiftTime { get; } = ShipDataContainer.ManeuverabilityDataContainer.ManeuverabilityRudderShiftTime;
    public decimal ManeuverabilityTurningCircle { get; } = ShipDataContainer.ManeuverabilityDataContainer.ManeuverabilityTurningCircle;
    public decimal ManeuverabilityTimeToFullAhead { get; } = ShipDataContainer.ManeuverabilityDataContainer.ForwardMaxSpeedTime;
    public decimal ManeuverabilityTimeToFullReverse { get; } = ShipDataContainer.ManeuverabilityDataContainer.ReverseMaxSpeedTime;
    public decimal ManeuverabilityRudderProtection { get; } = ShipDataContainer.ManeuverabilityDataContainer.RudderBlastProtection;
    public decimal ManeuverabilityEngineProtection { get; } = ShipDataContainer.ManeuverabilityDataContainer.EngineBlastProtection;

    //Concealment
    public decimal ConcealmentFromShipsBase { get; } = ShipDataContainer.ConcealmentDataContainer.ConcealmentBySea;
    public decimal ConcealmentFromShipsOnFire { get; } = ShipDataContainer.ConcealmentDataContainer.ConcealmentBySeaFire;
    public decimal ConcealmentFromShipsSmokeFiring { get; } = ShipDataContainer.ConcealmentDataContainer.ConcealmentBySeaFiringSmoke;
    public decimal ConcealmentFromPlanesBase { get; } = ShipDataContainer.ConcealmentDataContainer.ConcealmentByAir;
    public decimal ConcealmentFromPlanesOnFire { get; } = ShipDataContainer.ConcealmentDataContainer.ConcealmentByAirFire;
    public decimal ConcealmentFromSubsPeriscopeDepth { get; } = ShipDataContainer.ConcealmentDataContainer.ConcealmentBySubPeriscope;

    //Survivability
    public decimal SurvivabilityShipHp { get; } = ShipDataContainer.SurvivabilityDataContainer.HitPoints;
    public decimal SurvivabilityFireMaxAmount { get; } = ShipDataContainer.SurvivabilityDataContainer.FireAmount;
    public decimal SurvivabilityFireDuration { get; } = ShipDataContainer.SurvivabilityDataContainer.FireDuration;
    public decimal SurvivabilityFireDps { get; } = ShipDataContainer.SurvivabilityDataContainer.FireDPS;
    public decimal SurvivabilityFireMaxDmg { get; } = ShipDataContainer.SurvivabilityDataContainer.FireTotalDamage;
    public decimal SurvivabilityFireChanceReduction { get; } = ShipDataContainer.SurvivabilityDataContainer.FireReduction;
    public decimal SurvivabilityFloodingMaxAmount { get; } = ShipDataContainer.SurvivabilityDataContainer.FloodAmount;
    public decimal SurvivabilityFloodingDuration { get; } = ShipDataContainer.SurvivabilityDataContainer.FloodDuration;
    public decimal SurvivabilityFloodingDps { get; } = ShipDataContainer.SurvivabilityDataContainer.FloodDPS;
    public decimal SurvivabilityFloodingMaxDmg { get; } = ShipDataContainer.SurvivabilityDataContainer.FloodTotalDamage;
    public decimal SurvivabilityFloodingTorpedoProtection { get; } = ShipDataContainer.SurvivabilityDataContainer.FloodTorpedoProtection;

    //Sonar
    public decimal? SonarReloadTime { get; } = ShipDataContainer.PingerGunDataContainer?.Reload;
    public decimal? SonarTraverseSpeed { get; } = ShipDataContainer.PingerGunDataContainer?.TraverseSpeed;
    public decimal? SonarTurnTime { get; } = ShipDataContainer.PingerGunDataContainer?.TurnTime;
    public decimal? SonarRange { get; } = ShipDataContainer.PingerGunDataContainer?.Range;
    public decimal? SonarWidth { get; } = ShipDataContainer.PingerGunDataContainer?.PingWidth;
    public decimal? SonarSpeed { get; } = ShipDataContainer.PingerGunDataContainer?.PingSpeed;
    public decimal? SonarFirstPingDuration { get; } = ShipDataContainer.PingerGunDataContainer?.FirstPingDuration;
    public decimal? SonarSecondPingDuration { get; } = ShipDataContainer.PingerGunDataContainer?.SecondPingDuration;

    //RocketPlanes
    public List<string> RocketPlanesType { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Rocket.ToString())).Select(x => x.PlaneVariant).ToList() ?? new();
    public List<int> RocketPlanesInSquadron { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Rocket.ToString())).Select(x => x.NumberInSquad).ToList() ?? new();
    public List<int> RocketPlanesPerAttack { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Rocket.ToString())).Select(x => x.NumberDuringAttack).ToList() ?? new();
    public List<int> RocketPlanesOnDeck { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Rocket.ToString())).Select(x => x.MaxNumberOnDeck).ToList() ?? new();
    public List<decimal> RocketPlanesRestorationTime { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Rocket.ToString())).Select(x => x.RestorationTime).ToList() ?? new();
    public List<decimal> RocketPlanesCruisingSpeed { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Rocket.ToString())).Select(x => x.CruisingSpeed).ToList() ?? new();
    public List<decimal> RocketPlanesMaxSpeed { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Rocket.ToString())).Select(x => x.MaxSpeed).ToList() ?? new();
    public List<decimal> RocketPlanesMinSpeed { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Rocket.ToString())).Select(x => x.MinSpeed).ToList() ?? new();
    public List<decimal> RocketPlanesEngineBoostDuration { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Rocket.ToString())).Select(x => x.MaxEngineBoostDuration).ToList() ?? new();
    public List<decimal> RocketPlanesInitialBoostDuration { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Rocket.ToString())).Select(x => x.JatoDuration).ToList() ?? new();
    public List<decimal> RocketPlanesInitialBoostValue { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Rocket.ToString())).Select(x => x.JatoSpeedMultiplier).ToList() ?? new();
    public List<int> RocketPlanesPlaneHp { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Rocket.ToString())).Select(x => x.PlaneHp).ToList() ?? new();
    public List<int> RocketPlanesSquadronHp { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Rocket.ToString())).Select(x => x.SquadronHp).ToList() ?? new();
    public List<int> RocketPlanesAttackGroupHp { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Rocket.ToString())).Select(x => x.AttackGroupHp).ToList() ?? new();
    public List<int> RocketPlanesDamageDuringAttack { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Rocket.ToString())).Select(x => x.DamageTakenDuringAttack).ToList() ?? new();
    public List<int> RocketPlanesWeaponsPerPlane { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Rocket.ToString())).Select(x => x.AmmoPerAttack).ToList() ?? new();
    public List<decimal> RocketPlanesPreparationTime { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Rocket.ToString())).Select(x => x.PreparationTime).ToList() ?? new();
    public List<decimal> RocketPlanesAimingTime { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Rocket.ToString())).Select(x => x.AimingTime).ToList() ?? new();
    public List<decimal> RocketPlanesTimeToFullyAimed { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Rocket.ToString())).Select(x => x.TimeToFullyAimed).ToList() ?? new();
    public List<decimal> RocketPlanesPostAttackInvulnerability { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Rocket.ToString())).Select(x => x.PostAttackInvulnerabilityDuration).ToList() ?? new();
    public List<decimal> RocketPlanesAttackCooldown { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Rocket.ToString())).Select(x => x.AttackCd).ToList() ?? new();
    public List<decimal> RocketPlanesConcealment { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Rocket.ToString())).Select(x => x.ConcealmentFromShips).ToList() ?? new();
    public List<decimal> RocketPlanesSpotting { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Rocket.ToString())).Select(x => x.MaxViewDistance).ToList() ?? new();
    public List<string> RocketPlanesAreaChangeAiming { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Rocket.ToString())).Select(x => x.AimingRateMoving).ToList() ?? new();
    public List<string> RocketPlanesAreaChangePreparation { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Rocket.ToString())).Select(x => x.AimingPreparationRateMoving).ToList() ?? new();

    public List<string> RocketPlanesWeaponType { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Rocket.ToString())).Select(x => x.Weapon).Select(x => (x as RocketDataContainer)?.RocketType ?? default!).ToList() ?? new();
    public List<decimal> RocketPlanesWeaponDamage { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Rocket.ToString())).Select(x => x.Weapon).Select(x => (x as RocketDataContainer)?.Damage ?? 0).ToList() ?? new();
    public List<decimal> RocketPlanesWeaponSplashRadius { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Rocket.ToString())).Select(x => x.Weapon).Select(x => (x as RocketDataContainer)?.SplashRadius ?? 0).ToList() ?? new();
    public List<decimal> RocketPlanesWeaponSplashDamage { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Rocket.ToString())).Select(x => x.Weapon).Select(x => (x as RocketDataContainer)?.SplashDmg ?? 0).ToList() ?? new();
    public List<int> RocketPlanesWeaponPenetration { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Rocket.ToString())).Select(x => x.Weapon).Select(x => (x as RocketDataContainer)?.Penetration ?? 0).ToList() ?? new();
    public List<decimal> RocketPlanesWeaponFireChance { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Rocket.ToString())).Select(x => x.Weapon).Select(x => (x as RocketDataContainer)?.FireChance ?? 0).ToList() ?? new();
    public List<decimal> RocketPlanesWeaponFuseTimer { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Rocket.ToString())).Select(x => x.Weapon).Select(x => (x as RocketDataContainer)?.FuseTimer ?? 0).ToList() ?? new();
    public List<int> RocketPlanesWeaponArmingThreshold { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Rocket.ToString())).Select(x => x.Weapon).Select(x => (x as RocketDataContainer)?.ArmingThreshold ?? 0).ToList() ?? new();
    public List<string> RocketPlanesWeaponRicochetAngles { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Rocket.ToString())).Select(x => x.Weapon).Select(x => (x as RocketDataContainer)?.RicochetAngles ?? default!).ToList() ?? new();
    public List<decimal> RocketPlanesWeaponBlastRadius { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Rocket.ToString())).Select(x => x.Weapon).Select(x => (x as RocketDataContainer)?.ExplosionRadius ?? 0).ToList() ?? new();
    public List<decimal> RocketPlanesWeaponBlastPenetration { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Rocket.ToString())).Select(x => x.Weapon).Select(x => (x as RocketDataContainer)?.SplashCoeff ?? 0).ToList() ?? new();

    //TorpedoBombers
    public List<string> TorpedoBombersType { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Torpedo.ToString())).Select(x => x.PlaneVariant).ToList() ?? new();
    public List<int> TorpedoBombersInSquadron { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Torpedo.ToString())).Select(x => x.NumberInSquad).ToList() ?? new();
    public List<int> TorpedoBombersPerAttack { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Torpedo.ToString())).Select(x => x.NumberDuringAttack).ToList() ?? new();
    public List<int> TorpedoBombersOnDeck { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Torpedo.ToString())).Select(x => x.MaxNumberOnDeck).ToList() ?? new();
    public List<decimal> TorpedoBombersRestorationTime { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Torpedo.ToString())).Select(x => x.RestorationTime).ToList() ?? new();
    public List<decimal> TorpedoBombersCruisingSpeed { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Torpedo.ToString())).Select(x => x.CruisingSpeed).ToList() ?? new();
    public List<decimal> TorpedoBombersMaxSpeed { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Torpedo.ToString())).Select(x => x.MaxSpeed).ToList() ?? new();
    public List<decimal> TorpedoBombersMinSpeed { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Torpedo.ToString())).Select(x => x.MinSpeed).ToList() ?? new();
    public List<decimal> TorpedoBombersEngineBoostDuration { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Torpedo.ToString())).Select(x => x.MaxEngineBoostDuration).ToList() ?? new();
    public List<decimal> TorpedoBombersInitialBoostDuration { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Torpedo.ToString())).Select(x => x.JatoDuration).ToList() ?? new();
    public List<decimal> TorpedoBombersInitialBoostValue { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Torpedo.ToString())).Select(x => x.JatoSpeedMultiplier).ToList() ?? new();
    public List<int> TorpedoBombersPlaneHp { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Torpedo.ToString())).Select(x => x.PlaneHp).ToList() ?? new();
    public List<int> TorpedoBombersSquadronHp { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Torpedo.ToString())).Select(x => x.SquadronHp).ToList() ?? new();
    public List<int> TorpedoBombersAttackGroupHp { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Torpedo.ToString())).Select(x => x.AttackGroupHp).ToList() ?? new();
    public List<int> TorpedoBombersDamageDuringAttack { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Torpedo.ToString())).Select(x => x.DamageTakenDuringAttack).ToList() ?? new();
    public List<int> TorpedoBombersWeaponsPerPlane { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Torpedo.ToString())).Select(x => x.AmmoPerAttack).ToList() ?? new();
    public List<decimal> TorpedoBombersPreparationTime { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Torpedo.ToString())).Select(x => x.PreparationTime).ToList() ?? new();
    public List<decimal> TorpedoBombersAimingTime { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Torpedo.ToString())).Select(x => x.AimingTime).ToList() ?? new();
    public List<decimal> TorpedoBombersTimeToFullyAimed { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Torpedo.ToString())).Select(x => x.TimeToFullyAimed).ToList() ?? new();
    public List<decimal> TorpedoBombersPostAttackInvulnerability { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Torpedo.ToString())).Select(x => x.PostAttackInvulnerabilityDuration).ToList() ?? new();
    public List<decimal> TorpedoBombersAttackCooldown { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Torpedo.ToString())).Select(x => x.AttackCd).ToList() ?? new();
    public List<decimal> TorpedoBombersConcealment { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Torpedo.ToString())).Select(x => x.ConcealmentFromShips).ToList() ?? new();
    public List<decimal> TorpedoBombersSpotting { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Torpedo.ToString())).Select(x => x.MaxViewDistance).ToList() ?? new();
    public List<string> TorpedoBombersAreaChangeAiming { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Torpedo.ToString())).Select(x => x.AimingRateMoving).ToList() ?? new();
    public List<string> TorpedoBombersAreaChangePreparation { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Torpedo.ToString())).Select(x => x.AimingPreparationRateMoving).ToList() ?? new();

    //Aerial torps
    public List<string> TorpedoBombersWeaponType { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Torpedo.ToString())).Select(x => x.Weapon).Select(x => (x as TorpedoDataContainer)?.TorpedoType ?? default!).ToList() ?? new();
    public List<decimal> TorpedoBombersWeaponDamage { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Torpedo.ToString())).Select(x => x.Weapon).Select(x => (x as TorpedoDataContainer)?.Damage ?? 0).ToList() ?? new();
    public List<decimal> TorpedoBombersWeaponRange { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Torpedo.ToString())).Select(x => x.Weapon).Select(x => (x as TorpedoDataContainer)?.Range ?? 0).ToList() ?? new();
    public List<decimal> TorpedoBombersWeaponSpeed { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Torpedo.ToString())).Select(x => x.Weapon).Select(x => (x as TorpedoDataContainer)?.Speed ?? 0).ToList() ?? new();
    public List<decimal> TorpedoBombersWeaponDetectabilityRange { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Torpedo.ToString())).Select(x => x.Weapon).Select(x => (x as TorpedoDataContainer)?.Detectability ?? 0).ToList() ?? new();
    public List<int> TorpedoBombersWeaponArmingDistance { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Torpedo.ToString())).Select(x => x.Weapon).Select(x => (x as TorpedoDataContainer)?.ArmingDistance ?? 0).ToList() ?? new();
    public List<decimal> TorpedoBombersWeaponReactionTime { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Torpedo.ToString())).Select(x => x.Weapon).Select(x => (x as TorpedoDataContainer)?.ReactionTime ?? 0).ToList() ?? new();
    public List<decimal> TorpedoBombersWeaponFloodingChance { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Torpedo.ToString())).Select(x => x.Weapon).Select(x => (x as TorpedoDataContainer)?.FloodingChance ?? 0).ToList() ?? new();
    public List<decimal> TorpedoBombersWeaponBlastRadius { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Torpedo.ToString())).Select(x => x.Weapon).Select(x => (x as TorpedoDataContainer)?.ExplosionRadius ?? 0).ToList() ?? new();
    public List<decimal> TorpedoBombersWeaponBlastPenetration { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Torpedo.ToString())).Select(x => x.Weapon).Select(x => (x as TorpedoDataContainer)?.SplashCoeff ?? 0).ToList() ?? new();
    public List<List<ShipClass>?> TorpedoBombersWeaponCanHit { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Torpedo.ToString())).Select(x => x.Weapon).Select(x => (x as TorpedoDataContainer)?.CanHitClasses).ToList() ?? new();

    //Bombers
    public List<string> BombersType { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Bomb.ToString()) || x.WeaponType.Equals(ProjectileType.SkipBomb.ToString())).Select(x => x.PlaneVariant).ToList() ?? new();
    public List<int> BombersInSquadron { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Bomb.ToString()) || x.WeaponType.Equals(ProjectileType.SkipBomb.ToString())).Select(x => x.NumberInSquad).ToList() ?? new();
    public List<int> BombersPerAttack { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Bomb.ToString()) || x.WeaponType.Equals(ProjectileType.SkipBomb.ToString())).Select(x => x.NumberDuringAttack).ToList() ?? new();
    public List<int> BombersOnDeck { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Bomb.ToString()) || x.WeaponType.Equals(ProjectileType.SkipBomb.ToString())).Select(x => x.MaxNumberOnDeck).ToList() ?? new();
    public List<decimal> BombersRestorationTime { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Bomb.ToString()) || x.WeaponType.Equals(ProjectileType.SkipBomb.ToString())).Select(x => x.RestorationTime).ToList() ?? new();
    public List<decimal> BombersCruisingSpeed { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Bomb.ToString()) || x.WeaponType.Equals(ProjectileType.SkipBomb.ToString())).Select(x => x.CruisingSpeed).ToList() ?? new();
    public List<decimal> BombersMaxSpeed { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Bomb.ToString()) || x.WeaponType.Equals(ProjectileType.SkipBomb.ToString())).Select(x => x.MaxSpeed).ToList() ?? new();
    public List<decimal> BombersMinSpeed { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Bomb.ToString()) || x.WeaponType.Equals(ProjectileType.SkipBomb.ToString())).Select(x => x.MinSpeed).ToList() ?? new();
    public List<decimal> BombersEngineBoostDuration { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Bomb.ToString()) || x.WeaponType.Equals(ProjectileType.SkipBomb.ToString())).Select(x => x.MaxEngineBoostDuration).ToList() ?? new();
    public List<decimal> BombersInitialBoostDuration { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Bomb.ToString()) || x.WeaponType.Equals(ProjectileType.SkipBomb.ToString())).Select(x => x.JatoDuration).ToList() ?? new();
    public List<decimal> BombersInitialBoostValue { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Bomb.ToString()) || x.WeaponType.Equals(ProjectileType.SkipBomb.ToString())).Select(x => x.JatoSpeedMultiplier).ToList() ?? new();
    public List<int> BombersPlaneHp { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Bomb.ToString()) || x.WeaponType.Equals(ProjectileType.SkipBomb.ToString())).Select(x => x.PlaneHp).ToList() ?? new();
    public List<int> BombersSquadronHp { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Bomb.ToString()) || x.WeaponType.Equals(ProjectileType.SkipBomb.ToString())).Select(x => x.SquadronHp).ToList() ?? new();
    public List<int> BombersAttackGroupHp { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Bomb.ToString()) || x.WeaponType.Equals(ProjectileType.SkipBomb.ToString())).Select(x => x.AttackGroupHp).ToList() ?? new();
    public List<int> BombersDamageDuringAttack { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Bomb.ToString()) || x.WeaponType.Equals(ProjectileType.SkipBomb.ToString())).Select(x => x.DamageTakenDuringAttack).ToList() ?? new();
    public List<int> BombersWeaponsPerPlane { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Bomb.ToString()) || x.WeaponType.Equals(ProjectileType.SkipBomb.ToString())).Select(x => x.AmmoPerAttack).ToList() ?? new();
    public List<decimal> BombersPreparationTime { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Bomb.ToString()) || x.WeaponType.Equals(ProjectileType.SkipBomb.ToString())).Select(x => x.PreparationTime).ToList() ?? new();
    public List<decimal> BombersAimingTime { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Bomb.ToString()) || x.WeaponType.Equals(ProjectileType.SkipBomb.ToString())).Select(x => x.AimingTime).ToList() ?? new();
    public List<decimal> BombersTimeToFullyAimed { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Bomb.ToString()) || x.WeaponType.Equals(ProjectileType.SkipBomb.ToString())).Select(x => x.TimeToFullyAimed).ToList() ?? new();
    public List<decimal> BombersPostAttackInvulnerability { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Bomb.ToString()) || x.WeaponType.Equals(ProjectileType.SkipBomb.ToString())).Select(x => x.PostAttackInvulnerabilityDuration).ToList() ?? new();
    public List<decimal> BombersAttackCooldown { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Bomb.ToString()) || x.WeaponType.Equals(ProjectileType.SkipBomb.ToString())).Select(x => x.AttackCd).ToList() ?? new();
    public List<decimal> BombersConcealment { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Bomb.ToString()) || x.WeaponType.Equals(ProjectileType.SkipBomb.ToString())).Select(x => x.ConcealmentFromShips).ToList() ?? new();
    public List<decimal> BombersSpotting { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Bomb.ToString()) || x.WeaponType.Equals(ProjectileType.SkipBomb.ToString())).Select(x => x.MaxViewDistance).ToList() ?? new();
    public List<string> BombersAreaChangeAiming { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Bomb.ToString()) || x.WeaponType.Equals(ProjectileType.SkipBomb.ToString())).Select(x => x.AimingRateMoving).ToList() ?? new();
    public List<string> BombersAreaChangePreparation { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Bomb.ToString()) || x.WeaponType.Equals(ProjectileType.SkipBomb.ToString())).Select(x => x.AimingPreparationRateMoving).ToList() ?? new();
    public List<int> BombersInnerEllipse { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Bomb.ToString()) || x.WeaponType.Equals(ProjectileType.SkipBomb.ToString())).Select(x => x.InnerBombPercentage).ToList() ?? new();

    //Bombs
    public List<string> BombersWeaponType { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Bomb.ToString()) || x.WeaponType.Equals(ProjectileType.SkipBomb.ToString())).Select(x => x.WeaponType).ToList() ?? new();
    public List<string> BombersWeaponBombType { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Bomb.ToString()) || x.WeaponType.Equals(ProjectileType.SkipBomb.ToString())).Select(x => x.Weapon).Select(x => (x as BombDataContainer)?.BombType ?? default!).ToList() ?? new();
    public List<decimal> BombersWeaponDamage { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Bomb.ToString()) || x.WeaponType.Equals(ProjectileType.SkipBomb.ToString())).Select(x => x.Weapon).Select(x => (x as BombDataContainer)?.Damage ?? 0).ToList() ?? new();
    public List<decimal> BombersWeaponSplashRadius { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Bomb.ToString()) || x.WeaponType.Equals(ProjectileType.SkipBomb.ToString())).Select(x => x.Weapon).Select(x => (x as BombDataContainer)?.SplashRadius ?? 0).ToList() ?? new();
    public List<decimal> BombersWeaponSplashDamage { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Bomb.ToString()) || x.WeaponType.Equals(ProjectileType.SkipBomb.ToString())).Select(x => x.Weapon).Select(x => (x as BombDataContainer)?.SplashDmg ?? 0).ToList() ?? new();
    public List<int> BombersWeaponPenetration { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Bomb.ToString()) || x.WeaponType.Equals(ProjectileType.SkipBomb.ToString())).Select(x => x.Weapon).Select(x => (x as BombDataContainer)?.Penetration ?? 0).ToList() ?? new();
    public List<decimal> BombersWeaponFireChance { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Bomb.ToString()) || x.WeaponType.Equals(ProjectileType.SkipBomb.ToString())).Select(x => x.Weapon).Select(x => (x as BombDataContainer)?.FireChance ?? 0).ToList() ?? new();
    public List<decimal> BombersWeaponBlastRadius { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Bomb.ToString()) || x.WeaponType.Equals(ProjectileType.SkipBomb.ToString())).Select(x => x.Weapon).Select(x => (x as BombDataContainer)?.ExplosionRadius ?? 0).ToList() ?? new();
    public List<decimal> BombersWeaponBlastPenetration { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Bomb.ToString()) || x.WeaponType.Equals(ProjectileType.SkipBomb.ToString())).Select(x => x.Weapon).Select(x => (x as BombDataContainer)?.SplashCoeff ?? 0).ToList() ?? new();
    public List<decimal> BombersWeaponFuseTimer { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Bomb.ToString()) || x.WeaponType.Equals(ProjectileType.SkipBomb.ToString())).Select(x => x.Weapon).Select(x => (x as BombDataContainer)?.FuseTimer ?? 0).ToList() ?? new();
    public List<int> BombersWeaponArmingThreshold { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Bomb.ToString()) || x.WeaponType.Equals(ProjectileType.SkipBomb.ToString())).Select(x => x.Weapon).Select(x => (x as BombDataContainer)?.ArmingThreshold ?? 0).ToList() ?? new();
    public List<string> BombersWeaponRicochetAngles { get; } = ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Bomb.ToString()) || x.WeaponType.Equals(ProjectileType.SkipBomb.ToString())).Select(x => x.Weapon).Select(x => (x as BombDataContainer)?.RicochetAngles ?? default!).ToList() ?? new();

}
