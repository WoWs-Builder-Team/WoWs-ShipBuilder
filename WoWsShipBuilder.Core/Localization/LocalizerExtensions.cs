using WoWsShipBuilder.Core.DataProvider;

namespace WoWsShipBuilder.Core.Localization;

public static class LocalizerExtensions
{
    public static string SimpleAppLocalization(this ILocalizer localizer, string key) => localizer.GetAppLocalization(key).Localization;

    public static string SimpleAppLocalization(this ILocalizer localizer, string key, CultureDetails cultureDetails) => localizer.GetAppLocalization(key, cultureDetails).Localization;

    public static string SimpleGameLocalization(this ILocalizer localizer, string key) => localizer.GetGameLocalization(key).Localization;

    public static string SimpleGameLocalization(this ILocalizer localizer, string key, CultureDetails cultureDetails) => localizer.GetGameLocalization(key, cultureDetails).Localization;
}
