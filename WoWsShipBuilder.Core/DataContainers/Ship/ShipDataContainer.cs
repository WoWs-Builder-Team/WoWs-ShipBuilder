using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Core.DataContainers
{
    public record ShipDataContainer(string Index)
    {
        // TODO: check if really necessary
        public static readonly ConcurrentDictionary<string, bool> ExpanderStateMapper = new();

        public SurvivabilityDataContainer SurvivabilityDataContainer { get; set; } = default!;

        public MainBatteryDataContainer? MainBatteryDataContainer { get; set; }

        public SecondaryBatteryUiDataContainer SecondaryBatteryUiDataContainer { get; set; } = default!;

        public PingerGunDataContainer? PingerGunDataContainer { get; set; }

        public TorpedoArmamentDataContainer? TorpedoArmamentDataContainer { get; set; }

        public AirstrikeDataContainer? AirstrikeDataContainer { get; set; }

        public AirstrikeDataContainer? AswAirstrikeDataContainer { get; set; }

        public DepthChargesLauncherDataContainer? DepthChargeLauncherDataContainer { get; set; }

        public List<CvAircraftDataContainer>? CvAircraftDataContainer { get; set; }

        public ManeuverabilityDataContainer ManeuverabilityDataContainer { get; set; } = default!;

        public ConcealmentDataContainer ConcealmentDataContainer { get; set; } = default!;

        public AntiAirDataContainer? AntiAirDataContainer { get; set; }

        public List<object> SecondColumnContent { get; set; } = default!;

        public SpecialAbilityDataContainer? SpecialAbilityDataContainer { get; set; }

        public static async Task<ShipDataContainer> FromShipAsync(Ship ship, List<ShipUpgrade> shipConfiguration, List<(string, float)> modifiers, IAppDataService appDataService)
        {
            var shipDataContainer = new ShipDataContainer(ship.Index)
            {
                // Main weapons
                MainBatteryDataContainer = await MainBatteryDataContainer.FromShip(ship, shipConfiguration, modifiers, appDataService),
                TorpedoArmamentDataContainer = await TorpedoArmamentDataContainer.FromShip(ship, shipConfiguration, modifiers, appDataService),
                CvAircraftDataContainer = await DataContainers.CvAircraftDataContainer.FromShip(ship, shipConfiguration, modifiers, appDataService),
                PingerGunDataContainer = PingerGunDataContainer.FromShip(ship, shipConfiguration, modifiers),

                // Secondary weapons
                SecondaryBatteryUiDataContainer = await SecondaryBatteryUiDataContainer.FromShip(ship, shipConfiguration, modifiers, appDataService),
                AntiAirDataContainer = AntiAirDataContainer.FromShip(ship, shipConfiguration, modifiers),
                AirstrikeDataContainer = await AirstrikeDataContainer.FromShip(ship, modifiers, false, appDataService),
                AswAirstrikeDataContainer = await AirstrikeDataContainer.FromShip(ship, modifiers, true, appDataService),
                DepthChargeLauncherDataContainer = await DepthChargesLauncherDataContainer.FromShip(ship, shipConfiguration, modifiers, appDataService),

                // Misc
                ManeuverabilityDataContainer = ManeuverabilityDataContainer.FromShip(ship, shipConfiguration, modifiers),
                ConcealmentDataContainer = ConcealmentDataContainer.FromShip(ship, shipConfiguration, modifiers),
                SurvivabilityDataContainer = SurvivabilityDataContainer.FromShip(ship, shipConfiguration, modifiers),
                SpecialAbilityDataContainer = SpecialAbilityDataContainer.FromShip(ship, shipConfiguration, modifiers),
            };

            shipDataContainer.SecondColumnContent = new List<object?>
            {
                shipDataContainer.AntiAirDataContainer,
                shipDataContainer.AirstrikeDataContainer,
                shipDataContainer.AswAirstrikeDataContainer,
                shipDataContainer.DepthChargeLauncherDataContainer,
            }.Where(item => item != null)
             .Cast<object>()
             .ToList();

            if (shipDataContainer.SecondaryBatteryUiDataContainer.Secondaries != null)
            {
                shipDataContainer.SecondColumnContent.Insert(0, shipDataContainer.SecondaryBatteryUiDataContainer);
            }

            return shipDataContainer;
        }
    }
}
