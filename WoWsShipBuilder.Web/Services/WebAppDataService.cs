using BlazorWorker.BackgroundServiceFactory;
using BlazorWorker.Core;
using BlazorWorker.Extensions.JSRuntime;
using BlazorWorker.WorkerBackgroundService;
using DnetIndexedDb;
using WoWsShipBuilder.Core;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.Web.Data;
using WoWsShipBuilder.Web.WebWorkers;

namespace WoWsShipBuilder.Web.Services;

public class WebAppDataService : IAppDataService
{
    private readonly GameDataDb gameDataDb;

    // private readonly IWorkerFactory workerFactory;

    private readonly IDataService dataService;

    // private IWorkerBackgroundService<StartupWebWorkerService>? startupWorker;

    // private IWorkerBackgroundService<WebWorkerDataService>? worker;

    public WebAppDataService(IDataService dataService, GameDataDb gameDataDb)
    {
        this.gameDataDb = gameDataDb;
        // this.workerFactory = workerFactory;
        this.dataService = dataService;
    }

    public string DefaultAppDataDirectory { get; } = default!;

    public string AppDataDirectory { get; } = default!;

    public string AppDataImageDirectory { get; } = default!;

    public async Task<Dictionary<string, T>?> ReadLocalJsonData<T>(Nation nation, ServerType serverType)
    {
        string dataLocation = FindDataPath<T>(nation, serverType);
        return await DeserializeFromDb<Dictionary<string, T>>(dataLocation);
    }

    private string FindDataPath<T>(Nation nation, ServerType serverType)
    {
        string categoryString = IAppDataService.GetCategoryString<T>();
        string nationString = IAppDataService.GetNationString(nation);
        return CombinePaths(GetDataPath(serverType), categoryString, $"{nationString}");
    }

    // This doesn't use the worker cause VersionInfo is a record, so not serializable by the library. #BlameFloribe.
    public async Task<VersionInfo?> ReadLocalVersionInfo(ServerType serverType)
    {
        string dataLocation = dataService.CombinePaths(serverType.StringName(), "VersionInfo");
        return await dataService.LoadAsync<VersionInfo?>(dataLocation);
    }

    public async Task<List<ShipSummary>> GetShipSummaryList(ServerType serverType)
    {
        string dataLocation = CombinePaths(GetDataPath(serverType), "Summary", "Common");
        return await DeserializeFromDb<List<ShipSummary>>(dataLocation) ?? new List<ShipSummary>();
    }

    public async Task LoadNationFiles(Nation nation)
    {
        var server = AppData.Settings.SelectedServerType;
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
        string fileName = CombinePaths(GetDataPath(serverType), "Localization", $"{language}");
        return await DeserializeFromDb<Dictionary<string, string>>(fileName);
    }

    public async Task<Ship?> GetShipFromSummary(ShipSummary summary, bool changeDictionary = true)
    {
        if (AppData.ShipDictionary!.TryGetValue(summary.Index, out var ship))
        {
            Console.WriteLine("cache hit");
            return ship;
        }

        var dataPath = FindDataPath<Ship>(summary.Nation, AppData.Settings.SelectedServerType);
        // ship = await worker!.RunAsync(service => service.LoadShipAsync(summary.Index, dataPath));

        // var shipDict = await ReadLocalJsonData<Ship>(summary.Nation, AppData.Settings.SelectedServerType);
        // if (shipDict != null)
        // {
        //     ship = shipDict[summary.Index];
        //     if (changeDictionary)
        //     {
        //         AppData.ShipDictionary = AppData.ShipDictionary.Union(shipDict)
        //             .DistinctBy(x => x.Key)
        //             .ToDictionary(x => x.Key, x => x.Value);
        //     }
        // }
        if (ship is not null)
        {
            AppData.ShipDictionary[ship.Index] = ship;
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

    public string GetLocalizationPath(ServerType serverType) => CombinePaths(GetDataPath(serverType), "Localization");

    public async Task<List<string>> GetInstalledLocales(ServerType serverType, bool includeFileType = true)
    {
        await gameDataDb.OpenIndexedDb();
        var basePath = serverType.StringName() + ".Localization";
        return (await gameDataDb.GetRange<string, GameDataDto>(serverType.StringName(), basePath, basePath + "\uffff"))
            .Select(dto => dto.Path.Split('.').Last())
            .ToList();
    }

    private async Task<T?> DeserializeFromDb<T>(string dataLocation)
    {
        if (string.IsNullOrWhiteSpace(dataLocation))
        {
            throw new ArgumentException("The provided data location must not be empty.");
        }

        // await InitializeWorker();
        // var returnValue = await worker!.RunAsync(w => w.LoadAsync<T>(dataLocation));
        var returnValue = default(T);

        if (returnValue == null)
        {
            Logging.Logger.Warn($"Tried to load {dataLocation}, but it was not found.");
        }

        return returnValue;
    }

    // private async Task InitializeWorker()
    // {
    //     if (startupWorker is null)
    //     {
    //         var factory = await workerFactory.CreateAsync();
    //         startupWorker = await factory.CreateBackgroundServiceAsync<StartupWebWorkerService>(wo => wo.AddConventionalAssemblyOfService().AddAssemblyOf<ServiceCollection>().
    //             AddAssemblies(GetAssembliesForWorker()).AddBlazorWorkerJsRuntime().AddAssemblyOf<IndexedDbInterop>());
    //     }
    //
    //     worker ??= await startupWorker.CreateBackgroundServiceAsync(startup => startup.Resolve<WebWorkerDataService>());
    // }

    private static string[] GetAssembliesForWorker()
    {
        return new string[]
        {
            "Microsoft.Extensions.DependencyInjection.Abstractions.dll", "Microsoft.Extensions.DependencyInjection.dll",
            "WoWsShipBuilder.Core.dll",
            "System.Diagnostics.Tracing.dll",
            "WoWsShipBuilder.DataStructures.dll",
            "NLog.dll",
            "JsonSubTypes.dll",
        };
    }

    private string CombinePaths(params string[] paths)
    {
        IEnumerable<string> cleanPaths = paths.Select(path => path.Replace('/', '.').Trim('.'));
        return string.Join('.', cleanPaths);
    }

}
