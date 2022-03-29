using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Web.Utility;

public class ShellSelectionWrapper
{
    public ShellSelectionWrapper(string shipIndex, IEnumerable<ArtilleryData> artilleryData)
    {
        ShipIndex = shipIndex;
        ArtilleryDataDictionary = artilleryData.ToDictionary(x => x, _ => false);
    }

    public string ShipIndex { get; }

    public Dictionary<ArtilleryData, bool> ArtilleryDataDictionary { get; }
}

public sealed record ArtilleryData(ArtilleryShell Shell, Dispersion DispersionValues, string ShellIndex, double MaxRange)
{
    public ArtilleryData(string shellIndex)
        : this(new(), new(), shellIndex, default)
    {
    }

    public ArtilleryShell Shell { get; set; } = Shell;

    public string ShellIndex { get; } = ShellIndex;

    public double MaxRange { get; set;  } = MaxRange;

    public Dispersion DispersionValues { get; set; } = DispersionValues;
}
