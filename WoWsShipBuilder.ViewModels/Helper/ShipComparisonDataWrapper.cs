using System;
using WoWsShipBuilder.Core.BuildCreator;
using WoWsShipBuilder.Core.DataContainers;

namespace WoWsShipBuilder.ViewModels.Helper;

public sealed record ShipComparisonDataWrapper(Ship Ship, ShipDataContainer ShipDataContainer, Build? Build = null)
{
    public ShipComparisonDataWrapper(Ship ship, ShipDataContainer shipDataContainer, Build? build, Guid id)
        : this(ship, shipDataContainer, build)
    {
        Id = id;
    }

    public Guid Id { get; } = Guid.NewGuid();

    //base columns
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
    public decimal? SapMass { get; } = ShipDataContainer.MainBatteryDataContainer?.ShellData.FirstOrDefault(x => x.Type.Equals($"ArmamentType_{ShellType.AP}"))?.Mass;
    public decimal? SapDamage { get; } = ShipDataContainer.MainBatteryDataContainer?.ShellData.FirstOrDefault(x => x.Type.Equals($"ArmamentType_{ShellType.AP}"))?.Damage;
    public decimal? SapSplashRadius { get; } = ShipDataContainer.MainBatteryDataContainer?.ShellData.FirstOrDefault(x => x.Type.Equals($"ArmamentType_{ShellType.AP}"))?.SplashRadius;
    public decimal? SapSplashDamage { get; } = ShipDataContainer.MainBatteryDataContainer?.ShellData.FirstOrDefault(x => x.Type.Equals($"ArmamentType_{ShellType.AP}"))?.SplashDmg;
    public decimal? SapPenetration { get; } = ShipDataContainer.MainBatteryDataContainer?.ShellData.FirstOrDefault(x => x.Type.Equals($"ArmamentType_{ShellType.AP}"))?.Penetration;
    public decimal? SapSpeed { get; } = ShipDataContainer.MainBatteryDataContainer?.ShellData.FirstOrDefault(x => x.Type.Equals($"ArmamentType_{ShellType.AP}"))?.ShellVelocity;
    public decimal? SapAirDrag { get; } = ShipDataContainer.MainBatteryDataContainer?.ShellData.FirstOrDefault(x => x.Type.Equals($"ArmamentType_{ShellType.AP}"))?.AirDrag;
    public decimal? SapOvermatch { get; } = ShipDataContainer.MainBatteryDataContainer?.ShellData.FirstOrDefault(x => x.Type.Equals($"ArmamentType_{ShellType.AP}"))?.Overmatch;
    public string? SapRicochet { get; } = ShipDataContainer.MainBatteryDataContainer?.ShellData.FirstOrDefault(x => x.Type.Equals($"ArmamentType_{ShellType.AP}"))?.RicochetAngles;

    //TorpLaunchers
    public int? TorpedoCount { get; } = ShipDataContainer.TorpedoArmamentDataContainer?.TorpCount;
    public string? TorpedoLayout { get; } = ShipDataContainer.TorpedoArmamentDataContainer?.TorpLayout;
    public decimal? TorpedoTurnTime { get; } = ShipDataContainer.TorpedoArmamentDataContainer?.TurnTime;
    public decimal? TorpedoTraverseSpeed { get; } = ShipDataContainer.TorpedoArmamentDataContainer?.TraverseSpeed;
    public decimal? TorpedoReload { get; } = ShipDataContainer.TorpedoArmamentDataContainer?.Reload;
    public string? TorpedoSpread { get; } = ShipDataContainer.TorpedoArmamentDataContainer?.TorpedoArea;
    public decimal? TorpedoTimeToSwitch { get; } = ShipDataContainer.TorpedoArmamentDataContainer?.TimeToSwitch;

    //Torpedoes
    public IEnumerable<string?> TorpedoFullSalvoDamage { get; } = new[] {ShipDataContainer.TorpedoArmamentDataContainer?.TorpFullSalvoDmg, ShipDataContainer.TorpedoArmamentDataContainer?.AltTorpFullSalvoDmg};
    public List<string>? TorpedoType { get; } = ShipDataContainer.TorpedoArmamentDataContainer?.Torpedoes.Select(x => x.TorpedoType).ToList();
    public List<decimal>? TorpedoDamage { get; } = ShipDataContainer.TorpedoArmamentDataContainer?.Torpedoes.Select(x => x.Damage).ToList();
    public List<decimal>? TorpedoRange { get; } = ShipDataContainer.TorpedoArmamentDataContainer?.Torpedoes.Select(x => x.Range).ToList();
    public List<decimal>? TorpedoSpeed { get; } = ShipDataContainer.TorpedoArmamentDataContainer?.Torpedoes.Select(x => x.Speed).ToList();
    public List<decimal>? TorpedoDetectRange { get; } = ShipDataContainer.TorpedoArmamentDataContainer?.Torpedoes.Select(x => x.Detectability).ToList();
    public List<int>? TorpedoArmingDistance { get; } = ShipDataContainer.TorpedoArmamentDataContainer?.Torpedoes.Select(x => x.ArmingDistance).ToList();
    public List<decimal>? TorpedoReactionTime { get; } = ShipDataContainer.TorpedoArmamentDataContainer?.Torpedoes.Select(x => x.ReactionTime).ToList();
    public List<decimal>? TorpedoFloodingChance { get; } = ShipDataContainer.TorpedoArmamentDataContainer?.Torpedoes.Select(x => x.FloodingChance).ToList();
    public List<decimal>? TorpedoBlastRadius { get; } = ShipDataContainer.TorpedoArmamentDataContainer?.Torpedoes.Select(x => x.ExplosionRadius).ToList();
    public List<decimal>? TorpedoBlastPenetration { get; } = ShipDataContainer.TorpedoArmamentDataContainer?.Torpedoes.Select(x => x.SplashCoeff).ToList();
    public List<List<ShipClass>?>? TorpedoCanHit { get; } = ShipDataContainer.TorpedoArmamentDataContainer?.Torpedoes.Select(x => x.CanHitClasses).ToList();

}
