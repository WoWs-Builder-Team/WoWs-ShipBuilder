using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Sentry;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.HttpClients;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Aircraft;
using WoWsShipBuilder.DataStructures.Projectile;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.DataStructures.Versioning;
using WoWsShipBuilder.Web.Data;

namespace WoWsShipBuilder.Web.Services;

public class ServerAppDataService : IAppDataService
{
    private readonly IAwsClient awsClient;

    private readonly CdnOptions options;

    private readonly ILogger<ServerAppDataService> logger;

    private VersionInfo? versionInfo;

    public string DefaultAppDataDirectory { get; }

    public string AppDataDirectory { get; }

    public string AppDataImageDirectory { get; }

    public ServerAppDataService(IAwsClient awsClient, IOptions<CdnOptions> options, ILogger<ServerAppDataService> logger)
    {
        this.awsClient = awsClient;
        this.options = options.Value;
        this.logger = logger;
        DefaultAppDataDirectory = string.Empty;
        AppDataDirectory = string.Empty;
        AppDataImageDirectory = string.Empty;
    }

    public async Task FetchData()
    {
        logger.LogInformation("Starting to fetch data with server type {server}...", options.Server);
        const string undefinedMarker = "undefined";
        AppData.ResetCaches();

        var onlineVersionInfo = await awsClient.DownloadVersionInfo(options.Server);
        if (onlineVersionInfo.CurrentVersion is not null)
        {
            AppData.DataVersion = onlineVersionInfo.CurrentVersion.MainVersion.ToString(3) + "#" + onlineVersionInfo.CurrentVersion.DataIteration;
            logger.LogInformation("Found online version info with version {version}", AppData.DataVersion);
        }
        else
        {
            AppData.DataVersion = undefinedMarker;
            logger.LogWarning("Online version info not available");
        }

        SentrySdk.ConfigureScope(scope =>
        {
            scope.SetTag("data.version", onlineVersionInfo.CurrentVersion?.MainVersion.ToString(3) ?? undefinedMarker);
            scope.SetTag("data.iteration", onlineVersionInfo.CurrentVersion?.DataIteration.ToString() ?? undefinedMarker);
            scope.SetTag("data.server", onlineVersionInfo.CurrentVersion?.VersionType.ToString() ?? undefinedMarker);
        });
        var files = onlineVersionInfo.Categories.SelectMany(category => category.Value.Select(file => (category.Key, file.FileName))).ToList();
        await awsClient.DownloadFiles(options.Server, files);
        logger.LogInformation("Finished fetching data");
    }

    public async Task LoadLocalFilesAsync(ServerType serverType)
    {
        AppData.ResetCaches();
        const string shipBuilderDirectory = "WoWsShipBuilderDev";
        string dataRoot = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), shipBuilderDirectory, "json", serverType.StringName());

        string versionInfoContent = await File.ReadAllTextAsync(Path.Join(dataRoot, "VersionInfo.json"));
        var localVersionInfo = JsonConvert.DeserializeObject<VersionInfo>(versionInfoContent)!;
        AppData.DataVersion = localVersionInfo.CurrentVersion.MainVersion.ToString(3) + "#" + localVersionInfo.CurrentVersion.DataIteration;

        var dataRootInfo = new DirectoryInfo(dataRoot);
        DirectoryInfo[] categories = dataRootInfo.GetDirectories();
        foreach (var category in categories)
        {
            if (category.Name.Contains("Localization", StringComparison.InvariantCultureIgnoreCase))
            {
                continue;
            }
            foreach (var file in category.GetFiles())
            {
                string content = await File.ReadAllTextAsync(file.FullName);
                await DataCacheHelper.AddToCache(file.Name, category.Name, content);
            }
        }
    }

    public async Task<VersionInfo?> GetCurrentVersionInfo(ServerType serverType)
    {
        versionInfo ??= await awsClient.DownloadVersionInfo(serverType);
        return versionInfo;
    }

    public Task<List<ShipSummary>> GetShipSummaryList(ServerType serverType)
    {
        return Task.FromResult(AppData.ShipSummaryList ?? throw new InvalidOperationException());
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
        if (options.UseLocalFiles)
        {
            const string shipBuilderDirectory = "WoWsShipBuilderDev";
            string localizationRoot = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), shipBuilderDirectory, "json", options.Server.StringName(), "Localization");
            string file = Path.Join(localizationRoot, $"{language}.json");
            if (!File.Exists(file))
            {
                return new();
            }

            string fileContent = await File.ReadAllTextAsync(Path.Join(localizationRoot, $"{language}.json"));
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(fileContent);
        }

        if (awsClient is ServerAwsClient serverAwsClient)
        {
            return await serverAwsClient.DownloadLocalization(language, serverType);
        }

        return null;
    }

    public Ship GetShipFromSummary(ShipSummary summary) => AppData.ShipDictionary[summary.Index];

    public string GetDataPath(ServerType serverType)
    {
        throw new InvalidOperationException();
    }

    public string GetLocalizationPath(ServerType serverType) => string.Empty;

    public Task<List<string>> GetInstalledLocales(ServerType serverType, bool includeFileType = true)
    {
        return Task.FromResult(AppConstants.SupportedLanguages.Select(language => language.LocalizationFileName).ToList());
    }
}
