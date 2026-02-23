namespace WoWsShipBuilder.Features.Settings;

public class DispersionPlotSettings
{
    public int ShotsNumber { get; set; } = 100;

    public double AimingRange { get; set; } = 10.0;

    public double ShootingRange { get; set; } = 10.0; // this is for shell trajectory chart

    public bool IsVertical { get; set; } = true;
}
