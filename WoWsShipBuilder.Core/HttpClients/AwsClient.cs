using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.HttpResponses;

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
                    url = @$"{Host}/images/ship/{index}.png";
                    localFolder = "Ships";
                }
                else
                {
                    url = @$"{Host}/images/camo/{index}.png";
                    localFolder = "Camos";
                }

                string folderPath = Path.Combine(AppDataHelper.AppDataDirectory, "Images", localFolder);

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string fileName = Path.Combine(folderPath, $"{index}.png");

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
    }
}
