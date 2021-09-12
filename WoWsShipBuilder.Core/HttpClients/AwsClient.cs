using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using WoWsShipBuilder.Core.HttpResponses;

namespace WoWsShipBuilder.Core.HttpClients
{
    public class AwsClient
    {
        #region Static Fields and Constants

        private static readonly Lazy<AwsClient> InstanceValue = new(() => new AwsClient());

        private static readonly string Host = "https://d2nzlaerr9l5k3.cloudfront.net";

        #endregion

        private readonly HttpClient client;

        private AwsClient()
        {
            client = new HttpClient();
        }

        public static AwsClient Instance => InstanceValue.Value;

        /// <summary>
        /// Downloads images from the AWS server.
        /// </summary>
        /// <param name="indexList">List of images indexes to download.</param>
        /// <param name="type">The type of images to donwload. Can be either Ship or Camo.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        /// <exception cref = "HttpRequestException" > Occurs if the server does not respond properly.</exception>
        /// <exception cref = "ArgumentNullException" > Occurs if files are not available on the server.</exception>
        public async Task DownloadImages(List<string> indexList, ImageType type)
        {
            List<Task> downloads = new();

            foreach (var index in indexList)
            {
                string url;
                string localFolder;
                if (type == ImageType.Ship)
                {
                    url = @$"{Host}/images/ships/{index}.png";
                    localFolder = "Ships";
                }
                else
                {
                    url = @$"{Host}/images/camos/{index}.png";
                    localFolder = "Camos";
                }

                string fileName = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "WoWsShipBuilder",
                    "Images",
                    localFolder,
                    $"{index}.png");

                downloads.Add(DownloadFileAsync(new Uri(url), fileName));
            }

            await Task.WhenAll(downloads).ConfigureAwait(false);
        }

        /// <summary>
        /// Downloads all the images stored into a .zip file on the server and saves them into the local folder for images. Then deletes the .zip file.
        /// </summary>
        /// <param name="zipName">Name of the .zip file (must include .zip extension).</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        /// <exception cref = "HttpRequestException" > Occurs if the server does not respond properly.</exception>
        /// <exception cref = "ArgumentNullException" > Occurs if the file is not available on the server.</exception>
        public async Task DownloadAllImages(string zipName)
        {
            string zipUrl = @$"{Host}/images/{zipName}";
            string directoryPath = Path.Combine(
                   Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                   "WoWsShipBuilder",
                   "Images");
            string zipPath = Path.Combine(directoryPath, zipName);
            await DownloadFileAsync(new Uri(zipUrl), zipPath);
            ZipFile.ExtractToDirectory(zipPath, directoryPath);
            File.Delete(zipPath);
        }

        private async Task DownloadFileAsync(Uri uri, string fileName)
        {
            await using Stream stream = await client.GetStreamAsync(uri);
            var fileInfo = new FileInfo(fileName);
            await using FileStream fileStream = fileInfo.OpenWrite();
            await stream.CopyToAsync(fileStream);
        }
    }
}
