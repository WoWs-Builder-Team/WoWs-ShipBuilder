using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WoWsShipBuilder.Features.Builds;

namespace WoWsShipBuilder.Features.LinkShortening;

public sealed class FirebaseLinkShortener : ILinkShortener, IDisposable
{
    private readonly HttpClient httpClient;

    private readonly ILogger<FirebaseLinkShortener> logger;

    private readonly LinkShorteningOptions options;

    private readonly SemaphoreSlim semaphore;

    private readonly JsonSerializerOptions serializerOptions = new()
    {
        Converters = { new JsonStringEnumConverter() },
        PropertyNameCaseInsensitive = true,
    };

    public FirebaseLinkShortener(HttpClient httpClient, IOptions<LinkShorteningOptions> options, ILogger<FirebaseLinkShortener> logger)
    {
        this.httpClient = httpClient;
        this.options = options.Value;
        this.logger = logger;
        semaphore = new(this.options.RateLimit, this.options.RateLimit);
        IsAvailable = !string.IsNullOrEmpty(this.options.ApiKey);
    }

    public bool IsAvailable { get; }

    public void Dispose()
    {
        semaphore.Dispose();
    }

    public async Task<ShorteningResult> CreateLinkForBuild(Build build)
    {
        logger.LogInformation("Creating short link for build {BuildHash}", build.Hash);
        string buildString = build.CreateShortStringFromBuild();
        string encodedBuild = WebUtility.UrlEncode(buildString);

        var path = $"/ship?shipIndexes={build.ShipIndex}&build={encodedBuild}";

        var request = new DynamicLinkRequest(new(options.UriPrefix, options.LinkBaseUrl + path), new(LinkSuffixType.SHORT));
        return await SendRequestAsync(request);
    }

    public async Task<ShorteningResult> CreateShortLink(string link)
    {
        logger.LogInformation("Creating short link for link {Link}", link);
        var request = new DynamicLinkRequest(new(options.UriPrefix, link), new(LinkSuffixType.SHORT));
        return await SendRequestAsync(request);
    }

    private async Task<ShorteningResult> SendRequestAsync(DynamicLinkRequest linkRequest)
    {
        if (!await semaphore.WaitAsync(options.RequestTimeout))
        {
            logger.LogWarning("Timeout while waiting for dynamic link api access");
            return new(false, linkRequest.DynamicLinkInfo.Link);
        }

        var response = await httpClient.PostAsJsonAsync(options.ApiUrl + options.ApiKey, linkRequest, serializerOptions);
#pragma warning disable CS4014
        Task.Run(async () => await ResetLock());
#pragma warning restore CS4014
        var result = await response.Content.ReadFromJsonAsync<DynamicLinkResponse>() ?? throw new InvalidOperationException();
        return new(true, result.ShortLink);
    }

    private async Task ResetLock()
    {
        logger.LogDebug("Scheduling reset for semaphore permit. Available permits: {PermitCount}", semaphore.CurrentCount);
        await Task.Delay(1000);
        semaphore.Release();
        logger.LogDebug("Permit released. Available permits: {PermitCount}", semaphore.CurrentCount);
    }
}
