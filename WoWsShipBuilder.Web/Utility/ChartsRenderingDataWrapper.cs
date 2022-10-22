using WoWsShipBuilder.DataStructures.Projectile;
using WoWsShipBuilder.DataStructures.Ship;

namespace WoWsShipBuilder.Web.Utility;

public sealed record ChartsRenderingDataWrapper(string ShipIndex, Dictionary<string, ArtilleryData> ArtilleryDataDictionary, double MaxRange = -1, double Sigma = -1)
{
    public bool IsFullyConfigured { get; } = Math.Abs(MaxRange - -1) > 0.001;
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
