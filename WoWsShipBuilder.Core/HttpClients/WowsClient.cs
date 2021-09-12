using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilder.Core.HttpResponses;

namespace WoWsShipBuilder.Core.HttpClients
{
    public class WowsClient
    {
        #region Static Fields and Constants

        private static readonly Lazy<WowsClient> InstanceValue = new(() => new WowsClient());

        private static readonly string Host = "https://api.worldofwarships.ru/wows";

        #endregion

        private readonly HttpClient client;

        private WowsClient()
        {
            client = new HttpClient();
        }

        public static WowsClient Instance => InstanceValue.Value;

        /// <summary>
        /// Downloads the images of ships and camos.
        /// </summary>
        /// <param name="appId">ID of the application to access WG API.</param>
        /// <param name="request">Dictionary containing as key the ID and as value the index of ships or camos.</param>
        /// <param name="type">Can be either ship or camo.</param>
        /// <param name="sizes">The size of the image to download. If type is camo then sizes can be null.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        /// <exception cref="WowsApiException">Occurs if the API call fails or returns an incorrect response.</exception>
        public async Task DownloadShipsOrCamosImages(string appId, Dictionary<long, string> request, ImageType type, List<ImageSize> sizes)
        {
            Dictionary<long, ImageData?> data;
            List<long> id = request.Keys.ToList();

            if (type == ImageType.Ship)
            {
                data = await GetShipsImagesDownloadLinks(appId, id).ConfigureAwait(false);
            }
            else
            {
                data = await GetCamosImagesDownloadLinks(appId, id).ConfigureAwait(false);
            }

            Dictionary<long, ImageData> finalData = data.Where(entry => entry.Value != null)
                .ToDictionary(entry => entry.Key, entry => entry.Value!);

            await DownloadImages(finalData, request, type, sizes).ConfigureAwait(false);
        }

        internal async Task<WowsApiResponse<T>> Get<T>(string appId, string resource, string[] field, params (string Key, object Value)[] additionalParameters)
            where T : IWowsApiResponseData
        {
            var paramsString = $"{(additionalParameters.Any() ? "&" : "")}{string.Join("&", additionalParameters.Select(p => $"{p.Key}={p.Value}"))}";
            var fields = $"{(field.Any() ? "&fields=" : "")}{string.Join(",", field)}";
            var url = $"{Host}/{resource}/?application_id={appId}&language=en{fields}{paramsString}";

            await using Stream stream = await client.GetStreamAsync(url);
            using var streamReader = new StreamReader(stream);
            using var jsonReader = new JsonTextReader(streamReader);
            var serializer = new JsonSerializer
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy(),
                },
            };
            WowsApiResponse<T> result = serializer.Deserialize<WowsApiResponse<T>>(jsonReader) ??
                                        throw new WowsApiException("Could not process response from WG API.");

            if (result.Error != null)
            {
                throw new WowsApiException("Status: Error", result.Error.Field, result.Error.Message, result.Error.Code, result.Error.Value);
            }

            return result;
        }

        private async Task<Dictionary<long, ImageData?>> GetShipsImagesDownloadLinks(string appId, List<long> id)
        {
            // Request a list of ship data with all specified ship ids.
            var result = await Get<ImageData>(appId, "encyclopedia/ships", new[] { "images" }, ("ship_id", string.Join(",", id)))
                .ConfigureAwait(false);

            Dictionary<long, ImageData?> data;
            if (result.Meta.Total == null || id.Count != result.Meta.Total)
            {
                List<long> errors = new();
                foreach ((long key, ImageData? value) in result.Data)
                {
                    if (value?.ShipImages == null)
                    {
                        errors.Add(key);
                    }
                }

                throw new WowsApiException("Unsuccessful call for some ships", string.Join(",", errors));
            }

            data = result.Data;

            if (result.Meta.TotalPages > 1)
            {
                for (int i = (result.Meta.Page ?? 1) + 1; i < result.Meta.TotalPages + 1; i++)
                {
                    (string, object)[] moreParams = { ("page_no", i), ("ship_id", string.Join(",", id)) };
                    var tmpResult = await Get<ImageData>(appId, "encyclopedia/ships", new[] { "images" }, moreParams)
                        .ConfigureAwait(false);
                    if (tmpResult.Meta.Total == null || id.Count != tmpResult.Meta.Total)
                    {
                        List<long> errors = new();
                        foreach ((long key, ImageData? value) in tmpResult.Data)
                        {
                            if (value?.ShipImages == null)
                            {
                                errors.Add(key);
                            }
                        }

                        throw new WowsApiException("Unsuccessful call for some ships", string.Join(",", errors));
                    }

                    data.AddDict(tmpResult.Data);
                }
            }

            return data;
        }

        private async Task<Dictionary<long, ImageData?>> GetCamosImagesDownloadLinks(string appId, List<long> id)
        {
            var result = await Get<ImageData>(appId, "encyclopedia/consumables", new[] { "image" }, ("consumable_id", string.Join(",", id)))
                               .ConfigureAwait(false);

            Dictionary<long, ImageData?> data;
            if (result.Meta.Total == null || id.Count != result.Meta.Total)
            {
                List<long> errors = new();
                foreach ((long key, ImageData? value) in result.Data)
                {
                    if (value?.CamoImage == null)
                    {
                        errors.Add(key);
                    }
                }

                throw new WowsApiException("Unsuccessful call for some camos", string.Join(",", errors));
            }

            data = result.Data;
            if (result.Meta.TotalPages > 1)
            {
                for (int i = (result.Meta.Page ?? 1) + 1; i < result.Meta.TotalPages + 1; i++)
                {
                    ValueTuple<string, object>[] moreParams = { ("page_no", i), ("consumable_id", string.Join(",", id)) };
                    var tmpResult = await Get<ImageData>(appId, "encyclopedia/consumables", new[] { "image" }, moreParams)
                        .ConfigureAwait(false);
                    if (tmpResult.Meta.Total == null || id.Count != tmpResult.Meta.Total)
                    {
                        List<long> errors = new();
                        foreach ((long key, var value) in tmpResult.Data)
                        {
                            if (value?.CamoImage == null)
                            {
                                errors.Add(key);
                            }
                        }

                        throw new WowsApiException("Unsuccessful call for some camos", string.Join(",", errors));
                    }

                    data.AddDict(tmpResult.Data);
                }
            }

            return data;
        }

        private async Task DownloadImages(Dictionary<long, ImageData> data, Dictionary<long, string> request, ImageType type, List<ImageSize> sizes)
        {
            List<Task> downloads = new();
            if (type == ImageType.Ship)
            {
                foreach ((long key, var value) in data)
                {
                    foreach (ImageSize size in sizes)
                    {
                        string imageSize = size == ImageSize.Small ? "" : $"_{size}";

                        string folderPath = Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                            "WoWsShipBuilder",
                            "Images",
                            "Ships");

                        if (!Directory.Exists(folderPath))
                        {
                            Directory.CreateDirectory(folderPath);
                        }

                        string fileName = Path.Combine(folderPath, $"{request[key]}{imageSize}.png");
                        downloads.Add(DownloadFileAsync(new Uri(value.ShipImages![size]), fileName));
                    }
                }
            }
            else
            {
                foreach ((long key, var value) in data)
                {
                    string folderPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "WoWsShipBuilder",
                        "Images",
                        "Camos");

                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    string fileName = Path.Combine(folderPath, $"{request[key]}.png");
                    downloads.Add(DownloadFileAsync(new Uri(value.CamoImage!), fileName));
                }
            }

            await Task.WhenAll(downloads).ConfigureAwait(false);
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
