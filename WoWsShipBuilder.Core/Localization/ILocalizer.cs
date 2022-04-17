namespace WoWsShipBuilder.Core.Localization;

public interface ILocalizer
{
    LocalizationResult this[string key] { get; }

    LocalizationResult GetGameLocalization(string key);

    LocalizationResult GetAppLocalization(string key);
}

public readonly record struct LocalizationResult(bool LocalizationFound, string Localization);
