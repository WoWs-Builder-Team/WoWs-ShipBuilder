using System.IO.Abstractions;
using Newtonsoft.Json;
using NLog;
using WoWsShipBuilder.Core;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilder.Core.HttpClients;
using WoWsShipBuilder.Core.HttpResponses;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Web.Server.Services;

public class ServerAwsClient : IAwsClient
{
    private const string Host = "https://d2nzlaerr9l5k3.cloudfront.net";

    private static readonly Logger Logger = Logging.GetLogger("AwsClient");

    private readonly HttpClient httpClient;

    public ServerAwsClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<VersionInfo> DownloadVersionInfo(ServerType serverType)
    {
        string url = @$"{Host}/api/{serverType.StringName()}/VersionInfo.json";
        var stringContent = await httpClient.GetStringAsync(url);
        return JsonConvert.DeserializeObject<VersionInfo>(stringContent) ?? throw new HttpRequestException("Unable to process VersionInfo response from AWS server.");

        // return await httpClient.GetFromJsonAsync<VersionInfo>(url) ?? throw new HttpRequestException("Unable to process VersionInfo response from AWS server.");
    }

    public async Task DownloadFiles(ServerType serverType, List<(string, string)> relativeFilePaths, IProgress<int>? downloadProgress = null)
    {
        string baseUrl = @$"{Host}/api/{serverType.StringName()}/";
        var taskList = new List<Task>();
        int totalFiles = relativeFilePaths.Count;
        var finished = 0;
        IProgress<int> progress = new Progress<int>(update =>
        {
            finished += update;
            downloadProgress?.Report(finished / totalFiles);
        });

        foreach ((string category, string fileName) in relativeFilePaths)
        {
            Uri uri = string.IsNullOrWhiteSpace(category) ? new(baseUrl + fileName) : new(baseUrl + $"{category}/{fileName}");
            var task = Task.Run(async () =>
            {
                try
                {
                    await DownloadFileAsync(uri, category, fileName);
                }
                catch (HttpRequestException e)
                {
                    Logger.Warn(e, "Encountered an exception while downloading a file with uri {}.", uri);
                }

                progress.Report(1);
            });
            taskList.Add(task);
        }

        // TODO: handle exceptions all at one place
        await Task.WhenAll(taskList);
    }

    public async Task<Dictionary<string, string>> DownloadLocalization(string language, ServerType serverType)
    {
        string baseUrl = @$"{Host}/api/{serverType.StringName()}/";
        return await httpClient.GetFromJsonAsync<Dictionary<string, string>>($"{baseUrl}Localization/{language}.json") ?? throw new InvalidOperationException();
    }

    private async Task DownloadFileAsync(Uri uri, string category, string fileName)
    {
        var parseResult = Enum.TryParse(fileName.Replace(".json", string.Empty).Replace("_", string.Empty), true, out Nation nation);
        if (!parseResult)
        {
            throw new InvalidOperationException();
        }

        var type = category.ToLowerInvariant() switch
        {
            "ability" => typeof(Dictionary<string, Consumable>),
            "aircraft" => typeof(Dictionary<string, Aircraft>),
            "crew" => typeof(Dictionary<string, Captain>),
            "exterior" => typeof(Dictionary<string, Exterior>),
            "modernization" => typeof(Dictionary<string, Modernization>),
            "projectile" => typeof(Dictionary<string, Projectile>),
            "ship" => typeof(Dictionary<string, Ship>),
            "unit" => typeof(Dictionary<string, Module>),
            "summary" => typeof(List<ShipSummary>),
            _ => throw new InvalidOperationException(),
        };

        var str = await httpClient.GetStringAsync(uri);
        var jsonObject = JsonConvert.DeserializeObject(str, type);

        switch (category.ToLowerInvariant())
        {
            case "ability":
                if (jsonObject is Dictionary<string, Consumable> consumables)
                {
                    AppData.ConsumableList = consumables;
                }

                break;
            case "aircraft":
                AppData.AircraftCache.SetIfNotNull(nation, (Dictionary<string, Aircraft>?)jsonObject);
                break;
            case "crew":
                AppData.CaptainCache.SetIfNotNull(nation, (Dictionary<string, Captain>?)jsonObject);
                break;
            case "exterior":
                AppData.ExteriorCache.SetIfNotNull(nation, (Dictionary<string, Exterior>?)jsonObject);
                break;
            case "modernization":
                if (jsonObject is Dictionary<string, Modernization> modernizations)
                {
                    foreach (var modernization in modernizations)
                    {
                        AppData.ModernizationCache.Add(modernization.Key, modernization.Value);
                    }
                }

                break;
            case "projectile":
                AppData.ProjectileCache.SetIfNotNull(nation, (Dictionary<string, Projectile>?)jsonObject);
                break;
            case "ship":
                if (jsonObject is Dictionary<string, Ship> ships)
                {
                    foreach (var ship in ships)
                    {
                        AppData.ShipDictionary!.Add(ship.Key, ship.Value);
                    }
                }

                break;
            case "unit":
                break;
            case "summary":
                if (jsonObject is List<ShipSummary> shipSummaries)
                {
                    AppData.ShipSummaryList = shipSummaries;
                }

                break;
        }
    }

    public Task DownloadImages(ImageType type, IFileSystem fileSystem, string? fileName = null)
    {
        throw new NotImplementedException();
    }
}
