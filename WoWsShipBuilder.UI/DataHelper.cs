using System.Collections.Generic;
using System.Linq;
using WoWsShipBuilder.Core.Data;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.DataUI;
using WoWsShipBuilder.Core.Localization;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.ViewModels.ShipVm;

namespace WoWsShipBuilder.UI
{
    public static class DataHelper
    {
        public static readonly IReadOnlyList<Modernization> PlaceholderBaseList = new List<Modernization> { UpgradePanelViewModelBase.PlaceholderModernization };

        public static ILocalizer DemoLocalizer { get; } = new DemoLocalizerImpl();

        public static (Ship Ship, List<ShipUpgrade> Configuration) LoadPreviewShip(ShipClass shipClass, int tier, Nation nation)
        {
            DesktopAppDataService.PreviewInstance.LoadNationFiles(nation);

            var ship = DesktopAppDataService.PreviewInstance.ReadLocalJsonData<Ship>(nation, ServerType.Live).Result!
                .Select(entry => entry.Value)
                .First(ship => ship.ShipClass == shipClass && ship.Tier == tier);

            var configuration = ShipModuleHelper.GroupAndSortUpgrades(ship.ShipUpgradeInfo.ShipUpgrades)
                .Select(entry => entry.Value.FirstOrDefault())
                .Where(item => item != null)
                .Cast<ShipUpgrade>()
                .ToList();

            return (ship, configuration);
        }

        public static ShipSummary GetPreviewShipSummary(ShipClass shipClass, int tier, Nation nation)
        {
            var ship = LoadPreviewShip(shipClass, tier, nation);
            return DesktopAppDataService.PreviewInstance.GetShipSummaryList(ServerType.Live).Result.First(summary => summary.Index == ship.Ship.Index);
        }

        public static MainViewModelParams GetPreviewViewModelParams(ShipClass shipClass, int tier, Nation nation)
        {
            return new(LoadPreviewShip(shipClass, tier, nation).Ship, GetPreviewShipSummary(shipClass, tier, nation));
        }

        public static TurretModule GetPreviewTurretModule(ShipClass shipClass, int tier, Nation nation)
        {
            var testData = LoadPreviewShip(shipClass, tier, nation);
            var currentShipStats = ShipDataContainer.FromShip(testData.Ship, testData.Configuration, new(), DesktopAppDataService.Instance, DemoLocalizer).Result;
            return currentShipStats.MainBatteryDataContainer!.OriginalMainBatteryData;
        }

        public class DemoLocalizerImpl : ILocalizer
        {
            public LocalizationResult this[string key] => GetGameLocalization(key);

            public LocalizationResult GetGameLocalization(string key) => new(true, key);

            public LocalizationResult GetAppLocalization(string key) => new(true, key);
        }
    }
}
