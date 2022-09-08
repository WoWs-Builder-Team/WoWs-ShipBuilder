using System;
using WoWsShipBuilder.Core.BuildCreator;
using WoWsShipBuilder.Core.DataContainers;
using WoWsShipBuilder.ViewModels.Other;

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
    public string? BuildName { get; } = Build?.BuildName;

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

}
