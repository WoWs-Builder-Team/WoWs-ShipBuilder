using System.Collections.Immutable;
using WoWsShipBuilder.DataStructures.Modifiers;
using WoWsShipBuilder.DataStructures.Ship;

namespace WoWsShipBuilder.Features.DataContainers;

public class ShipDataContainer(string index)
{
    public string Index { get; } = index;

    public SurvivabilityDataContainer SurvivabilityDataContainer { get; set; } = default!;

    public MainBatteryDataContainer? MainBatteryDataContainer { get; set; }

    public SecondaryBatteryUiDataContainer SecondaryBatteryUiDataContainer { get; set; } = default!;

    public PingerGunDataContainer? PingerGunDataContainer { get; set; }

    public TorpedoArmamentDataContainer? TorpedoArmamentDataContainer { get; set; }

    public AirstrikeDataContainer? AirstrikeDataContainer { get; set; }

    public AirstrikeDataContainer? AswAirstrikeDataContainer { get; set; }

    public DepthChargesLauncherDataContainer? DepthChargeLauncherDataContainer { get; set; }

    public ImmutableList<CvAircraftDataContainer>? CvAircraftDataContainer { get; set; }

    public ManeuverabilityDataContainer ManeuverabilityDataContainer { get; set; } = default!;

    public ConcealmentDataContainer ConcealmentDataContainer { get; set; } = default!;

    public AntiAirDataContainer? AntiAirDataContainer { get; set; }

    public ImmutableList<object> SecondColumnContent { get; set; } = ImmutableList<object>.Empty;

    public SpecialAbilityDataContainer? SpecialAbilityDataContainer { get; set; }

    public static ShipDataContainer CreateFromShip(Ship ship, ImmutableList<ShipUpgrade> shipConfiguration, ImmutableList<Modifier> modifiers)
    {
        var shipDataContainer = new ShipDataContainer(ship.Index)
        {
            // Main weapons
            MainBatteryDataContainer = MainBatteryDataContainer.FromShip(ship, shipConfiguration, modifiers),
            TorpedoArmamentDataContainer = TorpedoArmamentDataContainer.FromShip(ship, shipConfiguration, modifiers),
            CvAircraftDataContainer = DataContainers.CvAircraftDataContainer.FromShip(ship, shipConfiguration, modifiers)?.ToImmutableList(),
            PingerGunDataContainer = PingerGunDataContainer.FromShip(ship, shipConfiguration, modifiers),

            // Secondary weapons
            SecondaryBatteryUiDataContainer = SecondaryBatteryUiDataContainer.FromShip(ship, shipConfiguration, modifiers),
            AntiAirDataContainer = AntiAirDataContainer.FromShip(ship, shipConfiguration, modifiers),
            AirstrikeDataContainer = AirstrikeDataContainer.FromShip(ship, modifiers, false),
            AswAirstrikeDataContainer = AirstrikeDataContainer.FromShip(ship, modifiers, true),
            DepthChargeLauncherDataContainer = DepthChargesLauncherDataContainer.FromShip(ship, shipConfiguration, modifiers),

            // Misc
            ManeuverabilityDataContainer = ManeuverabilityDataContainer.FromShip(ship, shipConfiguration, modifiers),
            ConcealmentDataContainer = ConcealmentDataContainer.FromShip(ship, shipConfiguration, modifiers),
            SurvivabilityDataContainer = SurvivabilityDataContainer.FromShip(ship, shipConfiguration, modifiers),
            SpecialAbilityDataContainer = SpecialAbilityDataContainer.FromShip(ship, shipConfiguration),
        };

        var secondColumnContent = new List<object?>
            {
                shipDataContainer.AntiAirDataContainer,
                shipDataContainer.AirstrikeDataContainer,
                shipDataContainer.AswAirstrikeDataContainer,
                shipDataContainer.DepthChargeLauncherDataContainer,
            }.Where(item => item != null)
            .Cast<object>()
            .ToList();

        if (!shipDataContainer.SecondaryBatteryUiDataContainer.Secondaries.IsEmpty)
        {
            secondColumnContent.Insert(0, shipDataContainer.SecondaryBatteryUiDataContainer);
        }

        shipDataContainer.SecondColumnContent = secondColumnContent.ToImmutableList();

        return shipDataContainer;
    }
}
