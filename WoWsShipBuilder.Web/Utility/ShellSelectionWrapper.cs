using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Web.Utility;

public class ShellSelectionWrapper
{
    public string ShipIndex = string.Empty;
    public Dictionary<ArtilleryData, bool> ArtilleryDictionary = new();

    public ShellSelectionWrapper(string shipIndex, IEnumerable<ArtilleryData> artilleryData)
    {
        ShipIndex = shipIndex;
        ArtilleryDictionary = artilleryData.ToDictionary(x => x, _ => false);
    }
}

public sealed record ArtilleryData(ArtilleryShell Shell, Dispersion DispersionValues, string ShellIndex, double MaxRange)
{
    public ArtilleryShell Shell { get; } = Shell;
    public string ShellIndex { get; } = ShellIndex;
    public double MaxRange { get; } = MaxRange;
    public Dispersion DispersionValues { get; } = DispersionValues;
}
