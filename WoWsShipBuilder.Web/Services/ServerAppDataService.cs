using Sentry;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.HttpClients;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Web.Services;

public class ServerAppDataService : IAppDataService
{
    private readonly IAwsClient awsClient;

    private VersionInfo? versionInfo;

    public string DefaultAppDataDirectory { get; }

    public string AppDataDirectory { get; }

    public string AppDataImageDirectory { get; }

    public ServerAppDataService(IAwsClient awsClient)
    {
        this.awsClient = awsClient;
        DefaultAppDataDirectory = string.Empty;
        AppDataDirectory = string.Empty;
        AppDataImageDirectory = string.Empty;
    }

    public async Task FetchData()
    {
        const string undefinedMarker = "undefined";
        AppData.ShipDictionary = new();

        var onlineVersionInfo = await awsClient.DownloadVersionInfo(ServerType.Dev1);
        if (onlineVersionInfo.CurrentVersion is not null)
        {
            AppData.DataVersion = onlineVersionInfo.CurrentVersion.MainVersion.ToString(3) + "#" + onlineVersionInfo.CurrentVersion.DataIteration;
        }
        else
        {
            AppData.DataVersion = undefinedMarker;
        }

        SentrySdk.ConfigureScope(scope =>
        {
            scope.SetTag("data.version", onlineVersionInfo.CurrentVersion?.MainVersion.ToString(3) ?? undefinedMarker);
            scope.SetTag("data.iteration", onlineVersionInfo.CurrentVersion?.DataIteration.ToString() ?? undefinedMarker);
            scope.SetTag("data.server", onlineVersionInfo.CurrentVersion?.VersionType.ToString() ?? undefinedMarker);
        });
        var files = onlineVersionInfo.Categories.SelectMany(category => category.Value.Select(file => (category.Key, file.FileName))).ToList();
        await awsClient.DownloadFiles(ServerType.Dev1, files);
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

    public Task LoadNationFiles(Nation nation)
    {
        return Task.CompletedTask;
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

    public Task<Dictionary<string, Exterior>> GetExteriorList(Nation nation, ServerType serverType)
    {
        return Task.FromResult(AppData.ExteriorCache[nation]);
    }

    public Task<Dictionary<string, Captain>> GetCaptains(Nation nation, ServerType serverType)
    {
        return Task.FromResult(AppData.CaptainCache[nation]);
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
        throw new InvalidOperationException();
    }

    public string GetLocalizationPath(ServerType serverType) => string.Empty;

    public Task<List<string>> GetInstalledLocales(ServerType serverType, bool includeFileType = true)
    {
        return Task.FromResult(AppConstants.SupportedLanguages.Select(language => language.LocalizationFileName).ToList());
    }
}
