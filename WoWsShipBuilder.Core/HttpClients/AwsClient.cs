using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Compression;
using System.Net.Http;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.DataStructures.Versioning;

namespace WoWsShipBuilder.Core.HttpClients;

public class AwsClient : ClientBase, IAwsClient
{
    private const string Host = "https://d2nzlaerr9l5k3.cloudfront.net";

    private readonly ILogger<AwsClient> logger;

    public AwsClient(IDataService dataService, IAppDataService appDataService, ILogger<AwsClient> logger, HttpMessageHandler? handler = null)
        : base(dataService, appDataService)
    {
        Client = new(new RetryHttpHandler(handler ?? new HttpClientHandler()));
        this.logger = logger;
    }

    protected override HttpClient Client { get; }

    /// <summary>
    /// Downloads all the images stored into a .zip file on the server and saves them into the local folder for images. Then deletes the .zip file.
    /// </summary>
    /// <param name="fileSystem">An <see cref="IFileSystem"/> used to access local files.</param>
    /// <param name="fileName">The name of the zip archive to download, without .zip extension.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref = "HttpRequestException" > Occurs if the server does not respond properly.</exception>
    /// <exception cref = "ArgumentNullException" > Occurs if the file is not available on the server.</exception>
    [UnsupportedOSPlatform("browser")]
    public async Task DownloadImages(IFileSystem fileSystem, string? fileName = null)
    {
        logger.LogDebug("Downloading ship images");
        string zipUrl;
        string localFolder;
        string zipName;

        zipName = fileName ?? "ship";
        zipUrl = @$"{Host}/images/ship/{zipName}.zip";
        localFolder = "Ships";

        string directoryPath = fileSystem.Path.Combine(AppDataService.AppDataImageDirectory, localFolder);

        if (!fileSystem.Directory.Exists(directoryPath))
        {
            fileSystem.Directory.CreateDirectory(directoryPath);
        }

        string zipPath = fileSystem.Path.Combine(directoryPath, $"{zipName}.zip");
        try
        {
            await DownloadFileAsync(new(zipUrl), zipPath);
            ZipFile.ExtractToDirectory(zipPath, directoryPath, true);
            fileSystem.File.Delete(zipPath);
        }
        catch (HttpRequestException e)
        {
            logger.LogWarning(e, "Failed to download images from uri {}", zipUrl);
        }
    }

    public async Task<VersionInfo> DownloadVersionInfo(ServerType serverType)
    {
        string url = @$"{Host}/api/{serverType.StringName()}/VersionInfo.json";
        return await GetJsonAsync<VersionInfo>(url) ?? throw new HttpRequestException("Unable to process VersionInfo response from AWS server.");
    }

    public async Task DownloadFiles(ServerType serverType, List<(string, string)> relativeFilePaths, IProgress<int>? downloadProgress = null)
    {
        logger.LogWarning("Downloading files for server type {ServerType}", serverType);
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
            string localFileName = DataService.CombinePaths(AppDataService.GetDataPath(serverType), category, fileName);
            Uri uri = string.IsNullOrWhiteSpace(category) ? new(baseUrl + fileName) : new(baseUrl + $"{category}/{fileName}");
            var task = Task.Run(async () =>
            {
                try
                {
                    await DownloadFileAsync(uri, localFileName);
                }
                catch (HttpRequestException e)
                {
                    logger.LogWarning(e, "Encountered an exception while downloading a file with uri {} and filename {}", uri, localFileName);
                }

                progress.Report(1);
            });
            taskList.Add(task);
        }

        // TODO: handle exceptions all at one place
        await Task.WhenAll(taskList);
    }
}
