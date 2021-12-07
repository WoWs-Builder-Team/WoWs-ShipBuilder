﻿using System.Collections.Generic;
using System.Linq;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.Core.DataProvider
{
    public static class ShipModuleHelper
    {
        public static Dictionary<ComponentType, List<ShipUpgrade>> GroupAndSortUpgrades(List<ShipUpgrade> unsortedUpgrades)
        {
            Dictionary<ComponentType, List<ShipUpgrade>> upgradeDict = unsortedUpgrades
                .GroupBy(upgrade => upgrade.UcType)
                .Select(entry => (Type: entry.Key, Value: entry.OrderBy(item => item, UpgradeComparer.Instance).ToList()))
                .ToDictionary(pair => pair.Type, pair => pair.Value);
            return upgradeDict;
        }

        private class UpgradeComparer : IComparer<ShipUpgrade>
        {
            public static UpgradeComparer Instance { get; } = new();

            public int Compare(ShipUpgrade? firstUpgrade, ShipUpgrade? secondUpgrade)
            {
                if (firstUpgrade != null && secondUpgrade != null)
                {
                    if (firstUpgrade.Prev == secondUpgrade.Prev)
                    {
                        return 0;
                    }

                    if (string.IsNullOrEmpty(firstUpgrade.Prev))
                    {
                        return -1;
                    }

                    if (string.IsNullOrEmpty(secondUpgrade.Prev))
                    {
                        return 1;
                    }

                    if (firstUpgrade.Prev == secondUpgrade.Name)
                    {
                        return 1;
                    }

                    if (secondUpgrade.Prev == firstUpgrade.Name)
                    {
                        return -1;
                    }
                }

                return 0;
            }
        }
    }
}
