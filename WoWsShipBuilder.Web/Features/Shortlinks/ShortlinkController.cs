using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WoWsShipBuilder.Features.LinkShortening;

namespace WoWsShipBuilder.Web.Features.Shortlinks;

[Route("api/[controller]")]
[ApiController]
public class ShortlinkController : ControllerBase
{
    private readonly ILogger<ShortlinkController> logger;
    private readonly ILinkShortener linkShortener;
    private readonly Uri targetBaseUri;

    public ShortlinkController(IOptions<LinkShorteningOptions> options, ILinkShortener linkShortener, ILogger<ShortlinkController> logger)
    {
        this.linkShortener = linkShortener;
        this.logger = logger;
        this.targetBaseUri = new(options.Value.LinkBaseUrl);
    }

    [HttpPost]
    public async Task<ShorteningResult> CreateShortLink([FromBody] ShortlinkRequest shortlinkRequest)
    {
        var uri = new Uri(shortlinkRequest.TargetUrl);
        if (!this.targetBaseUri.IsBaseOf(uri))
        {
            this.logger.LogError("Target URL {TargetUrl} is not a valid target for link shortening", shortlinkRequest.TargetUrl);
            return new(false, string.Empty);
        }

        return await this.linkShortener.CreateShortLink(shortlinkRequest.TargetUrl);
    }
}

public sealed record ShortlinkRequest(string TargetUrl);
