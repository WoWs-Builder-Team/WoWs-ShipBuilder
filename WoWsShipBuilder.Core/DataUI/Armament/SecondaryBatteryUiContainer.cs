using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Core.DataUI
{
    public record SecondaryBatteryUiContainer(List<SecondaryBatteryUI>? Secondaries)
    {
        private string expanderKey = default!;

        [JsonIgnore]
        public bool IsExpanderOpen
        {
            get => ShipUI.ExpanderStateMapper[expanderKey];
            set => ShipUI.ExpanderStateMapper[expanderKey] = value;
        }

        public static async Task<SecondaryBatteryUiContainer> FromShip(Ship ship, List<ShipUpgrade> shipConfiguration, List<(string, float)> modifiers, IAppDataService appDataService)
        {
            var uiContainer = new SecondaryBatteryUiContainer(await SecondaryBatteryUI.FromShip(ship, shipConfiguration, modifiers, appDataService));
            uiContainer.expanderKey = $"{ship.Index}_SEC";
            if (!ShipUI.ExpanderStateMapper.ContainsKey(uiContainer.expanderKey))
            {
                ShipUI.ExpanderStateMapper[uiContainer.expanderKey] = true;
            }

            return uiContainer;
        }
    }
}
