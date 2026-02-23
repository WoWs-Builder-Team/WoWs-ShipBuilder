using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using RichardSzalay.MockHttp;
using WoWsShipBuilder.Features.LinkShortening;

namespace WoWsShipBuilder.Web.Test.LinkShorteningTests;

[TestFixture]
public class ShlinkLinkShortenerTests
{
    [Test]
    public async Task CreateShortLink_NormalLink_ShorteningSuccessful()
    {
        const string testLink = "https://app.wowssb.com/charts";
        var options = new LinkShorteningOptions
        {
            ApiKey = "1234",
            ApiServer = "https://share.wowssb.com",
        };
        var mockHttp = new MockHttpMessageHandler();
        var request = mockHttp.When(HttpMethod.Post, $"{options.ApiServer}/rest/v3/short-urls")
            .WithHeaders("X-Api-Key", options.ApiKey)
            .Respond("application/json", """{ "shortUrl": "https://share.wowssb.com/1234", "shortCode": "1234" }""");
        var logger = new Mock<ILogger<ShlinkLinkShortener>>();
        var linkShortener = new ShlinkLinkShortener(mockHttp.ToHttpClient(), Options.Create(options), logger.Object);

        var result = await linkShortener.CreateShortLink(testLink, string.Empty);

        result.Link.Should().NotBeEmpty();
        result.Shortened.Should().BeTrue();
        mockHttp.GetMatchCount(request).Should().Be(1);
    }

    [Test]
    public void CreateShortLink_ApiKeySet_IsAvailable()
    {
        var options = new LinkShorteningOptions
        {
            ApiKey = "1234",
            ApiServer = "https://share.wowssb.com",
        };
        var mockHttp = new MockHttpMessageHandler();
        var logger = new Mock<ILogger<ShlinkLinkShortener>>();
        var linkShortener = new ShlinkLinkShortener(mockHttp.ToHttpClient(), Options.Create(options), logger.Object);

        bool result = linkShortener.IsAvailable;

        result.Should().BeTrue();
    }

    [Test]
    public void CreateShortLink_ApiKeyNotSet_IsNotAvailable()
    {
        var options = new LinkShorteningOptions
        {
            ApiKey = string.Empty,
            ApiServer = "https://share.wowssb.com",
        };
        var mockHttp = new MockHttpMessageHandler();
        var logger = new Mock<ILogger<ShlinkLinkShortener>>();
        var linkShortener = new ShlinkLinkShortener(mockHttp.ToHttpClient(), Options.Create(options), logger.Object);

        bool result = linkShortener.IsAvailable;

        result.Should().BeFalse();
    }
}
