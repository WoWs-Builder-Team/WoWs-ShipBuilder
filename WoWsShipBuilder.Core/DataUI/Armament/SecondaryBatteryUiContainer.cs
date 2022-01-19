using System.Collections.Generic;
using Newtonsoft.Json;
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

        public static SecondaryBatteryUiContainer FromShip(Ship ship, List<ShipUpgrade> shipConfiguration, List<(string, float)> modifiers)
        {
            var uiContainer = new SecondaryBatteryUiContainer(SecondaryBatteryUI.FromShip(ship, shipConfiguration, modifiers));
            uiContainer.expanderKey = $"{ship.Index}_SEC";
            if (!ShipUI.ExpanderStateMapper.ContainsKey(uiContainer.expanderKey))
            {
                ShipUI.ExpanderStateMapper[uiContainer.expanderKey] = true;
            }

            return uiContainer;
        }
    }
}
