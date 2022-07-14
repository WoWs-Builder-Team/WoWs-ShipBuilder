using WoWsShipBuilder.Core.Settings;

namespace WoWsShipBuilder.Core.Localization;

public class Localizer : ILocalizer
{
    private readonly ILocalizationProvider gameLocalizationProvider;

    private readonly AppSettings appSettings;

    public Localizer(ILocalizationProvider gameLocalizationProvider, AppSettings appSettings)
    {
        this.gameLocalizationProvider = gameLocalizationProvider;
        this.appSettings = appSettings;
    }

    public LocalizationResult this[string key] => GetGameLocalization(key);

    public LocalizationResult GetGameLocalization(string key)
    {
        string? result = gameLocalizationProvider.GetString(key, appSettings.SelectedLanguage);
        return new(result != null, result ?? key);
    }

    public LocalizationResult GetAppLocalization(string key)
    {
        string? result = Translation.ResourceManager.GetString(key, appSettings.SelectedLanguage.CultureInfo);
        return new(result != null, result ?? key);
    }
}
