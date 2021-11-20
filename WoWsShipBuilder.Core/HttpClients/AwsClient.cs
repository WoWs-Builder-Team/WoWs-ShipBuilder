using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using NLog;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilder.Core.HttpResponses;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.Core.HttpClients
{
    public class AwsClient : ClientBase, IAwsClient
    {
        #region Static Fields and Constants

        private const string Host = "https://d2nzlaerr9l5k3.cloudfront.net";

        private static readonly Lazy<AwsClient> InstanceValue = new(() => new AwsClient());

        private static readonly Logger Logger = Logging.GetLogger("AwsClient");

        #endregion

        private AwsClient()
            : this(new FileSystem(), AppDataHelper.Instance, null)
        {
        }

        internal AwsClient(IFileSystem fileSystem, AppDataHelper appDataHelper, HttpMessageHandler? handler)
            : base(fileSystem, appDataHelper)
        {
            Client = new HttpClient(new RetryHttpHandler(handler ?? new HttpClientHandler()));
        }

        public static AwsClient Instance => InstanceValue.Value;

        protected override HttpClient Client { get; }

        /// <summary>
        /// Downloads all the images stored into a .zip file on the server and saves them into the local folder for images. Then deletes the .zip file.
        /// </summary>
        /// <param name="type">The type of images to download. Can be either Ship or Camo.</param>
        /// <param name="fileName">The name of the zip archive to download, without .zip extension.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        /// <exception cref = "HttpRequestException" > Occurs if the server does not respond properly.</exception>
        /// <exception cref = "ArgumentNullException" > Occurs if the file is not available on the server.</exception>
        public async Task DownloadImages(ImageType type, string? fileName = null)
        {
            Logger.Debug("Downloading images for type {}.", type);
            string zipUrl;
            string localFolder;
            string zipName;

            if (type == ImageType.Ship)
            {
                zipName = fileName ?? "ship";
                zipUrl = @$"{Host}/images/ship/{zipName}.zip";
                localFolder = "Ships";
            }
            else
            {
                zipName = fileName ?? "camo";
                zipUrl = @$"{Host}/images/camo/{zipName}.zip";
                localFolder = "Camos";
            }

            string directoryPath = FileSystem.Path.Combine(AppDataHelper.Instance.AppDataImageDirectory, localFolder);

            if (!FileSystem.Directory.Exists(directoryPath))
            {
                FileSystem.Directory.CreateDirectory(directoryPath);
            }

            string zipPath = FileSystem.Path.Combine(directoryPath, $"{zipName}.zip");
            try
            {
                await DownloadFileAsync(new Uri(zipUrl), zipPath)
                    .ContinueWith(
                        t => Logger.Warn(t.Exception, "Exception while downloading images from uri: {}", zipUrl),
                        TaskContinuationOptions.OnlyOnFaulted);
                ZipFile.ExtractToDirectory(zipPath, directoryPath, true);
                FileSystem.File.Delete(zipPath);
            }
            catch (HttpRequestException e)
            {
                Logger.Error(e, "Failed to download images.");
            }
        }

        public async Task<VersionInfo> DownloadVersionInfo(ServerType serverType)
        {
            string url = @$"{Host}/api/{serverType.StringName()}/VersionInfo.json";
            return await GetJsonAsync<VersionInfo>(url) ?? throw new HttpRequestException("Unable to process VersionInfo response from AWS server.");
        }

        public async Task DownloadFiles(ServerType serverType, List<(string, string)> relativeFilePaths, IProgress<int>? downloadProgress = null)
        {
            Logger.Debug("Downloading files for server type {}.", serverType);
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
                string localFileName = FileSystem.Path.Combine(AppDataHelper.GetDataPath(serverType), category, fileName);
                Uri uri = string.IsNullOrWhiteSpace(category) ? new Uri(baseUrl + fileName) : new Uri(baseUrl + $"{category}/{fileName}");
                Task task = Task.Run(async () =>
                {
                    await DownloadFileAsync(uri, localFileName)
                        .ContinueWith(
                            t => Logger.Warn(t.Exception, "Encountered an exception while downloading a file with uri {} and filename {}.", uri, localFileName),
                            TaskContinuationOptions.OnlyOnFaulted);
                    progress.Report(1);
                });
                taskList.Add(task);
            }

            await Task.WhenAll(taskList);
        }

        /// <summary>
        /// Downloads program languages.
        /// </summary>
        /// <param name="serverType">The <see cref="ServerType"/> of the requested data.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task DownloadLanguage(ServerType serverType)
        {
            Logger.Debug("Downloading language files for server type {}.", serverType);
            string server = serverType == ServerType.Live ? "live" : "pts";

            string localePath = FileSystem.Path.Combine(AppDataHelper.Instance.GetDataPath(serverType), "Localization");
            if (!FileSystem.Directory.Exists(localePath))
            {
                FileSystem.Directory.CreateDirectory(localePath);
            }

            string fileName = "en-GB.json";
            string url = $"{Host}/api/{server}/Localization/{fileName}";
            string localeName = FileSystem.Path.Combine(localePath, fileName);
            await DownloadFileAsync(new Uri(url), localeName)
                .ContinueWith(t => Logger.Error(t.Exception, "Error while downloading localization with url {}.", url), TaskContinuationOptions.OnlyOnFaulted)
                .ConfigureAwait(false);
        }
    }
}
