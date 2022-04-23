using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WoWsShipBuilder.Core.Localization;
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
            get => ShipDataContainer.ExpanderStateMapper[expanderKey];
            set => ShipDataContainer.ExpanderStateMapper[expanderKey] = value;
        }

        public static async Task<SecondaryBatteryUiContainer> FromShip(Ship ship, List<ShipUpgrade> shipConfiguration, List<(string, float)> modifiers, IAppDataService appDataService, ILocalizer localizer)
        {
            var uiContainer = new SecondaryBatteryUiContainer(await SecondaryBatteryUI.FromShip(ship, shipConfiguration, modifiers, appDataService, localizer))
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
