using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Web.Utility;

public class ShellSelectionWrapper
{
    public ShellSelectionWrapper(string shipIndex, Dictionary<string, ArtilleryData> artilleryData)
    {
        ShipIndex = shipIndex;
        ArtilleryDataDictionary = artilleryData;
    }

    public string ShipIndex { get; }

    public double MaxRange { get; set; } = -1;

    public Dictionary<string, ArtilleryData> ArtilleryDataDictionary { get; }
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
