using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using WoWsShipBuilder.Core.Data;
using WoWsShipBuilder.Core.DataContainers;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilder.Core.Localization;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.ViewModels.ShipVm;

namespace WoWsShipBuilder.UI
{
    public static class DataHelper
    {
        public static readonly IReadOnlyList<Modernization> PlaceholderBaseList = new List<Modernization> { UpgradePanelViewModelBase.PlaceholderModernization };

        public static ILocalizer DemoLocalizer { get; } = new DemoLocalizerImpl();

        public static (Ship Ship, List<ShipUpgrade> Configuration) LoadPreviewShip(ShipClass shipClass, int tier, Nation nation, ServerType serverType = ServerType.Dev1)
        {
            Console.WriteLine("Test1");
            LoadNationFiles(nation, serverType);
            Console.WriteLine("Test2");

            var ship = ReadLocalJsonData<Ship>(nation, ServerType.Live)!
                .Select(entry => entry.Value)
                .First(ship => ship.ShipClass == shipClass && ship.Tier == tier);

            Console.WriteLine("Test3");
            var configuration = ShipModuleHelper.GroupAndSortUpgrades(ship.ShipUpgradeInfo.ShipUpgrades)
                .Select(entry => entry.Value.FirstOrDefault())
                .Where(item => item != null)
                .Cast<ShipUpgrade>()
                .ToList();

            Console.WriteLine("Test4");

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
            var currentShipStats = ShipDataContainer.FromShipAsync(testData.Ship, testData.Configuration, new(), DesktopAppDataService.Instance).Result;
            return currentShipStats.MainBatteryDataContainer!.OriginalMainBatteryData;
        }

        private static void LoadNationFiles(Nation nation, ServerType serverType)
        {
            if (AppData.ShipDictionary?.FirstOrDefault() == null || AppData.ShipDictionary.First().Value.ShipNation != nation)
            {
                AppData.ShipDictionary = ReadLocalJsonData<Ship>(nation, serverType);
            }

            AppData.ProjectileCache.SetIfNotNull(nation, ReadLocalJsonData<Projectile>(nation, serverType));
            AppData.AircraftCache.SetIfNotNull(nation, ReadLocalJsonData<Aircraft>(nation, serverType));
            AppData.ConsumableList ??= ReadLocalJsonData<Consumable>(Nation.Common, serverType);
        }

        private static Dictionary<string, T>? ReadLocalJsonData<T>(Nation nation, ServerType serverType)
        {
            string categoryString = IAppDataService.GetCategoryString<T>();
            string nationString = IAppDataService.GetNationString(nation);
            var dataService = new DesktopDataService(new FileSystem());
            string fileName = dataService.CombinePaths(DesktopAppDataService.PreviewInstance.GetDataPath(serverType), categoryString, $"{nationString}.json");
            return dataService.Load<Dictionary<string, T>>(fileName);
        }

        public class DemoLocalizerImpl : ILocalizer
        {
            public LocalizationResult this[string key] => GetGameLocalization(key);

            public LocalizationResult GetGameLocalization(string key) => new(true, key);

            public LocalizationResult GetAppLocalization(string key) => new(true, key);
        }
    }
}
