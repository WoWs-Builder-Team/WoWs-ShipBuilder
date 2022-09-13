using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using WoWsShipBuilder.Core.Builds;

namespace WoWsShipBuilder.Web.LinkShortening;

public class FirebaseLinkShortener : ILinkShortener
{
    private readonly HttpClient httpClient;

    private readonly LinkShorteningOptions options;

    private readonly ILogger<FirebaseLinkShortener> logger;

    private readonly SemaphoreSlim semaphore;

    public FirebaseLinkShortener(HttpClient httpClient, IOptions<LinkShorteningOptions> options, ILogger<FirebaseLinkShortener> logger)
    {
        this.httpClient = httpClient;
        this.options = options.Value;
        this.logger = logger;
        semaphore = new(this.options.RateLimit, this.options.RateLimit);
    }

    public async Task<ShorteningResult> CreateLinkForBuild(Build build)
    {
        logger.LogInformation("Creating short link for build {}", build.Hash);
        string buildString = build.CreateStringFromBuild();
        string encodedBuild = WebUtility.UrlEncode(buildString);

        var path = $"/ship?shipIndexes={build.ShipIndex}&build={encodedBuild}";
        var serializerOptions = new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter() },
            PropertyNameCaseInsensitive = true,
        };

        var request = new DynamicLinkRequest(new(options.UriPrefix, options.LinkBaseUrl + path), new(LinkSuffixType.SHORT));
        if (!await semaphore.WaitAsync(options.RequestTimeout))
        {
            logger.LogWarning("Timeout while waiting for dynamic link api access");
            return new(false, request.DynamicLinkInfo.Link);
        }

        var response = await httpClient.PostAsJsonAsync(options.ApiUrl + options.ApiKey, request, serializerOptions);
#pragma warning disable CS4014
        Task.Run(async () => await ResetLock());
#pragma warning restore CS4014
        var result = await response.Content.ReadFromJsonAsync<DynamicLinkResponse>() ?? throw new InvalidOperationException();
        logger.LogInformation("Successfully created short link {} for build {}", result.ShortLink, build.Hash);
        return new(true, result.ShortLink);
    }

    private async Task ResetLock()
    {
        logger.LogDebug("Scheduling reset for semaphore permit. Available permits: {}", semaphore.CurrentCount);
        await Task.Delay(1000);
        semaphore.Release();
        logger.LogDebug("Permit released. Available permits: {}", semaphore.CurrentCount);
    }
}
