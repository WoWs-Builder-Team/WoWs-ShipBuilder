namespace WoWsShipBuilder.Core.Settings;

public record WebAppSettings
{
    public bool OpenAllMainExpandersByDefault { get; set; } = true;

    public bool OpenAllAmmoExpandersByDefault { get; set; } = true;

    public bool OpenSecondariesAndAAExpandersByDefault { get; set; } = true;
}
