using WoWsShipBuilder.Common.Infrastructure.GameData;

namespace WoWsShipBuilder.Common.Infrastructure.Localization;

public interface ILocalizationProvider
{
    Task RefreshDataAsync(ServerType serverType, params CultureDetails[] supportedCultures);

    string? GetString(string key, CultureDetails cultureDetails);
}
