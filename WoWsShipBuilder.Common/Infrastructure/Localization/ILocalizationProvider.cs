using WoWsShipBuilder.Infrastructure.DataTransfer;
using WoWsShipBuilder.Infrastructure.GameData;

namespace WoWsShipBuilder.Infrastructure.Localization;

public interface ILocalizationProvider
{
    Task RefreshDataAsync(ServerType serverType, params CultureDetails[] supportedCultures);

    string? GetString(string key, CultureDetails cultureDetails);
}
