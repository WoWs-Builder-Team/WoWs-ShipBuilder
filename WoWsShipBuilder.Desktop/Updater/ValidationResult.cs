using System.Collections.Generic;

namespace WoWsShipBuilder.Desktop.Updater;

public record ValidationResult(bool ValidationStatus)
{
    public IEnumerable<(string, string)>? InvalidFiles { get; init; }
}
