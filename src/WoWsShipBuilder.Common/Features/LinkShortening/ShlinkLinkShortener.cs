using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WoWsShipBuilder.Features.Builds;

namespace WoWsShipBuilder.Features.LinkShortening;

public class ShlinkLinkShortener : ILinkShortener
{
    private readonly ILogger<ShlinkLinkShortener> logger;

    private readonly HttpClient httpClient;

    private readonly LinkShorteningOptions options;

    private readonly JsonSerializerOptions serializerOptions = new()
    {
        Converters = { new JsonStringEnumConverter() },
        PropertyNameCaseInsensitive = true,
    };

    public ShlinkLinkShortener(HttpClient httpClient, IOptions<LinkShorteningOptions> options, ILogger<ShlinkLinkShortener> logger)
    {
        this.httpClient = httpClient;
        this.logger = logger;
        this.options = options.Value;
        this.IsAvailable = !string.IsNullOrWhiteSpace(this.options.ApiServer) && !string.IsNullOrWhiteSpace(this.options.ApiKey);
    }

    public bool IsAvailable { get; }

    public async Task<ShorteningResult> CreateLinkForBuild(Build build)
    {
        this.logger.LogInformation("Creating short link for build {BuildHash}", build.Hash);
        string buildString = build.CreateShortStringFromBuild();
        string encodedBuild = WebUtility.UrlEncode(buildString);

        var path = $"/ship?shipIndexes={build.ShipIndex}&build={encodedBuild}";

        var request = new ShlinkRequest(
            this.options.LinkBaseUrl + path,
            build.BuildName,
            this.options.Domain,
            true,
            ["build", "user-build"]);
        return await this.SendRequestAsync(request);
    }

    public async Task<ShorteningResult> CreateShortLink(string link, string buildName)
    {
        this.logger.LogInformation("Creating short link for link {Link}", link);
        var request = new ShlinkRequest(
            link,
            buildName,
            this.options.Domain,
            true,
            ["build", "user-build"]);
        return await this.SendRequestAsync(request);
    }

    private async Task<ShorteningResult> SendRequestAsync(ShlinkRequest linkRequest)
    {
        var url = $"{this.options.ApiServer}/rest/v3/short-urls";
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(linkRequest, options: this.serializerOptions),
            Headers = { { "X-Api-Key", this.options.ApiKey } },
        };
        var response = await this.httpClient.SendAsync(requestMessage);
        var result = await response.Content.ReadFromJsonAsync<ShlinkResponse>() ?? throw new InvalidOperationException();
        return new(true, result.ShortUrl);
    }
}
