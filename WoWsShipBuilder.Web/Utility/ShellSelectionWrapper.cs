using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Web.Utility;

public class ShellSelectionWrapper
{
    public ShellSelectionWrapper(string shipIndex, Dictionary<string, ArtilleryData> artilleryData, double maxRange, double sigma)
    {
        ShipIndex = shipIndex;
        ArtilleryDataDictionary = artilleryData;
        MaxRange = maxRange;
        Sigma = sigma;
    }

    public string ShipIndex { get; }

    public double MaxRange { get; set; }

    public double Sigma { get; set; }

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
