using System;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Aircraft;
using WoWsShipBuilder.DataStructures.Captain;
using WoWsShipBuilder.DataStructures.Consumable;
using WoWsShipBuilder.DataStructures.Exterior;
using WoWsShipBuilder.DataStructures.Module;
using WoWsShipBuilder.DataStructures.Projectile;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.DataStructures.Upgrade;

namespace WoWsShipBuilder.Core.Utility;

public static class GameDataHelper
{
    public static Nation GetNationFromIndex(string index)
    {
        if (index.Length < 7)
        {
            Logging.Logger.Error("Invalid index, received value {}.", index);
            return Nation.Common;
        }

        return index[1] switch
        {
            'A' => Nation.Usa,
            'B' => Nation.UnitedKingdom,
            'F' => Nation.France,
            'G' => Nation.Germany,
            'H' => Nation.Netherlands,
            'I' => Nation.Italy,
            'J' => Nation.Japan,
            'R' => Nation.Russia,
            'S' => Nation.Spain,
            'U' => Nation.Commonwealth,
            'V' => Nation.PanAmerica,
            'W' => Nation.Europe,
            'Z' => Nation.PanAsia,
            _ => Nation.Common,
        };
    }

    public static string GetNationString(Nation nation)
    {
        return nation switch
        {
            Nation.PanAmerica => "Pan_America",
            Nation.PanAsia => "Pan_Asia",
            Nation.UnitedKingdom => "United_Kingdom",
            Nation.Usa => "USA",
            _ => nation.ToString() ?? throw new InvalidOperationException("Unable to retrieve enum name."),
        };
    }

    public static string GetCategoryString<T>()
    {
        return typeof(T) switch
        {
            var consumableType when consumableType == typeof(Consumable) => "Ability",
            var aircraftType when aircraftType == typeof(Aircraft) => "Aircraft",
            var crewType when crewType == typeof(Captain) => "Crew",
            var exteriorType when exteriorType == typeof(Exterior) => "Exterior",
            var gunType when gunType == typeof(Gun) => "Gun",
            var modernizationType when modernizationType == typeof(Modernization) => "Modernization",
            var projectileType when typeof(Projectile).IsAssignableFrom(projectileType) => "Projectile",
            var shipType when shipType == typeof(Ship) => "Ship",
            var moduleType when moduleType == typeof(Module) => "Unit",
            _ => throw new InvalidOperationException(),
        };
    }
}
