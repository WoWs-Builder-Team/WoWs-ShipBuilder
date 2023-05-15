using Microsoft.Extensions.Logging;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Aircraft;
using WoWsShipBuilder.DataStructures.Captain;
using WoWsShipBuilder.DataStructures.Consumable;
using WoWsShipBuilder.DataStructures.Exterior;
using WoWsShipBuilder.DataStructures.Module;
using WoWsShipBuilder.DataStructures.Projectile;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.DataStructures.Upgrade;

namespace WoWsShipBuilder.Common.Infrastructure.GameData;

public static class GameDataHelper
{
    public static Nation GetNationFromIndex(string index)
    {
        if (index.Length < 7)
        {
            Logging.Logger.LogError("Invalid index, received value {Index}", index);
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

    public static char ClassToIndex(ShipClass shipClass)
    {
        return shipClass switch
        {
            ShipClass.Submarine => 'S',
            ShipClass.Destroyer => 'D',
            ShipClass.Cruiser => 'C',
            ShipClass.Battleship => 'B',
            ShipClass.AirCarrier => 'A',
            _ => throw new ArgumentOutOfRangeException(nameof(shipClass), shipClass, null),
        };
    }

    public static ShipClass IndexToClass(string index)
    {
        return index[0] switch
        {
            'S' => ShipClass.Submarine,
            'D' => ShipClass.Destroyer,
            'C' => ShipClass.Cruiser,
            'B' => ShipClass.Battleship,
            'A' => ShipClass.AirCarrier,
            _ => throw new ArgumentOutOfRangeException(nameof(index), index, null),
        };
    }

    public static Nation IndexToNation(string index)
    {
        return index[0] switch
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

    public static char NationToIndex(Nation nation)
    {
        return nation switch
        {
            Nation.Usa =>'A',
            Nation.UnitedKingdom =>'B',
            Nation.France =>'F',
            Nation.Germany =>'G',
            Nation.Netherlands =>'H',
            Nation.Italy =>'I',
            Nation.Japan =>'J',
            Nation.Russia =>'R',
            Nation.Spain =>'S',
            Nation.Commonwealth =>'U',
            Nation.PanAmerica =>'V',
            Nation.Europe =>'W',
            Nation.PanAsia =>'Z',
            _ => 'C',
        };
    }

    public static string CategoryToIndex(ShipCategory category)
    {
        return category switch
        {
            ShipCategory.TechTree => "Tt",
            ShipCategory.Premium => "P",
            ShipCategory.Special => "S",
            ShipCategory.TestShip => "Ts",
            ShipCategory.SuperShip => "Ss",
            _ => throw new ArgumentOutOfRangeException(nameof(category), category, null),
        };
    }

    public static ShipCategory IndexToCategory(string index)
    {
        return index switch
        {
            "Tt" => ShipCategory.TechTree,
            "P" => ShipCategory.Premium,
            "S" => ShipCategory.Special,
            "Ts" => ShipCategory.TestShip,
            "Ss" => ShipCategory.SuperShip,
            _ => throw new ArgumentOutOfRangeException(nameof(index), index, null),
        };
    }
}
