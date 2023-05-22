using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WoWsShipBuilder.DataStructures.Versioning;
using WoWsShipBuilder.Infrastructure;
using WoWsShipBuilder.Infrastructure.Data;
using WoWsShipBuilder.Infrastructure.GameData;
using WoWsShipBuilder.Infrastructure.HttpClients;
using WoWsShipBuilder.Infrastructure.Utility;

namespace WoWsShipBuilder.Web.Infrastructure;

public class ServerAwsClient : IAwsClient
{
    private readonly ILogger logger;

    private readonly HttpClient httpClient;

    private readonly CdnOptions options;

    public ServerAwsClient(HttpClient httpClient, IOptions<CdnOptions> options, ILogger<ServerAwsClient> logger)
    {
        this.httpClient = httpClient;
        this.options = options.Value;
        this.logger = logger;
    }

    public async Task<VersionInfo> DownloadVersionInfo(ServerType serverType)
    {
        var url = @$"{options.Host}/api/{serverType.StringName()}/VersionInfo.json";
        string stringContent = await httpClient.GetStringAsync(url);
        return JsonConvert.DeserializeObject<VersionInfo>(stringContent) ?? throw new HttpRequestException("Unable to process VersionInfo response from AWS server.");
    }

    public async Task DownloadFiles(ServerType serverType, List<(string, string)> relativeFilePaths, IProgress<int>? downloadProgress = null)
    {
        string baseUrl = @$"{options.Host}/api/{serverType.StringName()}/";
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
                    logger.LogWarning(e, "Encountered an exception while downloading a file with uri {Uri}", uri);
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
        string baseUrl = @$"{options.Host}/api/{serverType.StringName()}/";
        return await httpClient.GetFromJsonAsync<Dictionary<string, string>>($"{baseUrl}Localization/{language}.json") ?? throw new InvalidOperationException();
    }

    private async Task DownloadFileAsync(Uri uri, string category, string fileName)
    {
        var str = await httpClient.GetStringAsync(uri);
        await DataCacheHelper.AddToCache(fileName, category, str);
    }
}
