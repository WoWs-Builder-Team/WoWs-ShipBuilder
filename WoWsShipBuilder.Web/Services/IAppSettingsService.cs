namespace WoWsShipBuilder.Web.Services;

using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Settings;

public interface IAppSettingsService
{
    AppSettings CurrentSettings { get; }
}

public class AppSettingsService : IAppSettingsService
{
    public AppSettings CurrentSettings => AppData.Settings;
}
