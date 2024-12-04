namespace WoWsShipBuilder.Desktop.CrossPlatform.Features.Updater;

public record ValidationResult(bool ValidationStatus)
{
    public IEnumerable<(string, string)>? InvalidFiles { get; init; }
}
