using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using Avalonia.Controls;
using Microsoft.Extensions.Logging.Abstractions;
using WoWsShipBuilder.Common.DataContainers;
using WoWsShipBuilder.Common.Features.Builds;
using WoWsShipBuilder.Common.Features.ShipStats;
using WoWsShipBuilder.Common.Features.ShipStats.ViewModels;
using WoWsShipBuilder.Common.Infrastructure;
using WoWsShipBuilder.Common.Infrastructure.Data;
using WoWsShipBuilder.Common.Infrastructure.DataTransfer;
using WoWsShipBuilder.Common.Infrastructure.GameData;
using WoWsShipBuilder.Common.Infrastructure.Localization;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Aircraft;
using WoWsShipBuilder.DataStructures.Consumable;
using WoWsShipBuilder.DataStructures.Projectile;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.DataStructures.Upgrade;
using WoWsShipBuilder.UI.Services;
using WoWsShipBuilder.UI.ViewModels;
using WoWsShipBuilder.UI.ViewModels.ShipVm;

namespace WoWsShipBuilder.UI;

// TODO: fix this entire mess
public static class DesignDataHelper
{
    public static readonly IReadOnlyList<Modernization> PlaceholderBaseList = new List<Modernization> { UpgradePanelViewModelBase.PlaceholderModernization };

    public static ILocalizer DemoLocalizer { get; } = new DemoLocalizerImpl();

    private static DesktopAppDataService PreviewAppDataService { get; } = new(new FileSystem(), new DesktopDataService(new FileSystem()), new());

    public static StartMenuViewModel StartMenuViewModel { get; } = new(new FileSystem(), new NavigationService(), new AvaloniaClipboardService(), PreviewAppDataService, PreviewAppDataService, DemoLocalizer, new());

    public static SettingsWindowViewModel SettingsWindowViewModel { get; } = new(new FileSystem(), new AvaloniaClipboardService(), PreviewAppDataService);

    public static ShipWindowViewModel ShipWindowViewModel { get; } = new(new NavigationService(), new AvaloniaClipboardService(), DemoLocalizer, NullLogger<ShipWindowViewModel>.Instance, GetPreviewViewModelParams(ShipClass.Cruiser, 10, Nation.Usa));

    public static Build CreateTestBuild(string name = "test-build") => new(name, null!, Nation.Common, null!, null!, null!, null!, null!, null!);

    public static (Ship Ship, List<ShipUpgrade> Configuration) LoadPreviewShip(ShipClass shipClass, int tier, Nation nation, ServerType serverType = ServerType.Dev1)
    {
        ThrowIfNotDesignMode();

        LoadNationFiles(nation, serverType);

        var ship = ReadLocalJsonData<Ship>(nation, ServerType.Live)!
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
        ThrowIfNotDesignMode();
        var ship = LoadPreviewShip(shipClass, tier, nation);
        var dataService = new DesktopDataService(new FileSystem());
        string fileName = dataService.CombinePaths(PreviewAppDataService.GetDataPath(ServerType.Live), "Summary", "Common.json");
        var summaryTask = PreviewAppDataService.DeserializeFile<List<ShipSummary>>(fileName);
        return summaryTask.Result!.First(summary => summary.Index == ship.Ship.Index);
    }

    public static ShipViewModelParams GetPreviewViewModelParams(ShipClass shipClass, int tier, Nation nation)
    {
        ThrowIfNotDesignMode();
        return new(LoadPreviewShip(shipClass, tier, nation).Ship, GetPreviewShipSummary(shipClass, tier, nation));
    }

    public static TurretModule GetPreviewTurretModule(ShipClass shipClass, int tier, Nation nation)
    {
        ThrowIfNotDesignMode();

        var testData = LoadPreviewShip(shipClass, tier, nation);
        var currentShipStats = ShipDataContainer.CreateFromShip(testData.Ship, testData.Configuration, new());
        return currentShipStats.MainBatteryDataContainer!.OriginalMainBatteryData;
    }

    private static void LoadNationFiles(Nation nation, ServerType serverType)
    {
        var newEntries = ReadLocalJsonData<Ship>(nation, serverType)!;
        foreach ((string key, var value) in newEntries)
        {
            AppData.ShipDictionary[key] = value;
        }

        AppData.ProjectileCache.SetIfNotNull(nation, ReadLocalJsonData<Projectile>(nation, serverType));
        AppData.AircraftCache.SetIfNotNull(nation, ReadLocalJsonData<Aircraft>(nation, serverType));
        AppData.ConsumableList = ReadLocalJsonData<Consumable>(Nation.Common, serverType)!;
        AppData.ModernizationCache = ReadLocalJsonData<Modernization>(Nation.Common, serverType)!;
    }

    private static Dictionary<string, T>? ReadLocalJsonData<T>(Nation nation, ServerType serverType)
    {
        string categoryString = GameDataHelper.GetCategoryString<T>();
        string nationString = GameDataHelper.GetNationString(nation);
        var dataService = new DesktopDataService(new FileSystem());
        string fileName = dataService.CombinePaths(PreviewAppDataService.GetDataPath(serverType), categoryString, $"{nationString}.json");
        return dataService.Load<Dictionary<string, T>>(fileName);
    }

    private static void ThrowIfNotDesignMode()
    {
        if (!Design.IsDesignMode)
        {
            throw new InvalidOperationException("This method must not be used outside of design mode");
        }
    }

    public class DemoLocalizerImpl : ILocalizer
    {
        public LocalizationResult this[string key] => GetGameLocalization(key);

        public LocalizationResult GetGameLocalization(string key) => new(true, key);

        public LocalizationResult GetGameLocalization(string key, CultureDetails language) => new(true, key);

        public LocalizationResult GetAppLocalization(string key) => new(true, key);

        public LocalizationResult GetAppLocalization(string key, CultureDetails language) => new(true, key);
    }
}
