using System;
using WoWsShipBuilder.Core.BuildCreator;
using WoWsShipBuilder.Core.DataContainers;
using WoWsShipBuilder.DataStructures;

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

    public string ShipImage { get; } //TODO implement after porting methods into Helper class

    //Main battery

    public string MainBatteryCaliber { get; } = ShipDataContainer.MainBatteryDataContainer is null ? "N/A" : $"{ShipDataContainer.MainBatteryDataContainer.GunCaliber}";

    public string MainBatteryBarrelCount { get; } = ShipDataContainer.MainBatteryDataContainer is null ? "N/A" : $"{ShipDataContainer.MainBatteryDataContainer.BarrelsCount}";

    public string MainBatteryRange { get; } = ShipDataContainer.MainBatteryDataContainer is null ? "N/A" : $"{ShipDataContainer.MainBatteryDataContainer.Range}";

    public string MainBatteryTurnTime { get; } = ShipDataContainer.MainBatteryDataContainer is null ? "N/A" : $"{ShipDataContainer.MainBatteryDataContainer.TurnTime}";

    public string MainBatteryTraverseSpeed { get; } = ShipDataContainer.MainBatteryDataContainer is null ? "N/A" : $"{ShipDataContainer.MainBatteryDataContainer.TraverseSpeed}";

    public string MainBatteryReload { get; } = ShipDataContainer.MainBatteryDataContainer is null ? "N/A" : $"{ShipDataContainer.MainBatteryDataContainer.Reload}";

    public string MainBatteryRoF { get; } = ShipDataContainer.MainBatteryDataContainer is null ? "N/A" : $"{ShipDataContainer.MainBatteryDataContainer.RoF}";

    public string MainBatteryHeDpm { get; } = ShipDataContainer.MainBatteryDataContainer is null ? "N/A" : ShipDataContainer.MainBatteryDataContainer.DisplayHeDpm ? $"{ShipDataContainer.MainBatteryDataContainer.TheoreticalHeDpm}" : "N/A";

    public string MainBatterySapDpm { get; } = ShipDataContainer.MainBatteryDataContainer is null ? "N/A" : ShipDataContainer.MainBatteryDataContainer.DisplaySapDpm ? $"{ShipDataContainer.MainBatteryDataContainer.TheoreticalSapDpm}" : "N/A";

    public string MainBatteryApDpm { get; } = ShipDataContainer.MainBatteryDataContainer is null ? "N/A" : ShipDataContainer.MainBatteryDataContainer.DisplayApDpm ? $"{ShipDataContainer.MainBatteryDataContainer.TheoreticalApDpm}" : "N/A";

    public string MainBatteryHeSalvo { get; } = ShipDataContainer.MainBatteryDataContainer is null ? "N/A" : ShipDataContainer.MainBatteryDataContainer.DisplayHeDpm ? $"{ShipDataContainer.MainBatteryDataContainer.HeSalvo}" : "N/A";

    public string MainBatterySapSalvo { get; } = ShipDataContainer.MainBatteryDataContainer is null ? "N/A" : ShipDataContainer.MainBatteryDataContainer.DisplaySapDpm ? $"{ShipDataContainer.MainBatteryDataContainer.SapSalvo}" : "N/A";

    public string MainBatteryApSalvo { get; } = ShipDataContainer.MainBatteryDataContainer is null ? "N/A" : ShipDataContainer.MainBatteryDataContainer.DisplayApDpm ? $"{ShipDataContainer.MainBatteryDataContainer.ApSalvo}" : "N/A";

    public string MainBatteryFpm { get; } = ShipDataContainer.MainBatteryDataContainer is null ? "N/A" : ShipDataContainer.MainBatteryDataContainer.DisplayHeDpm ? $"{ShipDataContainer.MainBatteryDataContainer.PotentialFpm}" : "N/A";

    public string MainBatterySigma { get; } = ShipDataContainer.MainBatteryDataContainer is null ? "N/A" : $"{ShipDataContainer.MainBatteryDataContainer.Sigma}";

}
