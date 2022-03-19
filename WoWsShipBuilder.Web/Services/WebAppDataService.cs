using WoWsShipBuilder.Core;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Web.Services;

public class WebAppDataService : IAppDataService
{
    private readonly IDataService dataService;

    public WebAppDataService(IDataService dataService)
    {
        this.dataService = dataService;
    }

    public string DefaultAppDataDirectory { get; } = default!;

    public string AppDataDirectory { get; } = default!;

    public string AppDataImageDirectory { get; } = default!;

    public async Task<Dictionary<string, T>?> ReadLocalJsonData<T>(Nation nation, ServerType serverType)
    {
        string categoryString = IAppDataService.GetCategoryString<T>();
        string nationString = IAppDataService.GetNationString(nation);
        string dataLocation = dataService.CombinePaths(GetDataPath(serverType), categoryString, $"{nationString}");
        return await DeserializeFromDb<Dictionary<string, T>>(dataLocation);
    }

    public async Task<VersionInfo?> ReadLocalVersionInfo(ServerType serverType)
    {
        string dataLocation = dataService.CombinePaths(GetDataPath(serverType), "VersionInfo");
        return await DeserializeFromDb<VersionInfo>(dataLocation);
    }

    public async Task<List<ShipSummary>> GetShipSummaryList(ServerType serverType)
    {
        string dataLocation = dataService.CombinePaths(GetDataPath(serverType), "Summary", "Common.json");
        return await DeserializeFromDb<List<ShipSummary>>(dataLocation) ?? new List<ShipSummary>();
    }

    public async Task LoadNationFiles(Nation nation)
    {
        var server = AppData.Settings.SelectedServerType;
        if (AppData.ShipDictionary?.FirstOrDefault() == null || AppData.ShipDictionary.First().Value.ShipNation != nation)
        {
            AppData.ShipDictionary = await ReadLocalJsonData<Ship>(nation, server);
        }

        AppData.ProjectileCache.SetIfNotNull(nation, await ReadLocalJsonData<Projectile>(nation, server));
        AppData.AircraftCache.SetIfNotNull(nation, await ReadLocalJsonData<Aircraft>(nation, server));
        AppData.ConsumableList ??= await ReadLocalJsonData<Consumable>(Nation.Common, server);
    }

    public async Task<Projectile> GetProjectile(string projectileName)
    {
        var nation = IAppDataService.GetNationFromIndex(projectileName);
        if (!AppData.ProjectileCache.ContainsKey(nation))
        {
            AppData.ProjectileCache.SetIfNotNull(nation, await ReadLocalJsonData<Projectile>(nation, AppData.Settings.SelectedServerType));
        }

        return AppData.ProjectileCache[nation][projectileName];
    }

    public async Task<T> GetProjectile<T>(string projectileName) where T : Projectile
    {
        return (T)await GetProjectile(projectileName);
    }

    public async Task<Aircraft> GetAircraft(string aircraftName)
    {
        var nation = IAppDataService.GetNationFromIndex(aircraftName);
        if (!AppData.AircraftCache.ContainsKey(nation))
        {
            AppData.AircraftCache.SetIfNotNull(nation, await ReadLocalJsonData<Aircraft>(nation, AppData.Settings.SelectedServerType));
        }

        return AppData.AircraftCache[nation][aircraftName];
    }

    public async Task<Dictionary<string, string>?> ReadLocalizationData(ServerType serverType, string language)
    {
        string fileName = dataService.CombinePaths(GetDataPath(serverType), "Localization", $"{language}");
        return await DeserializeFromDb<Dictionary<string, string>>(fileName);
    }

    public async Task<Ship?> GetShipFromSummary(ShipSummary summary, bool changeDictionary = true)
    {
        Ship? ship = null;

        if (summary.Nation.Equals(AppData.CurrentLoadedNation))
        {
            ship = AppData.ShipDictionary![summary.Index];
        }
        else
        {
            var shipDict = await ReadLocalJsonData<Ship>(summary.Nation, AppData.Settings.SelectedServerType);
            if (shipDict != null)
            {
                ship = shipDict[summary.Index];
                if (changeDictionary)
                {
                    AppData.ShipDictionary = shipDict;
                    AppData.CurrentLoadedNation = summary.Nation;
                }
            }
        }

        return ship;
    }

    public void SaveBuilds()
    {
        throw new NotImplementedException();
    }

    public void LoadBuilds()
    {
        throw new NotImplementedException();
    }

    public string GetDataPath(ServerType serverType)
    {
        return serverType.StringName();
    }

    public string GetLocalizationPath(ServerType serverType) => dataService.CombinePaths(GetDataPath(serverType), "Localization");

    public List<string> GetInstalledLocales(ServerType serverType, bool includeFileType = true)
    {
        // TODO list of locale
        throw new NotImplementedException();
    }

    internal async Task<T?> DeserializeFromDb<T>(string dataLocation)
    {
        if (string.IsNullOrWhiteSpace(dataLocation))
        {
            throw new ArgumentException("The provided data location must not be empty.");
        }

        var returnValue = await dataService.LoadAsync<T>(dataLocation);

        if (returnValue == null)
        {
            Logging.Logger.Warn($"Tried to load {dataLocation}, but it was not found.");
        }

        return returnValue;
    }
}
