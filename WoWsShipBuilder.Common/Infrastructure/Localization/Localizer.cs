using WoWsShipBuilder.Features.Settings;
using WoWsShipBuilder.Infrastructure.DataTransfer;
using WoWsShipBuilder.Infrastructure.Localization.Resources;

namespace WoWsShipBuilder.Infrastructure.Localization;

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

    public LocalizationResult GetGameLocalization(string key, CultureDetails language)
    {
        string? result = gameLocalizationProvider.GetString(key, language);
        return new(result != null, result ?? key);
    }

    public LocalizationResult GetAppLocalization(string key)
    {
        string? result = Translation.ResourceManager.GetString(key, appSettings.SelectedLanguage.CultureInfo);
        return new(result != null, result ?? key);
    }

    public LocalizationResult GetAppLocalization(string key, CultureDetails language)
    {
        string? result = Translation.ResourceManager.GetString(key, language.CultureInfo);
        return new(result != null, result ?? key);
    }
}
