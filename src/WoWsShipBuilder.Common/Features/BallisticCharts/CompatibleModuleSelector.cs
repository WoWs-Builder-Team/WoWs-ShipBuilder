using System.Collections.Immutable;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.Infrastructure.GameData;

namespace WoWsShipBuilder.Features.BallisticCharts;

/// <summary>
/// Resolves the hull and artillery a set of selected shells can be fired from.
/// </summary>
public static class CompatibleModuleSelector
{
    /// <summary>
    /// Finds the hull and artillery the selected shells can be fired from.
    /// The modules of the current configuration are kept whenever they are compatible, so selecting a shell does not
    /// silently discard the module selection of the user. When a module has to be replaced, the most upgraded
    /// compatible one wins.
    /// </summary>
    /// <param name="ship">The ship the shells belong to.</param>
    /// <param name="selectedShells">The currently selected shells.</param>
    /// <param name="shipConfiguration">The modules currently configured for the ship.</param>
    /// <returns>The names of the compatible hull and artillery components, or <see langword="null"/> if no combination supports all selected shells.</returns>
    public static (string Hull, string Artillery)? FindCompatibleModules(Ship ship, ICollection<string> selectedShells, List<ShipUpgrade> shipConfiguration)
    {
        if (selectedShells.Count == 0)
        {
            return null;
        }

        var currentHulls = GetConfiguredComponents(shipConfiguration, ComponentType.Hull);
        var currentArtillery = GetConfiguredComponents(shipConfiguration, ComponentType.Artillery);
        var sortedUpgrades = ShipModuleHelper.GroupAndSortUpgrades(ship.ShipUpgradeInfo.ShipUpgrades);

        var candidates = GetCompatibleCombos(ship, selectedShells)
            .SelectMany(combo => combo.Value.Select(artillery => (Hull: combo.Key, Artillery: artillery)))
            .OrderByDescending(candidate => currentHulls.Contains(candidate.Hull))
            .ThenByDescending(candidate => currentArtillery.Contains(candidate.Artillery))
            .ThenByDescending(candidate => GetUpgradeRank(sortedUpgrades, ComponentType.Hull, candidate.Hull))
            .ThenByDescending(candidate => GetUpgradeRank(sortedUpgrades, ComponentType.Artillery, candidate.Artillery))
            .ThenBy(candidate => candidate.Hull, StringComparer.Ordinal)
            .ThenBy(candidate => candidate.Artillery, StringComparer.Ordinal)
            .ToList();

        return candidates.Count > 0 ? candidates[0] : null;
    }

    /// <summary>
    /// Gets the hull and artillery combinations that can fire every selected shell, as a map of hull component name to compatible artillery component names.
    /// </summary>
    private static Dictionary<string, List<string>> GetCompatibleCombos(Ship ship, ICollection<string> selectedShells)
    {
        List<ImmutableDictionary<string, ImmutableList<string>>> combosPerShell = selectedShells.Select(shell => ship.ShellCompatibilities[shell].CompatibleHullArtilleryModulesCombo).ToList();

        return combosPerShell[0]
            .Select(combo => (combo.Key, Artillery: combo.Value.Where(artillery => combosPerShell.TrueForAll(combos => combos.TryGetValue(combo.Key, out var compatible) && compatible.Contains(artillery))).ToList()))
            .Where(combo => combo.Artillery.Count > 0)
            .ToDictionary(combo => combo.Key, combo => combo.Artillery, StringComparer.Ordinal);
    }

    /// <summary>
    /// Gets the position of the module providing the given component within the upgrade path of its type, where a higher value means a more upgraded module.
    /// </summary>
    private static int GetUpgradeRank(Dictionary<ComponentType, List<ShipUpgrade>> sortedUpgrades, ComponentType componentType, string componentName)
    {
        if (!sortedUpgrades.TryGetValue(componentType, out var upgrades))
        {
            return -1;
        }

        return upgrades.FindLastIndex(upgrade => upgrade.Components.TryGetValue(componentType, out var components) && components.Contains(componentName));
    }

    /// <summary>
    /// Gets the component names of the module currently configured for the given component type.
    /// </summary>
    private static ImmutableArray<string> GetConfiguredComponents(List<ShipUpgrade> shipConfiguration, ComponentType componentType)
    {
        var module = shipConfiguration.Find(upgrade => upgrade.UcType == componentType);
        return module is not null && module.Components.TryGetValue(componentType, out var components) ? components : [];
    }
}
