using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NLog;
using WoWsShipBuilder.Core;

namespace WoWsShipBuilder.UI.Updater
{
    public static class ApplicationUpdater
    {
        public static async Task<GithubApiResponse?> GetLatestVersionNumber()
        {
            Logger logger = Logging.GetLogger("UpdateCheck");
            HttpClient client = new();

            var host = @"https://api.github.com";
            var paramString = "repos/WoWs-Builder-Team/WoWs-ShipBuilder/releases/latest";
            var url = $"{host}/{paramString}";
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Get,
            };
            request.Headers.Add("Accept", @"application/vnd.github.v3+json");
            request.Headers.Add("User-Agent", @"Unnamed-Stats-Viewer");

            HttpResponseMessage response;
            try
            {
                response = await client.SendAsync(request).ConfigureAwait(false);
            }
            catch (HttpRequestException e)
            {
                Logging.Logger.Error(e, "GitHub connection failed.");
                return null;
            }

            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                logger.Error(e, "An exception occurred during the version check. Error Code: {0}", response.StatusCode);
                return null;
            }

            string responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var result = JsonConvert.DeserializeObject<GithubApiResponse>(responseText, new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy(),
                },
            });

            return result;
        }
    }
}
