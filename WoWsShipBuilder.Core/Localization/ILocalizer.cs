using WoWsShipBuilder.Core.DataProvider;

namespace WoWsShipBuilder.Core.Localization;

public interface ILocalizer
{
    LocalizationResult this[string key] { get; }

    LocalizationResult GetGameLocalization(string key);

    LocalizationResult GetAppLocalization(string key);

    LocalizationResult GetGameLocalization(string key, CultureDetails language);

    LocalizationResult GetAppLocalization(string key, CultureDetails language);
}

public readonly record struct LocalizationResult(bool LocalizationFound, string Localization);
