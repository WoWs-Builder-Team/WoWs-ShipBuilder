using WoWsShipBuilder.Core.ConverterHelpers;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Web.Utility;

public static class Helpers
{
    public static string GetIconFromClass(ShipClass shipClass, ShipCategory category)
    {
        string path = ClassToPathHelper.GetSvgPathFromClass(shipClass);
        string stroke = ClassToPathHelper.GetColorFromCategory(category, true)[3..];
        string fill = ClassToPathHelper.GetColorFromCategory(category, false)[3..];
        return $"<path fill=\"#{fill}\" stroke=\"#{stroke}\" stroke-width=\"1\"  d=\"{path}\" />";
    }

    public static string GetNationFlag(IHostEnvironment environment, Nation shipNation, string shipIndex)
    {
        string imgName = File.Exists(Path.Combine(environment.ContentRootPath, "wwwroot", "assets", "nation_flags", $"flag_{shipIndex}.png")) ? shipIndex : shipNation.ToString();
        return $"/assets/nation_flags/flag_{imgName}.png";
    }
}
