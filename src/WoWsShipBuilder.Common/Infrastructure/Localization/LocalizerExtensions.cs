using WoWsShipBuilder.Infrastructure.DataTransfer;

namespace WoWsShipBuilder.Infrastructure.Localization;

public static class LocalizerExtensions
{
    public static string SimpleAppLocalization(this ILocalizer localizer, string key) => localizer.GetAppLocalization(key).Localization;

    public static string SimpleAppLocalization(this ILocalizer localizer, string key, params object[] args) => localizer.GetAppLocalization(key, args).Localization;

    public static string SimpleAppLocalization(this ILocalizer localizer, string key, CultureDetails cultureDetails) => localizer.GetAppLocalization(key, cultureDetails).Localization;

    public static string SimpleAppLocalization(this ILocalizer localizer, string key, CultureDetails cultureDetails, params object[] args) => localizer.GetAppLocalization(key, cultureDetails, args).Localization;

    public static string SimpleGameLocalization(this ILocalizer localizer, string key) => localizer.GetGameLocalization(key).Localization;

    public static string SimpleGameLocalization(this ILocalizer localizer, string key, CultureDetails cultureDetails) => localizer.GetGameLocalization(key, cultureDetails).Localization;

    /// <summary>
    /// Returns the first game localization that actually resolved, or an empty string if none did.
    /// </summary>
    /// <remarks>
    /// <see cref="ILocalizer.GetGameLocalization(string)"/> echoes the key back when it is missing, so a key that a
    /// game update has renamed renders as raw text such as TALENT_PJW018_1_1_2_DESCRIPTION. Use this wherever the
    /// exact key depends on game data, so an unexpected shape degrades to nothing rather than to a visible key.
    /// </remarks>
    public static string FirstGameLocalizationOrEmpty(this ILocalizer localizer, params string[] keys)
    {
        foreach (string key in keys)
        {
            if (string.IsNullOrEmpty(key))
            {
                continue;
            }

            var result = localizer.GetGameLocalization(key);
            if (result.LocalizationFound)
            {
                return result.Localization;
            }
        }

        return string.Empty;
    }
}
