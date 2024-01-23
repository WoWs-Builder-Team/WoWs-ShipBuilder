using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WoWsShipBuilder.Features.LinkShortening;

namespace WoWsShipBuilder.Web.Features.LinkShortening;

[Route("api/[controller]")]
[ApiController]
public class ShortLinkController : ControllerBase
{
    private readonly ILogger<ShortLinkController> logger;
    private readonly ILinkShortener linkShortener;
    private readonly Uri targetBaseUri;

    public ShortLinkController(IOptions<LinkShorteningOptions> options, ILinkShortener linkShortener, ILogger<ShortLinkController> logger)
    {
        this.linkShortener = linkShortener;
        this.logger = logger;
        this.targetBaseUri = new(options.Value.LinkBaseUrl);
    }

    [HttpPost("create")]
    public async Task<LinkShorteningResult> CreateShortLink([FromBody] ShortlinkRequest shortlinkRequest)
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
