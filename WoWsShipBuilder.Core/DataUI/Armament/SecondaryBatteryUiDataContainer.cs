using System.Collections.Generic;
using System.Threading.Tasks;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Core.DataUI
{
    public record SecondaryBatteryUiDataContainer(List<SecondaryBatteryDataContainer>? Secondaries)
    {
        private string expanderKey = default!;

        public bool IsExpanderOpen
        {
            get => ShipDataContainer.ExpanderStateMapper[expanderKey];
            set => ShipDataContainer.ExpanderStateMapper[expanderKey] = value;
        }

        public static async Task<SecondaryBatteryUiDataContainer> FromShip(Ship ship, IEnumerable<ShipUpgrade> shipConfiguration, List<(string, float)> modifiers, IAppDataService appDataService)
        {
            var uiContainer = new SecondaryBatteryUiDataContainer(await SecondaryBatteryDataContainer.FromShip(ship, shipConfiguration, modifiers, appDataService))
            {
                expanderKey = $"{ship.Index}_SEC",
            };
            if (!ShipDataContainer.ExpanderStateMapper.ContainsKey(uiContainer.expanderKey))
            {
                ShipDataContainer.ExpanderStateMapper[uiContainer.expanderKey] = true;
            }

            return uiContainer;
        }
    }
}
