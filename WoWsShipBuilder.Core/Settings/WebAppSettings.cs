using System.Collections.Generic;

namespace WoWsShipBuilder.Core.Settings;

public record WebAppSettings
{
    public bool OpenAllMainExpandersByDefault { get; set; } = true;

    public bool OpenAllAmmoExpandersByDefault { get; set; } = true;

    public bool OpenSecondariesAndAaExpandersByDefault { get; set; } = true;

    public List<string> BetaAccessCodes { get; set; } = new();
}
