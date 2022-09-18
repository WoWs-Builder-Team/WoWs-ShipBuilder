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
    public List<string?> TorpedoFullSalvoDamage { get; } = new() {ShipDataContainer.TorpedoArmamentDataContainer?.TorpFullSalvoDmg, ShipDataContainer.TorpedoArmamentDataContainer?.AltTorpFullSalvoDmg};
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
}
