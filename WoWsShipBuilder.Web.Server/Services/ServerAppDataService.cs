using System.Collections.Concurrent;
using System.IO.Abstractions;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilder.Core.HttpClients;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Web.Server.Services;

// TODO: implement properly
public class ServerAppDataService : IAppDataService
{
    private const string ShipBuilderName = "WoWsShipBuilder";

    private readonly IFileSystem fileSystem;

    private readonly IDataService dataService;

    private readonly IAwsClient awsClient;

    private readonly string devDataSource;

    private readonly ConcurrentBag<Nation> loadedNations = new();

    private readonly SemaphoreSlim semaphoreSlim = new(1);

    private VersionInfo? versionInfo;

    public string DefaultAppDataDirectory { get; }

    public string AppDataDirectory { get; }

    public string AppDataImageDirectory { get; }

    public ServerAppDataService(IFileSystem fileSystem, IDataService dataService, IAwsClient awsClient)
    {
        this.fileSystem = fileSystem;
        this.dataService = dataService;
        this.awsClient = awsClient;
        DefaultAppDataDirectory = string.Empty;
        AppDataDirectory = string.Empty;
        AppDataImageDirectory = string.Empty;
        devDataSource = dataService.CombinePaths(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ShipBuilderName);
    }

    public async Task FetchData()
    {
        AppData.ShipDictionary = new();

        var versionInfo = await awsClient.DownloadVersionInfo(ServerType.Live);
        var files = versionInfo.Categories.SelectMany(category => category.Value.Select(file => (category.Key, file.FileName))).ToList();
        await awsClient.DownloadFiles(ServerType.Live, files);
    }

    public async Task<Dictionary<string, T>?> ReadLocalJsonData<T>(Nation nation, ServerType serverType)
    {
        string categoryString = IAppDataService.GetCategoryString<T>();
        string nationString = IAppDataService.GetNationString(nation);
        string fileName = dataService.CombinePaths(GetDataPath(serverType), categoryString, $"{nationString}.json");
        if (fileSystem.File.Exists(fileName))
        {
            return await dataService.LoadAsync<Dictionary<string, T>>(fileName);
        }

        return null;
    }

    public async Task<VersionInfo?> GetLocalVersionInfo(ServerType serverType)
    {
        versionInfo ??= await awsClient.DownloadVersionInfo(serverType);
        return versionInfo;
    }

    public async Task<List<ShipSummary>> GetShipSummaryList(ServerType serverType)
    {
        if (AppData.ShipSummaryList is null)
        {
            string fileName = dataService.CombinePaths(GetDataPath(serverType), "Summary", "Common.json");
            AppData.ShipSummaryList = await dataService.LoadAsync<List<ShipSummary>>(fileName) ?? throw new InvalidOperationException();
        }

        return AppData.ShipSummaryList;
    }

    public async Task LoadNationFiles(Nation nation)
    {
        if (loadedNations.Contains(nation))
        {
            return;
        }

        await semaphoreSlim.WaitAsync();
        if (!loadedNations.Contains(nation))
        {
            var server = ServerType.Live;

            AppData.ConsumableList ??= await ReadLocalJsonData<Consumable>(Nation.Common, server);
            AppData.ProjectileCache.SetIfNotNull(nation, await ReadLocalJsonData<Projectile>(nation, server));
            AppData.AircraftCache.SetIfNotNull(nation, await ReadLocalJsonData<Aircraft>(nation, server));
            AppData.ExteriorCache.SetIfNotNull(nation, await ReadLocalJsonData<Exterior>(nation, server));
            AppData.CaptainCache.SetIfNotNull(nation, await ReadLocalJsonData<Captain>(nation, server));

            if (nation == Nation.Common)
            {
                var modernizations = await ReadLocalJsonData<Modernization>(nation, server);
                foreach ((string? key, var value) in modernizations!)
                {
                    AppData.ModernizationCache.Add(key, value);
                }
            }
        }

        loadedNations.Add(nation);
        semaphoreSlim.Release();
    }

    public Task<Projectile> GetProjectile(string projectileName)
    {
        var nation = IAppDataService.GetNationFromIndex(projectileName);
        return Task.FromResult(AppData.ProjectileCache[nation][projectileName]);
    }

    public async Task<T> GetProjectile<T>(string projectileName) where T : Projectile
    {
        return (T)await GetProjectile(projectileName);
    }

    public Task<Aircraft> GetAircraft(string aircraftName)
    {
        var nation = IAppDataService.GetNationFromIndex(aircraftName);
        return Task.FromResult(AppData.AircraftCache[nation][aircraftName]);
    }

    public async Task<Dictionary<string, string>?> ReadLocalizationData(ServerType serverType, string language)
    {
        if (awsClient is ServerAwsClient serverAwsClient)
        {
            return await serverAwsClient.DownloadLocalization(language, serverType);
        }

        return null;
    }

    public Task<Ship?> GetShipFromSummary(ShipSummary summary, bool changeDictionary = true)
    {
        return Task.FromResult<Ship?>(AppData.ShipDictionary![summary.Index]);
    }

    public string GetDataPath(ServerType serverType)
    {
        return dataService.CombinePaths(devDataSource, "json", serverType.StringName());
    }

    public string GetLocalizationPath(ServerType serverType) => dataService.CombinePaths(GetDataPath(serverType), "Localization");

    public Task<List<string>> GetInstalledLocales(ServerType serverType, bool includeFileType = true)
    {
        fileSystem.Directory.CreateDirectory(GetLocalizationPath(serverType));
        var files = fileSystem.Directory.GetFiles(GetLocalizationPath(serverType)).Select(file => fileSystem.FileInfo.FromFileName(file));
        return Task.FromResult(includeFileType ? files.Select(file => file.Name).ToList() : files.Select(file => fileSystem.Path.GetFileNameWithoutExtension(file.Name)).ToList());
    }
}
