using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WoWsShipBuilder.Features.Builds;
using WoWsShipBuilder.Features.LinkShortening;

namespace WoWsShipBuilder.Desktop.Features.LinkShortening;

public class ApiLinkShortener : ILinkShortener
{
    private readonly ILogger<ApiLinkShortener> logger;

    private readonly LinkShorteningOptions linkShorteningOptions;

    private readonly HttpClient httpClient;

    public ApiLinkShortener(IOptions<LinkShorteningOptions> linkShorteningOptions, IHttpClientFactory httpClientFactory, ILogger<ApiLinkShortener> logger)
    {
        this.logger = logger;
        this.linkShorteningOptions = linkShorteningOptions.Value;
        this.httpClient = httpClientFactory.CreateClient();
    }

    public async Task<LinkShorteningResult> CreateLinkForBuild(Build build)
    {
        string buildString = build.CreateShortStringFromBuild();
        string encodedBuild = WebUtility.UrlEncode(buildString);

        var path = $"/ship?shipIndexes={build.ShipIndex}&build={encodedBuild}";
        var link = $"{this.linkShorteningOptions.LinkBaseUrl}{path}";
        return await this.CreateShortLink(link);
    }

    public async Task<LinkShorteningResult> CreateShortLink(string link)
    {
        var request = new ShortlinkRequest(link);
        var response = await this.httpClient.PostAsJsonAsync(this.linkShorteningOptions.ApiUrl, request);

        LinkShorteningResult? result;
        try
        {
            result = await response.Content.ReadFromJsonAsync<LinkShorteningResult>();
        }
        catch (JsonException)
        {
            result = null;
        }

        if (result is null)
        {
            this.logger.LogError("Failed to shorten link {Link}", link);
            result = new(false, link);
        }

        return result;
    }
}
