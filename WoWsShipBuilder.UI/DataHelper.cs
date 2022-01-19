using System.Collections.Generic;
using System.Linq;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.UI
{
    public static class DataHelper
    {
        public static readonly Modernization PlaceholderModernization = new() { Index = null!, Name = "PlaceholderMod" };

        public static readonly IReadOnlyList<Modernization> PlaceholderBaseList = new List<Modernization> { PlaceholderModernization };

        public static (Ship Ship, List<ShipUpgrade> Configuration) LoadPreviewShip(ShipClass shipClass, int tier, Nation nation)
        {
            AppDataHelper.Instance.LoadNationFiles(nation);

            var ship = AppDataHelper.Instance.ReadLocalJsonData<Ship>(nation, ServerType.Live)!
                .Select(entry => entry.Value)
                .First(ship => ship.ShipClass == shipClass && ship.Tier == tier);

            var configuration = ShipModuleHelper.GroupAndSortUpgrades(ship.ShipUpgradeInfo.ShipUpgrades)
                .Select(entry => entry.Value.FirstOrDefault())
                .Where(item => item != null)
                .Cast<ShipUpgrade>()
                .ToList();

            return (ship, configuration);
        }
    }
}
