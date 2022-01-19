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
using WoWsShipBuilder.DataStructures;

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
            Client = new(new RetryHttpHandler(handler ?? new HttpClientHandler()));
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
                await DownloadFileAsync(new(zipUrl), zipPath);
                ZipFile.ExtractToDirectory(zipPath, directoryPath, true);
                FileSystem.File.Delete(zipPath);
            }
            catch (HttpRequestException e)
            {
                Logger.Warn(e, "Failed to download images from uri {}.", zipUrl);
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
                Uri uri = string.IsNullOrWhiteSpace(category) ? new(baseUrl + fileName) : new(baseUrl + $"{category}/{fileName}");
                var task = Task.Run(async () =>
                {
                    try
                    {
                        await DownloadFileAsync(uri, localFileName);
                    }
                    catch (HttpRequestException e)
                    {
                        Logger.Warn(e, "Encountered an exception while downloading a file with uri {} and filename {}.", uri, localFileName);
                    }

                    progress.Report(1);
                });
                taskList.Add(task);
            }

            // TODO: handle exceptions all at one place
            await Task.WhenAll(taskList);
        }
    }
}
