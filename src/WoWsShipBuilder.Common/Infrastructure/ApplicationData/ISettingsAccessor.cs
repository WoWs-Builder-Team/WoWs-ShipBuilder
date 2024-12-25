using WoWsShipBuilder.Features.Settings;

namespace WoWsShipBuilder.Infrastructure.ApplicationData;

public interface ISettingsAccessor
{
    public Task<AppSettings?> LoadSettings();

    public Task SaveSettings(AppSettings appSettings);
}
