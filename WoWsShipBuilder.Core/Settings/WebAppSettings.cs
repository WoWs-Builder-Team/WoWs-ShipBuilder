using System.Collections.Generic;

namespace WoWsShipBuilder.Core.Settings;

public record WebAppSettings
{
    public bool OpenAllMainExpandersByDefault { get; set; } = true;

    public bool OpenAllAmmoExpandersByDefault { get; set; } = true;

    public bool OpenSecondariesAndAaExpandersByDefault { get; set; } = true;

    public List<string> BetaAccessCodes { get; set; } = new();

    public bool[] BuildImageLayoutSettings { get; set; } = { true, false, true, true, true, true };
}
