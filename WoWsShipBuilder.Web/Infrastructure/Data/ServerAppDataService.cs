using System.Collections.Immutable;
using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Sentry;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.DataStructures.Versioning;
using WoWsShipBuilder.Infrastructure.ApplicationData;
using WoWsShipBuilder.Infrastructure.GameData;
using WoWsShipBuilder.Infrastructure.HttpClients;

namespace WoWsShipBuilder.Web.Infrastructure.Data;

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
        this.DefaultAppDataDirectory = string.Empty;
        this.AppDataDirectory = string.Empty;
        this.AppDataImageDirectory = string.Empty;
    }

    public async Task FetchData()
    {
        this.logger.LogInformation("Starting to fetch data with server type {Server}...", this.options.Server);
        const string undefinedMarker = "undefined";
        AppData.ResetCaches();

        var onlineVersionInfo = await this.awsClient.DownloadVersionInfo(this.options.Server);
        if (onlineVersionInfo.CurrentVersion is not null)
        {
            AppData.DataVersion = onlineVersionInfo.CurrentVersion.MainVersion.ToString(3) + "#" + onlineVersionInfo.CurrentVersion.DataIteration;
            this.logger.LogInformation("Found online version info with version {Version}", AppData.DataVersion);
        }
        else
        {
            AppData.DataVersion = undefinedMarker;
            this.logger.LogWarning("Online version info not available");
        }

        SentrySdk.ConfigureScope(scope =>
        {
            scope.SetTag("data.version", onlineVersionInfo.CurrentVersion?.MainVersion.ToString(3) ?? undefinedMarker);
            scope.SetTag("data.iteration", onlineVersionInfo.CurrentVersion?.DataIteration.ToString(CultureInfo.InvariantCulture) ?? undefinedMarker);
            scope.SetTag("data.server", onlineVersionInfo.CurrentVersion?.VersionType.ToString() ?? undefinedMarker);
        });
        var files = onlineVersionInfo.Categories.SelectMany(category => category.Value.Select(file => (category.Key, file.FileName))).ToList();
        await this.awsClient.DownloadFiles(this.options.Server, files);
        InitializeShipSelectorDataStructure();
        this.logger.LogInformation("Finished fetching data");
    }

    public async Task LoadLocalFilesAsync(ServerType serverType)
    {
        AppData.ResetCaches();
        const string shipBuilderDirectory = "WoWsShipBuilderDev";
        string dataRoot = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), shipBuilderDirectory, "json", serverType.StringName());

        string versionInfoContent = await File.ReadAllTextAsync(Path.Join(dataRoot, "VersionInfo.json"));
        var localVersionInfo = JsonSerializer.Deserialize<VersionInfo>(versionInfoContent, AppConstants.JsonSerializerOptions)!;
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

        InitializeShipSelectorDataStructure();
    }

    public async Task<VersionInfo?> GetCurrentVersionInfo(ServerType serverType)
    {
        this.versionInfo ??= await this.awsClient.DownloadVersionInfo(serverType);
        return this.versionInfo;
    }

    public async Task<Dictionary<string, string>?> ReadLocalizationData(ServerType serverType, string language)
    {
        if (this.options.UseLocalFiles)
        {
            const string shipBuilderDirectory = "WoWsShipBuilderDev";
            string localizationRoot = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), shipBuilderDirectory, "json", this.options.Server.StringName(), "Localization");
            string file = Path.Join(localizationRoot, $"{language}.json");
            if (!File.Exists(file))
            {
                return new();
            }

            string fileContent = await File.ReadAllTextAsync(Path.Join(localizationRoot, $"{language}.json"));
            return JsonSerializer.Deserialize<Dictionary<string, string>>(fileContent, AppConstants.JsonSerializerOptions);
        }

        if (this.awsClient is ServerAwsClient serverAwsClient)
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

    private static void InitializeShipSelectorDataStructure()
    {
        var result = AppData.ShipDictionary.GroupBy(x => x.Value.ShipNation)
            .ToImmutableDictionary(
                nationGrouping => nationGrouping.Key,
                nationGrouping => nationGrouping.GroupBy(nationShip => nationShip.Value.ShipCategory)
                    .ToImmutableDictionary(
                        categoryGrouping => categoryGrouping.Key,
                        categoryGrouping => categoryGrouping.GroupBy(categoryShip => categoryShip.Value.ShipClass)
                            .ToImmutableDictionary(
                                shipClassGrouping => shipClassGrouping.Key,
                                shipClassGrouping => shipClassGrouping.GroupBy(shipClassShip => shipClassShip.Value.Tier)
                                    .ToImmutableDictionary(
                                        tierGrouping => tierGrouping.Key,
                                        tierGrouping => tierGrouping.Select(tierShip => tierShip.Value).ToImmutableList()))));

        AppData.FittingToolShipSelectorDataStructure = result;
    }
}
