using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.HttpResponses;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.Core.HttpClients
{
    public class AwsClient : ClientBase
    {
        #region Static Fields and Constants

        private const string Host = "https://d2nzlaerr9l5k3.cloudfront.net";

        private static readonly Lazy<AwsClient> InstanceValue = new(() => new AwsClient());

        private static readonly Logger Logger = Logging.GetLogger("AwsClient");

        #endregion

        private AwsClient()
            : this(new FileSystem(), AppDataHelper.Instance)
        {
        }

        internal AwsClient(IFileSystem fileSystem, AppDataHelper appDataHelper)
            : base(fileSystem, appDataHelper)
        {
        }

        public static AwsClient Instance => InstanceValue.Value;

        /// <summary>
        /// Downloads images from the AWS server.
        /// </summary>
        /// <param name="fileList">List of indexes of the ships or names of the camos to download.</param>
        /// <param name="type">The type of images to download. Can be either Ship or Camo.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        /// <exception cref = "HttpRequestException" > Occurs if the server does not respond properly.</exception>
        /// <exception cref = "ArgumentNullException" > Occurs if files are not available on the server.</exception>
        public async Task DownloadImages(List<string> fileList, ImageType type)
        {
            List<Task> downloads = new();

            foreach (var file in fileList)
            {
                string url;
                string localFolder;
                if (type == ImageType.Ship)
                {
                    url = @$"{Host}/images/ship/{file}.png";
                    localFolder = "Ships";
                }
                else
                {
                    url = @$"{Host}/images/camo/{file}.png";
                    localFolder = "Camos";
                }

                string folderPath = FileSystem.Path.Combine(AppDataHelper.Instance.AppDataDirectory, "Images", localFolder);

                if (!FileSystem.Directory.Exists(folderPath))
                {
                    FileSystem.Directory.CreateDirectory(folderPath);
                }

                string fileName = FileSystem.Path.Combine(folderPath, $"{file}.png");

                downloads.Add(DownloadFileAsync(new Uri(url), fileName));
            }

            await Task.WhenAll(downloads).ConfigureAwait(false);
        }

        /// <summary>
        /// Downloads all the images stored into a .zip file on the server and saves them into the local folder for images. Then deletes the .zip file.
        /// </summary>
        /// <param name="type">The type of images to download. Can be either Ship or Camo.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        /// <exception cref = "HttpRequestException" > Occurs if the server does not respond properly.</exception>
        /// <exception cref = "ArgumentNullException" > Occurs if the file is not available on the server.</exception>
        public async Task DownloadImages(ImageType type, string? fileName = null)
        {
            Logging.Logger.Info("Test " + type);
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
                await DownloadFileAsync(new Uri(zipUrl), zipPath);
                ZipFile.ExtractToDirectory(zipPath, directoryPath, true);
                FileSystem.File.Delete(zipPath);
            }
            catch (HttpRequestException e)
            {
                Logging.Logger.Error(e, "Failed to download images.");
            }
        }

        /// <summary>
        /// Checks if there are updates to the program data.
        /// </summary>
        /// <param name="serverType">The <see cref="ServerType"/> for the requested data.</param>
        /// <param name="progress">A <see cref="IProgress{T}"/> to track the state of the operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        /// <exception cref = "HttpRequestException" > Occurs if the server does not respond properly.</exception>
        /// <exception cref = "InvalidOperationException" > Occurs if the file can't be deserialized.</exception>
        public async Task<(bool ShouldUpdate, bool CanDeltaUpdate, string NewVersion)> CheckFileVersion(ServerType serverType, IProgress<(int, string)>? progress = null)
        {
            string server = serverType == ServerType.Live ? "live" : "pts";

            string url = @$"{Host}/api/{server}/VersionInfo.json";

            progress?.Report((0, "versionInfo"));
            VersionInfo? versionInfo = null;
            var attempts = 0;

            var shouldUpdate = false;
            var canDeltaUpdate = false;

            progress?.Report((10, "versionProcessing"));
            while (attempts < 5)
            {
                try
                {
                    versionInfo = await GetJsonAsync<VersionInfo>(url) ??
                                  throw new HttpRequestException("Could not process response from AWS Server.");
                    break;
                }
                catch (HttpRequestException e)
                {
                    attempts++;
                    if (attempts < 5)
                    {
                        Logging.Logger.Warn(e);
                    }
                    else
                    {
                        Logging.Logger.Error(e, "Error during app update. Maximum retries reached.");
                        return (shouldUpdate, canDeltaUpdate, string.Empty);
                    }
                }
            }

            string newVersion = versionInfo!.VersionName.Substring(0, versionInfo.VersionName.IndexOf('#'));
            string dataPath = AppDataHelper.Instance.GetDataPath(serverType);
            if (!FileSystem.Directory.Exists(dataPath))
            {
                FileSystem.Directory.CreateDirectory(dataPath);
            }

            string localVersionInfoPath = FileSystem.Path.Combine(dataPath, "VersionInfo.json");
            if (FileSystem.File.Exists(localVersionInfoPath))
            {
                VersionInfo localVersionInfo = JsonConvert.DeserializeObject<VersionInfo>(await FileSystem.File.ReadAllTextAsync(localVersionInfoPath)) ??
                                               throw new InvalidOperationException();

                if (localVersionInfo.CurrentVersionCode < versionInfo.CurrentVersionCode)
                {
                    shouldUpdate = true;
                    List<Task> downloads = new();
                    foreach ((string key, var value) in versionInfo.Categories)
                    {
                        foreach (var item in value)
                        {
                            FileVersion? currentFile = localVersionInfo.Categories[key].Find(x => x.FileName.Equals(item.FileName));
                            string fileName = $"{item.FileName}";

                            if (currentFile == null || currentFile.Version < item.Version)
                            {
                                string fileUrl = $"{Host}/api/{server}/{key}/{fileName}";
                                string filePath = FileSystem.Path.Combine(dataPath, key);
                                if (!FileSystem.Directory.Exists(filePath))
                                {
                                    FileSystem.Directory.CreateDirectory(filePath);
                                }

                                downloads.Add(DownloadFileAsync(new Uri(fileUrl), FileSystem.Path.Combine(filePath, fileName)));
                            }
                        }
                    }

                    downloads.Add(DownloadFileAsync(new Uri(url), localVersionInfoPath));
                    progress?.Report((20, "jsonData"));
                    await Task.WhenAll(downloads).ConfigureAwait(false);
                    progress?.Report((60, "localizationData"));
                    try
                    {
                        string oldVersionSubstring = localVersionInfo.VersionName.Substring(0, localVersionInfo.VersionName.IndexOf('#'));
                        string expectedOldVersion = versionInfo.LastVersionName.Substring(0, versionInfo.LastVersionName.IndexOf('#'));
                        canDeltaUpdate = oldVersionSubstring.Equals(expectedOldVersion, StringComparison.Ordinal);
                    }
                    catch (Exception)
                    {
                        Logger.Error("Unable to parse old version properly.");
                        canDeltaUpdate = false;
                    }
                }

                string localePath = FileSystem.Path.Combine(AppDataHelper.Instance.GetDataPath(serverType), "Localization", AppData.Settings.Locale);
                if (localVersionInfo.CurrentVersionCode < versionInfo.CurrentVersionCode || !FileSystem.File.Exists(localePath))
                {
                    await DownloadLanguage(serverType).ConfigureAwait(false);
                }
            }
            else
            {
                shouldUpdate = true;
                List<Task> downloads = new();
                foreach ((string key, var value) in versionInfo!.Categories)
                {
                    foreach (var item in value)
                    {
                        string fileName = $"{item.FileName}";
                        string fileUrl = $"{Host}/api/{server}/{key}/{fileName}";
                        string filePath = FileSystem.Path.Combine(dataPath, key);
                        if (!FileSystem.Directory.Exists(filePath))
                        {
                            FileSystem.Directory.CreateDirectory(filePath);
                        }

                        downloads.Add(DownloadFileAsync(new Uri(fileUrl), FileSystem.Path.Combine(filePath, fileName)));
                    }
                }

                downloads.Add(DownloadFileAsync(new Uri(url), localVersionInfoPath));
                progress?.Report((20, "jsonData"));
                await Task.WhenAll(downloads).ConfigureAwait(false);
                progress?.Report((60, "localizationData"));
                await DownloadLanguage(serverType).ConfigureAwait(false);
            }

            progress?.Report((100, "done"));
            return (shouldUpdate, canDeltaUpdate, newVersion);
        }

        /// <summary>
        /// Downloads program languages.
        /// </summary>
        /// <param name="serverType">The <see cref="ServerType"/> of the requested data.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task DownloadLanguage(ServerType serverType)
        {
            string server = serverType == ServerType.Live ? "live" : "pts";

            string localePath = FileSystem.Path.Combine(AppDataHelper.Instance.GetDataPath(serverType), "Localization");
            if (!FileSystem.Directory.Exists(localePath))
            {
                FileSystem.Directory.CreateDirectory(localePath);
            }

            string fileName = "en-GB.json";
            string url = $"{Host}/api/{server}/Localization/{fileName}";
            string localeName = FileSystem.Path.Combine(localePath, fileName);
            await DownloadFileAsync(new Uri(url), localeName).ConfigureAwait(false);
        }
    }
}
