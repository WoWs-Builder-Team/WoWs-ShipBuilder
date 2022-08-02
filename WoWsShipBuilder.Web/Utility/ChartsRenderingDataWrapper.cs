using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Web.Utility;

public sealed record ChartsRenderingDataWrapper(string ShipIndex, Dictionary<string, ArtilleryData> ArtilleryDataDictionary, double MaxRange = -1, double Sigma = -1)
{
    public double MaxRange { get; set; } = MaxRange;
    public double Sigma { get; set; } = Sigma;
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
