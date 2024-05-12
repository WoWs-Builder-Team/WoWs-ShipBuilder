using System.Collections.Immutable;
using Microsoft.Extensions.Hosting;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.Infrastructure.ApplicationData;
using WoWsShipBuilder.Infrastructure.ApplicationTheme;
using WoWsShipBuilder.Infrastructure.GameData;

namespace WoWsShipBuilder.Infrastructure.Utility;

public static class Helpers
{
    public static string GetIconFromClass(ShipClass shipClass, ShipCategory category, ThemeManager.ThemeVariant currentThemeVariant)
    {
        string path = ClassToPathHelper.GetSvgPathFromClass(shipClass);
        string stroke = ClassToPathHelper.GetColorFromCategory(category, true, currentThemeVariant)[3..];
        string fill = ClassToPathHelper.GetColorFromCategory(category, false, currentThemeVariant)[3..];
        return $"<path fill=\"#{fill}\" stroke=\"#{stroke}\" stroke-width=\"1\"  d=\"{path}\" />";
    }

    public static string GetNationFlag(IHostEnvironment environment, Nation shipNation, string shipIndex)
    {
        string imgName = File.Exists(Path.Combine(environment.ContentRootPath, "wwwroot", "assets", "nation_flags", $"flag_{shipIndex}.png")) ? shipIndex : shipNation.ShipNationToString();
        return $"/_content/WoWsShipBuilder.Common/assets/nation_flags/flag_{imgName}.png";
    }

    public static List<ShipUpgrade> GetStockShipConfiguration(Ship ship)
    {
        return ShipModuleHelper.GroupAndSortUpgrades(ship.ShipUpgradeInfo.ShipUpgrades)
            .OrderBy(entry => entry.Key)
            .Select(entry => entry.Value)
            .Select(module => module[0])
            .ToList();
    }

    public static List<ShipUpgrade> GetFullUpgradedShipConfiguration(Ship ship)
    {
        return ShipModuleHelper.GroupAndSortUpgrades(ship.ShipUpgradeInfo.ShipUpgrades)
            .OrderBy(entry => entry.Key)
            .Select(entry => entry.Value)
            .Select(module => module[^1])
            .ToList();
    }

    public static List<ShipUpgrade> GetShipConfigurationFromBuild(IEnumerable<string> storedData, IEnumerable<ShipUpgrade> upgrades)
    {
        var results = new List<ShipUpgrade>();
        var shipUpgrades = ShipModuleHelper.GroupAndSortUpgrades(upgrades).OrderBy(entry => entry.Key).Select(entry => entry.Value).ToList();
        foreach (List<ShipUpgrade> upgradeList in shipUpgrades)
        {
            results.AddRange(upgradeList.Where(upgrade => storedData.Contains(upgrade.Name.NameToIndex())));
        }

        return results;
    }

    public static bool IsAprilFool()
    {
        // For debugging
        // Return DateTime.Now.Minute > 30
        return DateTime.Now is { Month: 4, Day: 1 };
    }

    public static string GenerateRandomColor()
    {
        return $"#{Random.Shared.Next(0x1000000):X6}";
    }

    public static void InitializeShipSelectorDataStructure()
    {
        var result = AppData.ShipDictionary.GroupBy(x => x.Value.ShipNation)
            .ToImmutableDictionary(
                nationGrouping => nationGrouping.Key,
                nationGrouping => nationGrouping.GroupBy(nationShip => nationShip.Value.ShipCategory)
                    .ToImmutableDictionary(
                        categoryGrouping => categoryGrouping.Key,
                        categoryGrouping => categoryGrouping.GroupBy(categoryShip => categoryShip.Value.ShipClass)
                            .ToImmutableDictionary(
                                shipClassGrouping => shipClassGrouping.Key,
                                shipClassGrouping => shipClassGrouping.GroupBy(shipClassShip => shipClassShip.Value.Tier)
                                    .ToImmutableDictionary(
                                        tierGrouping => tierGrouping.Key,
                                        tierGrouping => tierGrouping.Select(tierShip => tierShip.Value).ToImmutableList()))));

        AppData.FittingToolShipSelectorDataStructure = result;
    }
}
