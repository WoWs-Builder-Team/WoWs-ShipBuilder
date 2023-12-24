using WoWsShipBuilder.Infrastructure.DataTransfer;

namespace WoWsShipBuilder.Infrastructure.Localization;

public interface ILocalizer
{
    LocalizationResult GetGameLocalization(string key);

    LocalizationResult GetGameLocalization(string key, params object[] args);

    LocalizationResult GetGameLocalization(string key, CultureDetails language);

    LocalizationResult GetGameLocalization(string key, CultureDetails language, params object[] args);

    LocalizationResult GetAppLocalization(string key);

    LocalizationResult GetAppLocalization(string key, params object[] args);

    LocalizationResult GetAppLocalization(string key, CultureDetails language);

    LocalizationResult GetAppLocalization(string key, CultureDetails language, params object[] args);
}

public readonly record struct LocalizationResult(bool LocalizationFound, string Localization);
