using System;
using System.IO;
using WoWsShipBuilder.Common.Builds;
using WoWsShipBuilder.Common.DataContainers;
using WoWsShipBuilder.Common.Infrastructure;
using WoWsShipBuilder.Common.Infrastructure.DataTransfer;
using WoWsShipBuilder.DataStructures.Ship;
using AirstrikeDataContainer = WoWsShipBuilder.Common.DataContainers.AirstrikeDataContainer;
using BombDataContainer = WoWsShipBuilder.Common.DataContainers.BombDataContainer;
using ConcealmentDataContainer = WoWsShipBuilder.Common.DataContainers.ConcealmentDataContainer;
using MainBatteryDataContainer = WoWsShipBuilder.Common.DataContainers.MainBatteryDataContainer;
using PingerGunDataContainer = WoWsShipBuilder.Common.DataContainers.PingerGunDataContainer;
using ShellDataContainer = WoWsShipBuilder.Common.DataContainers.ShellDataContainer;
using SurvivabilityDataContainer = WoWsShipBuilder.Common.DataContainers.SurvivabilityDataContainer;
using TorpedoArmamentDataContainer = WoWsShipBuilder.Common.DataContainers.TorpedoArmamentDataContainer;

namespace WoWsShipBuilder.ViewModels.Helper.GridData;

/// <summary>
/// Main container class for data in the ship comparison.
/// Where possible, regular DataContainer classes are used. If this is not possible due to custom transformations, the data is stored in a dedicated GridDataWrapper class.
/// </summary>
public sealed class GridDataWrapper
{
    public GridDataWrapper(ShipBuildContainer shipBuildContainer)
    {
        ShipBuildContainer = shipBuildContainer;

        Ship = shipBuildContainer.Ship;
        ShipDataContainer = shipBuildContainer.ShipDataContainer ?? throw new InvalidDataException("ShipDataContainer is null. ShipDataContainer must not be null.");
        Build = shipBuildContainer.Build;
        Id = shipBuildContainer.Id;

        ShipIndex = shipBuildContainer.Ship.Index;
        ShipNation = shipBuildContainer.Ship.ShipNation;
        ShipClass = shipBuildContainer.Ship.ShipClass;
        ShipCategory = shipBuildContainer.Ship.ShipCategory;
        ShipTier = shipBuildContainer.Ship.Tier;
        BuildName = shipBuildContainer.Build?.BuildName;

        MainBattery = ShipBuildContainer.ShipDataContainer?.MainBatteryDataContainer;
        HeShell = MainBattery?.ShellData.FirstOrDefault(x => x.Type.Equals($"ArmamentType_{ShellType.HE.ShellTypeToString()}"));
        ApShell = MainBattery?.ShellData.FirstOrDefault(x => x.Type.Equals($"ArmamentType_{ShellType.AP.ShellTypeToString()}"));
        SapShell = MainBattery?.ShellData.FirstOrDefault(x => x.Type.Equals($"ArmamentType_{ShellType.SAP.ShellTypeToString()}"));

        var torpedoArmament = shipBuildContainer.ShipDataContainer.TorpedoArmamentDataContainer;
        TorpedoLauncher = torpedoArmament;
        Torpedo = new(torpedoArmament);
        Secondary = new(shipBuildContainer.ShipDataContainer.SecondaryBatteryUiDataContainer.Secondaries);
        AntiAirArmament = shipBuildContainer.ShipDataContainer.AntiAirDataContainer;
        Asw = new(shipBuildContainer.ShipDataContainer.AswAirstrikeDataContainer, shipBuildContainer.ShipDataContainer.DepthChargeLauncherDataContainer);

        AirStrike = shipBuildContainer.ShipDataContainer.AirstrikeDataContainer;
        AirStrikeWeapon = AirStrike?.Weapon as BombDataContainer;

        ManeuverabilityData = new(shipBuildContainer.ShipDataContainer.ManeuverabilityDataContainer);
        Concealment = shipBuildContainer.ShipDataContainer.ConcealmentDataContainer;
        Survivability = shipBuildContainer.ShipDataContainer.SurvivabilityDataContainer;
        Sonar = shipBuildContainer.ShipDataContainer.PingerGunDataContainer;

        RocketPlanes = new(shipBuildContainer.ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Rocket.ProjectileTypeToString())).ToList());
        TorpedoBombers = new(shipBuildContainer.ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Torpedo.ProjectileTypeToString())).ToList());
        Bombers = new(shipBuildContainer.ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Bomb.ProjectileTypeToString()) || x.WeaponType.Equals(ProjectileType.SkipBomb.ProjectileTypeToString())).ToList());
    }

    public ShipBuildContainer ShipBuildContainer { get; }

    public Ship Ship { get; }

    public ShipDataContainer ShipDataContainer { get; }

    public Build? Build { get; }

    public Guid Id { get; }

    public string ShipIndex { get; }

    public Nation ShipNation { get; }

    public ShipClass ShipClass { get; }

    public ShipCategory ShipCategory { get; }

    public int ShipTier { get; }

    public string? BuildName { get; }

    public MainBatteryDataContainer? MainBattery { get; }

    public ShellDataContainer? HeShell { get; }

    public ShellDataContainer? ApShell { get; }

    public ShellDataContainer? SapShell { get; }

    public TorpedoArmamentDataContainer? TorpedoLauncher { get; }

    public TorpedoGridDataWrapper Torpedo { get; }

    public SecondaryGridDataWrapper Secondary { get; }

    public AntiAirDataContainer? AntiAirArmament { get; }

    public AswGridDataWrapper Asw { get; }

    public AirstrikeDataContainer? AirStrike { get; }

    public BombDataContainer? AirStrikeWeapon { get; }

    public ManeuverabilityGridDataWrapper ManeuverabilityData { get; }

    public ConcealmentDataContainer Concealment { get; }

    public SurvivabilityDataContainer Survivability { get; }

    public PingerGunDataContainer? Sonar { get; }

    public RocketPlaneGridDataWrapper RocketPlanes { get; }

    public TorpedoBomberGridDataWrapper TorpedoBombers { get; }

    public BomberGridDataWrapper Bombers { get; }
}
