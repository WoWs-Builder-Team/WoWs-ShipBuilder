namespace WoWsShipBuilder.Core.Localization;

public interface ILocalizer
{
    LocalizationResult GetGameLocalization(string key);

    LocalizationResult GetAppLocalization(string key);
}

public readonly record struct LocalizationResult(bool LocalizationFound, string Localization);
