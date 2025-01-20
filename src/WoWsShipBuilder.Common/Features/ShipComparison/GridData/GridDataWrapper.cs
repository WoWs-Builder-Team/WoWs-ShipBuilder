using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.Features.Builds;
using WoWsShipBuilder.Features.DataContainers;
using WoWsShipBuilder.Features.Navigation;
using WoWsShipBuilder.Infrastructure.GameData;

namespace WoWsShipBuilder.Features.ShipComparison.GridData;

/// <summary>
/// Main container class for data in the ship comparison.
/// Where possible, regular DataContainer classes are used. If this is not possible due to custom transformations, the data is stored in a dedicated GridDataWrapper class.
/// </summary>
public sealed class GridDataWrapper
{
    public GridDataWrapper(ShipBuildContainer shipBuildContainer)
    {
        this.ShipBuildContainer = shipBuildContainer;

        this.Ship = shipBuildContainer.Ship;
        this.ShipDataContainer = shipBuildContainer.ShipDataContainer ?? throw new InvalidDataException("ShipDataContainer is null. ShipDataContainer must not be null.");
        this.Build = shipBuildContainer.Build;
        this.Id = shipBuildContainer.Id;

        this.ShipIndex = shipBuildContainer.Ship.Index;
        this.ShipNation = shipBuildContainer.Ship.ShipNation;
        this.ShipClass = shipBuildContainer.Ship.ShipClass;
        this.ShipCategory = shipBuildContainer.Ship.ShipCategory;
        this.ShipTier = shipBuildContainer.Ship.Tier;
        this.BuildName = shipBuildContainer.Build?.BuildName;

        this.MainBattery = this.ShipBuildContainer.ShipDataContainer?.MainBatteryDataContainer;
        this.HeShell = this.MainBattery?.ShellData.Find(x => x.Type.Equals($"ArmamentType_{ShellType.HE.ShellTypeToString()}", StringComparison.Ordinal));
        this.ApShell = this.MainBattery?.ShellData.Find(x => x.Type.Equals($"ArmamentType_{ShellType.AP.ShellTypeToString()}", StringComparison.Ordinal));
        this.SapShell = this.MainBattery?.ShellData.Find(x => x.Type.Equals($"ArmamentType_{ShellType.SAP.ShellTypeToString()}", StringComparison.Ordinal));

        var torpedoArmament = shipBuildContainer.ShipDataContainer.TorpedoArmamentDataContainer;
        this.TorpedoLauncher = torpedoArmament;
        this.Torpedo = new(torpedoArmament);
        this.Secondary = new(shipBuildContainer.ShipDataContainer.SecondaryBatteryUiDataContainer.Secondaries);
        this.AntiAirArmament = shipBuildContainer.ShipDataContainer.AntiAirDataContainer;
        this.Asw = new(shipBuildContainer.ShipDataContainer.AswAirstrikeDataContainer, shipBuildContainer.ShipDataContainer.DepthChargeLauncherDataContainer);

        this.AirStrike = shipBuildContainer.ShipDataContainer.AirstrikeDataContainer;
        this.AirStrikeWeapon = this.AirStrike?.Weapon as BombDataContainer;

        this.ManeuverabilityData = new(shipBuildContainer.ShipDataContainer.ManeuverabilityDataContainer);
        this.Concealment = shipBuildContainer.ShipDataContainer.ConcealmentDataContainer;
        this.Survivability = shipBuildContainer.ShipDataContainer.SurvivabilityDataContainer;
        this.Sonar = shipBuildContainer.ShipDataContainer.PingerGunDataContainer;

        this.RocketPlanes = new(shipBuildContainer.ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Rocket.ProjectileTypeToString(), StringComparison.Ordinal)).ToList());
        this.TorpedoBombers = new(shipBuildContainer.ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Torpedo.ProjectileTypeToString(), StringComparison.Ordinal)).ToList());
        this.Bombers = new(shipBuildContainer.ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Bomb.ProjectileTypeToString(), StringComparison.Ordinal) || x.WeaponType.Equals(ProjectileType.SkipBomb.ProjectileTypeToString(), StringComparison.Ordinal)).ToList());
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
