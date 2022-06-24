using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Web.Utility;

public class ShellSelectionWrapper
{
    public ShellSelectionWrapper(string shipIndex, Dictionary<string, ArtilleryData> artilleryData, double maxRange)
    {
        ShipIndex = shipIndex;
        ArtilleryDataDictionary = artilleryData;
        MaxRange = maxRange;
    }

    public string ShipIndex { get; }

    public double MaxRange { get; set; }

    public Dictionary<string, ArtilleryData> ArtilleryDataDictionary { get; }

    protected bool Equals(ShellSelectionWrapper other)
    {
        return ShipIndex == other.ShipIndex;
    }

    public override int GetHashCode()
    {
        return ShipIndex.GetHashCode();
    }
}

public sealed record ArtilleryData(ArtilleryShell Shell, Dispersion DispersionValues, bool ShellSelected)
{
    public ArtilleryData()
        : this(new(), new(), false)
    {
    }

    public ArtilleryShell Shell { get; set; } = Shell;

    public Dispersion DispersionValues { get; set; } = DispersionValues;

    public bool ShellSelected { get; set; } = ShellSelected;
}
