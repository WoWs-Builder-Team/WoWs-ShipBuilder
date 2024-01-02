using WoWsShipBuilder.Features.DataTransfer;

namespace WoWsShipBuilder.Infrastructure.Localization;

public static class LocalizerExtensions
{
    public static string SimpleAppLocalization(this ILocalizer localizer, string key) => localizer.GetAppLocalization(key).Localization;

    public static string SimpleAppLocalization(this ILocalizer localizer, string key, CultureDetails cultureDetails) => localizer.GetAppLocalization(key, cultureDetails).Localization;

    public static string SimpleGameLocalization(this ILocalizer localizer, string key) => localizer.GetGameLocalization(key).Localization;

    public static string SimpleGameLocalization(this ILocalizer localizer, string key, CultureDetails cultureDetails) => localizer.GetGameLocalization(key, cultureDetails).Localization;
}
