using WoWsShipBuilder.Features.Settings;

namespace WoWsShipBuilder.Infrastructure.Data;

public interface ISettingsAccessor
{
    public Task<AppSettings?> LoadSettings();

    public Task SaveSettings(AppSettings appSettings);
}
