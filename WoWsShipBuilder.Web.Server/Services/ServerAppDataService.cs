using System.IO.Abstractions;
using WoWsShipBuilder.Core.DataProvider;
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

    public Task<VersionInfo?> ReadLocalVersionInfo(ServerType serverType)
    {
        throw new NotImplementedException();
    }

    public Task<List<ShipSummary>> GetShipSummaryList(ServerType serverType)
    {
        throw new NotImplementedException();
    }

    public Task LoadNationFiles(Nation nation)
    {
        throw new NotImplementedException();
    }

    public Task<Projectile> GetProjectile(string projectileName)
    {
        throw new NotImplementedException();
    }

    public Task<T> GetProjectile<T>(string projectileName) where T : Projectile
    {
        throw new NotImplementedException();
    }

    public Task<Aircraft> GetAircraft(string aircraftName)
    {
        throw new NotImplementedException();
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
        throw new NotImplementedException();
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
        return dataService.CombinePaths(devDataSource, "json", serverType.StringName());
    }

    public string GetLocalizationPath(ServerType serverType)
    {
        throw new NotImplementedException();
    }

    public Task<List<string>> GetInstalledLocales(ServerType serverType, bool includeFileType = true)
    {
        throw new NotImplementedException();
    }
}
