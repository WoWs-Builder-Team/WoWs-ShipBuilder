using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
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

        #endregion

        private AwsClient()
        {
        }

        public static AwsClient Instance => InstanceValue.Value;

        /// <summary>
        /// Downloads images from the AWS server.
        /// </summary>
        /// <param name="fileList">List of indexes of the ships or names of the camos to download.</param>
        /// <param name="type">The type of images to donwload. Can be either Ship or Camo.</param>
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

                string folderPath = Path.Combine(AppDataHelper.AppDataDirectory, "Images", localFolder);

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string fileName = Path.Combine(folderPath, $"{file}.png");

                downloads.Add(DownloadFileAsync(new Uri(url), fileName));
            }

            await Task.WhenAll(downloads).ConfigureAwait(false);
        }

        /// <summary>
        /// Downloads all the images stored into a .zip file on the server and saves them into the local folder for images. Then deletes the .zip file.
        /// </summary>
        /// <param name="type">The type of images to donwload. Can be either Ship or Camo.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        /// <exception cref = "HttpRequestException" > Occurs if the server does not respond properly.</exception>
        /// <exception cref = "ArgumentNullException" > Occurs if the file is not available on the server.</exception>
        public async Task DownloadAllImages(ImageType type)
        {
            string zipUrl;
            string localFolder;
            string zipName;
            if (type == ImageType.Ship)
            {
                zipName = "ship.zip";
                zipUrl = @$"{Host}/images/ship/{zipName}";
                localFolder = "Ships";
            }
            else
            {
                zipName = "camo.zip";
                zipUrl = @$"{Host}/images/camo/{zipName}";
                localFolder = "Camos";
            }

            string directoryPath = Path.Combine(AppDataHelper.AppDataDirectory, "Images", localFolder);

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string zipPath = Path.Combine(directoryPath, zipName);
            await DownloadFileAsync(new Uri(zipUrl), zipPath);
            ZipFile.ExtractToDirectory(zipPath, directoryPath, true);
            File.Delete(zipPath);
        }

        /// <summary>
        /// Checks if there are updates to the program data.
        /// </summary>
        /// <param name="pts">If true donwloads PTS server data instead of the live one.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        /// <exception cref = "HttpRequestException" > Occurs if the server does not respond properly.</exception>
        /// <exception cref = "InvalidOperationException" > Occurs if the file can't be deserialized.</exception>
        public async Task CheckFileVersion(bool pts)
        {
            string server = "live";

            if (pts)
            {
                server = "pts";
            }

            string url = @$"{Host}/api/{server}/VersionInfo.json";

            string dataPath = Path.Combine(AppDataHelper.AppDataDirectory, "api", server);

            if (!Directory.Exists(dataPath))
            {
                Directory.CreateDirectory(dataPath);
            }

            string localVersionInfoPath = Path.Combine(dataPath, "VersionInfo.json");

            List<Task> downloads = new();

            var jsonSerializer = new JsonSerializer
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy(),
                },
            };
            VersionInfo versionInfo = await GetJsonAsync<VersionInfo>(url, jsonSerializer) ??
                                            throw new HttpRequestException("Could not process response from AWS Server.");

            if (File.Exists(localVersionInfoPath))
            {
                VersionInfo localVersionInfo = JsonConvert.DeserializeObject<VersionInfo>(localVersionInfoPath) ?? throw new InvalidOperationException();

                if (localVersionInfo.CurrentVersionCode < versionInfo.CurrentVersionCode)
                {
                    foreach ((string key, var value) in versionInfo.Categories)
                    {
                        foreach (var item in value)
                        {
                            var currentFile = localVersionInfo.Categories[key].Find(x => x.FileName.Equals(item.FileName));
                            if (currentFile == null || currentFile!.Version < item.Version)
                            {
                                string fileName = $"{item.FileName}.json";
                                string fileUrl = $"{Host}/api/{server}/{key}/{fileName}";
                                string filePath = Path.Combine(dataPath, key);

                                if (!Directory.Exists(filePath))
                                {
                                    Directory.CreateDirectory(filePath);
                                }

                                downloads.Add(DownloadFileAsync(new Uri(fileUrl), Path.Combine(filePath, fileName)));
                            }
                        }
                    }

                    downloads.Add(DownloadFileAsync(new Uri(url), localVersionInfoPath));
                    await Task.WhenAll(downloads).ConfigureAwait(false);
                    await DownloadLanguage(true, pts).ConfigureAwait(false);
                }
            }
            else
            {
                foreach ((string key, var value) in versionInfo.Categories)
                {
                    foreach (var item in value)
                    {
                        string fileName = $"{item.FileName}.json";
                        string fileUrl = $"{Host}/api/{server}/{key}/{fileName}";
                        string filePath = Path.Combine(dataPath, key);
                        Debug.WriteLine(fileUrl);
                        if (!Directory.Exists(filePath))
                        {
                            Directory.CreateDirectory(filePath);
                        }

                        downloads.Add(DownloadFileAsync(new Uri(fileUrl), Path.Combine(filePath, fileName)));
                    }
                }

                downloads.Add(DownloadFileAsync(new Uri(url), localVersionInfoPath));
                await Task.WhenAll(downloads).ConfigureAwait(false);
                await DownloadLanguage(true, pts).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Downloads program languages.
        /// </summary>
        /// <param name="systemLocale">Downloads all the system locales if true otherwise just the main locale.</param>
        /// <param name="pts">If true donwloads PTS server data instead of the live one.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task DownloadLanguage(bool systemLocale, bool pts)
        {
            string server = "live";
            if (pts)
            {
                server = "pts";
            }

            string localePath = Path.Combine(AppDataHelper.AppDataDirectory, "api", server, "Localization");

            if (systemLocale)
            {
                CultureInfo[] localeList = CultureInfo.GetCultures(CultureTypes.AllCultures);

                if (!Directory.Exists(localePath))
                {
                    Directory.CreateDirectory(localePath);
                }

                List<Task> downloads = new();

                if (localeList.ToList().Find(x => x.Name.Equals(CultureInfo.CurrentUICulture.Name)) == null)
                {
                    string fileName = $"{CultureInfo.CurrentUICulture.Name}.json";
                    string url = $"{Host}/api/{server}/Localization/{fileName}";
                    string localeName = Path.Combine(localePath, fileName);
                    downloads.Add(DownloadFileAsync(new Uri(url), localeName));
                }

                foreach (var locale in localeList)
                {
                    string fileName = $"{locale.Name}.json";
                    string url = $"{Host}/api/{server}/Localization/{fileName}";
                    string localeName = Path.Combine(localePath, fileName);
                    downloads.Add(DownloadFileAsync(new Uri(url), localeName));
                }

                await Task.WhenAll(downloads).ConfigureAwait(false);
            }
            else
            {
                string fileName = $"{CultureInfo.CurrentUICulture.Name}.json";
                string url = $"{Host}/api/{server}/Localization/{fileName}";
                string localeName = Path.Combine(localePath, fileName);
                await DownloadFileAsync(new Uri(url), localeName).ConfigureAwait(false);
            }
        }
    }
}
