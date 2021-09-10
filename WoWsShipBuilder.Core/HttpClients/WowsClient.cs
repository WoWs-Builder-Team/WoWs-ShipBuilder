using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WoWsShipBuilder.Core.HttpResponses;

namespace WoWsShipBuilder.Core.HttpClients
{
    public class WowsClient : WebClient
    {
        private static Lazy<WowsClient> instance = new Lazy<WowsClient>(() => new WowsClient());

        private readonly HttpClient client;

        private WowsClient()
        {
            client = new HttpClient();
        }

        public static WowsClient Instance => instance.Value;

        private async Task<WowsApiResponse<T>> Get<T>(string appId, string resource, string[] field, params ValueTuple<string, object>[] additionalParameters)
        {
            var host = $"https://api.worldofwarships.ru/wows";

            var paramsString = $"{(additionalParameters.Any() ? "&" : "")}{string.Join("&", additionalParameters.Select(p => $"{p.Item1}={p.Item2}"))}";

            var fields = $"{(field.Any() ? "&fields=" : "")}{string.Join(",", field)}";

            var url = $"{host}/{resource}/?application_id={appId}&language=en{fields}{paramsString}";

            var response = await client.GetAsync(url).ConfigureAwait(false);

            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException)
            {
                throw new WowsApiException("Unsuccessful WG API call");
            }

            var responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var result = JsonConvert.DeserializeObject<WowsApiResponse<T>>(responseText, new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy(),
                },
            }) ?? throw new InvalidOperationException();

            if (result.Error != null)
            {
                throw new WowsApiException("Status: Error", result.Error.Field, result.Error.Message, result.Error.Code, result.Error.Value);
            }

            return result;
        }

        /// <summary>
        /// Downloads the images of ships and camos.
        /// </summary>
        /// <param name="appId">ID of the application to access WG API.</param>
        /// <param name="request">Dictionary containing as key the ID and as value the index of ships or camos.</param>
        /// <param name="type">Can be either ship or camo.</param>
        /// <param name="sizes">The size of the image to download. If type is camo then sizes can be null.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">Occurs if the API response cant be deserialized.</exception>
        /// <exception cref="WowsApiException">Occurs if the API call fails or returns an incorrect response.</exception>
        public static async Task DownloadShipsOrCamosImages(string appId, Dictionary<long, string> request, ImageType type, List<ImageSize> sizes)
        {
            var client = WowsClient.Instance;
            Dictionary<long, ImageData> data;
            List<long> id = request.Keys.ToList<long>();

            if (type == ImageType.Ship)
            {
                var result = await client.Get<Dictionary<long, ImageData>>(appId, "encyclopedia/ships", new[] { "images" }, ("ship_id", string.Join(",", id))).ConfigureAwait(false);

                if (result.Meta.Total == null || id.Count != result.Meta.Total)
                {
                    List<long> errors = new();
                    foreach ((var key, var value) in result.Data)
                    {
                        if (value.ShipImages == null)
                        {
                            errors.Add(key);
                        }
                    }

                    throw new WowsApiException("Unsuccessful call for some ships", string.Join(",", errors));
                }

                if (result.Meta.Page_total > 1)
                {
                    Dictionary<long, ImageData>[] datas = new Dictionary<long, ImageData>[(int)result.Meta.Page_total];

                    datas[result.Meta.Page ?? 1] = result.Data;

                    for (int i = (result.Meta.Page ?? 1) + 1; i < result.Meta.Page_total + 1; i++)
                    {
                        ValueTuple<string, object>[] moreParams = new ValueTuple<string, object>[] { ("page_no", i), ("ship_id", string.Join(",", id)) };
                        var tmpResult = await client.Get<Dictionary<long, ImageData>>(appId, "encyclopedia/ships", new[] { "images" }, moreParams).ConfigureAwait(false);
                        if (tmpResult.Meta.Total == null || id.Count != tmpResult.Meta.Total)
                        {
                            List<long> errors = new();
                            foreach ((var key, var value) in tmpResult.Data)
                            {
                                if (value.ShipImages == null)
                                {
                                    errors.Add(key);
                                }
                            }

                            throw new WowsApiException("Unsuccessful call for some ships", string.Join(",", errors));
                        }

                        datas[i] = tmpResult.Data;
                    }

                    data = datas.SelectMany(dict => dict).ToDictionary(pair => pair.Key, pair => pair.Value);
                }
                else
                {
                    data = result.Data;
                }
            }
            else
            {
                var result = await client.Get<Dictionary<long, ImageData>>(appId, "encyclopedia/consumables", new[] { "image" }, ("consumable_id", string.Join(",", id))).ConfigureAwait(false);

                if (result.Meta.Total == null || id.Count != result.Meta.Total)
                {
                    List<long> errors = new();
                    foreach ((var key, var value) in result.Data)
                    {
                        if (value.CamoImage == null)
                        {
                            errors.Add(key);
                        }
                    }

                    throw new WowsApiException("Unsuccessful call for some camos", string.Join(",", errors));
                }

                if (result.Meta.Page_total > 1)
                {
                    Dictionary<long, ImageData>[] datas = new Dictionary<long, ImageData>[(int)result.Meta.Page_total];
                    datas[result.Meta.Page ?? 1] = result.Data;
                    for (int i = (result.Meta.Page ?? 1) + 1; i < result.Meta.Page_total + 1; i++)
                    {
                        ValueTuple<string, object>[] moreParams = new ValueTuple<string, object>[] { ("page_no", i), ("consumable_id", string.Join(",", id)) };
                        var tmpResult = await client.Get<Dictionary<long, ImageData>>(appId, "encyclopedia/consumables", new[] { "image" }, moreParams).ConfigureAwait(false);
                        if (tmpResult.Meta.Total == null || id.Count != tmpResult.Meta.Total)
                        {
                            List<long> errors = new();
                            foreach ((var key, var value) in tmpResult.Data)
                            {
                                if (value.CamoImage == null)
                                {
                                    errors.Add(key);
                                }
                            }

                            throw new WowsApiException("Unsuccessful call for some camos", string.Join(",", errors));
                        }

                        datas[i] = tmpResult.Data;
                    }

                    data = datas.SelectMany(dict => dict).ToDictionary(pair => pair.Key, pair => pair.Value);
                }
                else
                {
                    data = result.Data;
                }
            }

            await DownloadImages(data, request, type, sizes).ConfigureAwait(false);
        }

        private static async Task DownloadImages(Dictionary<long, ImageData> data, Dictionary<long, string> request, ImageType type, List<ImageSize> sizes)
        {
            var client = WowsClient.Instance;
            List<Task> downloads = new();
            if (type == ImageType.Ship)
            {
                foreach ((var key, var value) in data)
                {
                    foreach (var size in sizes)
                    {
                        string imageSize;
                        if (size == ImageSize.Small)
                        {
                            imageSize = "";
                        }
                        else
                        {
                            imageSize = $"_{size}";
                        }

                        var fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WoWsShipBuilder", "Images", "Ships", $"{request.GetValueOrDefault(key)}{imageSize}.png");
                        downloads.Add(client.DownloadFileTaskAsync(new Uri(value.ShipImages[size]), fileName));
                    }
                }
            }
            else
            {
                foreach ((var key, var value) in data)
                {
                    var fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WoWsShipBuilder", "Images", "Camos", $"{request.GetValueOrDefault(key)}.png");
                    downloads.Add(client.DownloadFileTaskAsync(new Uri(value.CamoImage), fileName));
                }
            }

            await Task.WhenAll(downloads).ConfigureAwait(false);
        }
    }
}
