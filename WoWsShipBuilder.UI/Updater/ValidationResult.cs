using System.Collections.Generic;

namespace WoWsShipBuilder.Core.DataProvider.Updater;

public record ValidationResult(bool ValidationStatus)
{
    public IEnumerable<(string, string)>? InvalidFiles { get; init; }
}
