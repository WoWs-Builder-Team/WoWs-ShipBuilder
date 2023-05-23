using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Infrastructure;

public static class EnumToStringExtensions
{
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
}
