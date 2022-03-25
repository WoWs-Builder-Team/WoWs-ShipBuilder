namespace WoWsShipBuilder.Web.Utility;

public class ShellSelectionWrapper
{
    public string ShipIndex = string.Empty;
    public Dictionary<string, bool> ShellDictionary = new();

    public ShellSelectionWrapper(string shipIndex, IEnumerable<string> shellIndex)
    {
        ShipIndex = shipIndex;
        ShellDictionary = shellIndex.ToDictionary(x => x, _ => false);
    }
}
