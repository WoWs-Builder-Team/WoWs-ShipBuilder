using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Infrastructure.GameData;

public static class ClassToPathHelper
{
    private const string CVIcon = "M 0 0 V 3.9688 H 8.7312 V 0 Z M 18.8417 4.715 10.675 0 V 9.43 Z M 0 5.5563 V 9.525 H 8.7312 V 5.5563 Z";
    private const string DDIcon = "m 0 0 v 9.525 L 17.9917 4.7625 Z";
    private const string BBIcon = "M 0 0 H 6.6146 L 1.5875 9.525 H 0 Z m 9.5406 0 h 1.3229 L 5.8365 9.525 H 4.5136 Z m 4.1167 0 l -5.0271 9.525 h 5.0271 l 8.599 -4.7625 z";
    private const string CCIcon = "M 0 0 H 8.2021 L 3.175 9.525 H 0 Z M 11.6573 0 6.6302 9.525 h 5.0271 l 8.599 -4.7625 z";
    private const string SubIcon = "m 0 0 h 3.175 V 9.7896 h -3.175 z M 5.8208 0.1719 V 9.6969 L 18.124 4.9344 Z";

    private const string GoldColor = "#ffffd700";
    private const string WhiteColor = "#ffffffff";

    public static string GetSvgPathFromClass(ShipClass shipClass)
    {
        return shipClass switch
        {
            ShipClass.Submarine => SubIcon,
            ShipClass.Destroyer => DDIcon,
            ShipClass.Cruiser => CCIcon,
            ShipClass.Battleship => BBIcon,
            ShipClass.AirCarrier => CVIcon,
            _ => "",
        };
    }

    public static string GetColorFromCategory(ShipCategory category, bool border)
    {
        if (border)
        {
            return category.Equals(ShipCategory.TechTree) ? WhiteColor : GoldColor;
        }

        if (category.Equals(ShipCategory.Premium) && !border)
        {
            return GoldColor;
        }

        return WhiteColor;
    }
}
