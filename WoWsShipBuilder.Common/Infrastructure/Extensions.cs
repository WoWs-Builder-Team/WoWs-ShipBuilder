using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Common.Infrastructure;

public static class Extensions
{
    // ReSharper disable once InconsistentNaming
    public static void AddDict<T, S>(this Dictionary<T, S?> thisDict, Dictionary<T, S?> otherDict) where T : struct where S : class
    {
        foreach ((T key, S? value) in otherDict)
        {
            thisDict.Add(key, value);
        }
    }

    // ReSharper disable once InconsistentNaming
    public static void AddRange<T, S>(this Dictionary<T, S> thisDict, IEnumerable<KeyValuePair<T, S>> otherDict) where T : struct where S : class
    {
        foreach ((T key, S value) in otherDict)
        {
            thisDict.Add(key, value);
        }
    }

    // ReSharper disable once InconsistentNaming
    public static void RemoveMany<T, S>(this Dictionary<T, S> thisDict, IEnumerable<KeyValuePair<T, S>> otherDict) where T : struct where S : class => thisDict.RemoveMany(otherDict.Select(x => x.Key));

    // ReSharper disable once InconsistentNaming
    public static void RemoveMany<T, S>(this Dictionary<T, S> thisDict, IEnumerable<T> keyList) where T : struct where S : class
    {
        foreach (T key in keyList)
        {
            thisDict.Remove(key);
        }
    }

    public static int FindModifierIndex(this List<(string Key, float Value)> dataSource, string filter, bool strict = false)
    {
        if (strict)
        {
            return dataSource.FindIndex(modifier => modifier.Key.Equals(filter, StringComparison.InvariantCultureIgnoreCase));
        }

        return dataSource.FindIndex(modifier => modifier.Key.Contains(filter, StringComparison.InvariantCultureIgnoreCase));
    }

    public static IEnumerable<float> FindModifiers(this IEnumerable<(string Key, float Value)> dataSource, string filter, bool strict = false)
    {
        if (strict)
        {
            return dataSource.Where(modifier => modifier.Key.Equals(filter, StringComparison.InvariantCultureIgnoreCase))
                .Select(modifier => modifier.Value);
        }

        return dataSource.Where(modifier => modifier.Key.Contains(filter, StringComparison.InvariantCultureIgnoreCase))
            .Select(modifier => modifier.Value);
    }

    public static bool IsValidIndex(this int index) => index > -1;

    /// <summary>
    /// Sets the value of the dictionary for the specified nation key if the value is not null.
    /// </summary>
    /// <param name="thisDict">The dictionary extended by this method.</param>
    /// <param name="nation">The <see cref="Nation"/> used as key.</param>
    /// <param name="content">The content dictionary for the key.</param>
    /// <typeparam name="T">The data type of the content dictionary.</typeparam>
    /// <returns><see langword="true"/> if the content was added, <see langword="false"/> otherwise.</returns>
    public static bool SetIfNotNull<T>(this IDictionary<Nation, Dictionary<string, T>> thisDict, Nation nation, Dictionary<string, T>? content)
    {
        if (content == null)
        {
            return false;
        }

        thisDict[nation] = content;
        return true;
    }

    public static async Task<IEnumerable<TResult>> SelectAsync<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, Task<TResult>> method)
    {
        return await Task.WhenAll(source.Select(async s => await method(s)));
    }

    public static string ToTierString(this int level)
    {
        return level switch
        {
            1 => "I",
            2 => "II",
            3 => "III",
            4 => "IV",
            5 => "V",
            6 => "VI",
            7 => "VII",
            8 => "VIII",
            9 => "IX",
            10 => "X",
            11 => "XI",
            _ => level.ToString(),
        };
    }

    public static string PlaneTypeToString(this PlaneType planeType)
    {
        return planeType switch
        {
            PlaneType.None => "None",
            PlaneType.Fighter => "Fighter",
            PlaneType.DiveBomber => "DiveBomber",
            PlaneType.TorpedoBomber => "TorpedoBomber",
            PlaneType.SkipBomber => "SkipBomber",
            PlaneType.TacticalFighter => "TacticalFighter",
            PlaneType.TacticalDiveBomber => "TacticalDiveBomber",
            PlaneType.TacticalTorpedoBomber => "TacticalTorpedoBomber",
            PlaneType.TacticalSkipBomber => "TacticalSkipBomber",
            _ => planeType.ToString(),
        };
    }

    public static string ProjectileTypeToString(this ProjectileType projectileType)
    {
        return projectileType switch
        {
            ProjectileType.Artillery => "Artillery",
            ProjectileType.Bomb => "Bomb",
            ProjectileType.SkipBomb => "SkipBomb",
            ProjectileType.Torpedo => "Torpedo",
            ProjectileType.DepthCharge => "DepthCharge",
            ProjectileType.Rocket => "Rocket",
            _ => projectileType.ToString(),
        };
    }

    public static string ShellTypeToString(this ShellType shellType)
    {
        return shellType switch
        {
            ShellType.SAP => "SAP",
            ShellType.HE => "HE",
            ShellType.AP => "AP",
            _ => shellType.ToString(),
        };
    }

    public static string TorpedoTypeToString(this TorpedoType torpedoType)
    {
        return torpedoType switch
        {
            TorpedoType.Standard => "Standard",
            TorpedoType.DeepWater => "DeepWater",
            TorpedoType.Magnetic => "Magnetic",
            _ => torpedoType.ToString(),
        };
    }

    public static string BombTypeToString(this BombType bombType)
    {
        return bombType switch
        {
            BombType.HE => "HE",
            BombType.AP => "AP",
            _ => bombType.ToString(),
        };
    }

    public static string RocketTypeToString(this RocketType rocketType)
    {
        return rocketType switch
        {
            RocketType.HE => "HE",
            RocketType.AP => "AP",
            _ => rocketType.ToString(),
        };
    }

    public static string ShipNationToString(this Nation shipNation)
    {
        return shipNation switch
        {
            Nation.France => "France",
            Nation.Usa => "Usa",
            Nation.Russia => "Russia",
            Nation.Japan => "Japan",
            Nation.UnitedKingdom => "UnitedKingdom",
            Nation.Germany => "Germany",
            Nation.Italy => "Italy",
            Nation.Europe => "Europe",
            Nation.Netherlands => "Netherlands",
            Nation.Spain => "Spain",
            Nation.PanAsia => "PanAsia",
            Nation.PanAmerica => "PanAmerica",
            Nation.Commonwealth => "Commonwealth",
            Nation.Common => "Common",
            _ => shipNation.ToString(),
        };
    }

    public static string ShipCategoryToString(this ShipCategory shipCategory)
    {
        return shipCategory switch
        {
            ShipCategory.TechTree => "TechTree",
            ShipCategory.Premium => "Premium",
            ShipCategory.Special => "Special",
            ShipCategory.TestShip => "TestShip",
            ShipCategory.Disabled => "Disabled",
            ShipCategory.Clan => "Clan",
            ShipCategory.SuperShip => "SuperShip",
            _ => shipCategory.ToString(),
        };
    }

    public static string ShipClassToString(this ShipClass shipClass)
    {
        return shipClass switch
        {
            ShipClass.Submarine => "Submarine",
            ShipClass.Destroyer => "Destroyer",
            ShipClass.Cruiser => "Cruiser",
            ShipClass.Battleship => "Battleship",
            ShipClass.AirCarrier => "AirCarrier",
            ShipClass.Auxiliary => "Auxiliary",
            _ => shipClass.ToString(),
        };
    }

    public static string NameToIndex(this string name) => name.Split('_').First();
}
