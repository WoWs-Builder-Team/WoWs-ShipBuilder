using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using WoWsShipBuilder.Features.Builds;
using WoWsShipBuilder.Features.LinkShortening;

namespace WoWsShipBuilder.Web.Features.LinkShortening;

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

    private readonly bool linkShorteningAvailable;

    public FirebaseLinkShortener(HttpClient httpClient, IOptions<LinkShorteningOptions> options, ILogger<FirebaseLinkShortener> logger)
    {
        this.httpClient = httpClient;
        this.options = options.Value;
        this.logger = logger;
        this.semaphore = new(this.options.RateLimit, this.options.RateLimit);
        this.linkShorteningAvailable = !string.IsNullOrEmpty(this.options.ApiKey);
    }

    public void Dispose()
    {
        this.semaphore.Dispose();
    }

    public async Task<LinkShorteningResult> CreateLinkForBuild(Build build)
    {
        this.logger.LogInformation("Creating short link for build {BuildHash}", build.Hash);
        string buildString = build.CreateShortStringFromBuild();
        string encodedBuild = WebUtility.UrlEncode(buildString);

        var path = $"/ship?shipIndexes={build.ShipIndex}&build={encodedBuild}";

        var request = new DynamicLinkRequest(new(this.options.UriPrefix, this.options.LinkBaseUrl + path), new(LinkSuffixType.SHORT));
        return await this.SendRequestAsync(request);
    }

    public async Task<LinkShorteningResult> CreateShortLink(string link)
    {
        this.logger.LogInformation("Creating short link for link {Link}", link);
        var request = new DynamicLinkRequest(new(this.options.UriPrefix, link), new(LinkSuffixType.SHORT));
        return await this.SendRequestAsync(request);
    }

    private async Task<LinkShorteningResult> SendRequestAsync(DynamicLinkRequest linkRequest)
    {
        if (!this.linkShorteningAvailable)
        {
            this.logger.LogInformation("Link shortening is not available. Returning original link");
            return new(false, linkRequest.DynamicLinkInfo.Link);
        }

        if (!await this.semaphore.WaitAsync(this.options.RequestTimeout))
        {
            this.logger.LogWarning("Timeout while waiting for dynamic link api access");
            return new(false, linkRequest.DynamicLinkInfo.Link);
        }

        var response = await this.httpClient.PostAsJsonAsync(this.options.ApiUrl + this.options.ApiKey, linkRequest, this.serializerOptions);
#pragma warning disable CS4014
        Task.Run(async () => await this.ResetLock());
#pragma warning restore CS4014
        var result = await response.Content.ReadFromJsonAsync<DynamicLinkResponse>() ?? throw new InvalidOperationException();
        return new(true, result.ShortLink);
    }

    private async Task ResetLock()
    {
        this.logger.LogDebug("Scheduling reset for semaphore permit. Available permits: {PermitCount}", this.semaphore.CurrentCount);
        await Task.Delay(1000);
        this.semaphore.Release();
        this.logger.LogDebug("Permit released. Available permits: {PermitCount}", this.semaphore.CurrentCount);
    }
}
