using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using RichardSzalay.MockHttp;
using WoWsShipBuilder.Web.LinkShortening;

namespace WoWsShipBuilder.Web.Test.LinkShorteningTests;

[TestFixture]
public class FirebaseLinkShortenerTests
{
    [Test]
    public async Task CreateShortLink_NormalLink_ShorteningSuccessful()
    {
        const string testLink = "https://app.wowssb.com/charts";
        var options = new LinkShorteningOptions
        {
            ApiKey = "1234",
            ApiUrl = "https://www.example.com/",
        };
        var mockHttp = new MockHttpMessageHandler();
        var request = mockHttp.When(HttpMethod.Post, options.ApiUrl + options.ApiKey).Respond("application/json", @"{ ""shortLink"": ""https://share.wowssb.com/1234"", ""previewLink"": ""https://share.wowssb.com/1234?d=1"" }");
        var logger = new Mock<ILogger<FirebaseLinkShortener>>();
        var linkShortener = new FirebaseLinkShortener(mockHttp.ToHttpClient(), Options.Create(options), logger.Object);

        var result = await linkShortener.CreateShortLink(testLink);

        result.Link.Should().NotBeEmpty();
        result.Shortened.Should().BeTrue();
        mockHttp.GetMatchCount(request).Should().Be(1);
    }

    [Test]
    public async Task CreateShortLink_NormalLinkRateLimitExceeded_ShorteningFailed()
    {
        const string testLink = "https://app.wowssb.com/charts";
        var options = new LinkShorteningOptions
        {
            ApiKey = "1234",
            ApiUrl = "https://www.example.com/",
            RequestTimeout = 0,
        };
        var mockHttp = new MockHttpMessageHandler();
        var request = mockHttp.When(HttpMethod.Post, options.ApiUrl + options.ApiKey).Respond("application/json", @"{ ""shortLink"": ""https://share.wowssb.com/1234"", ""previewLink"": ""https://share.wowssb.com/1234?d=1"" }");
        var logger = new Mock<ILogger<FirebaseLinkShortener>>();
        var linkShortener = new FirebaseLinkShortener(mockHttp.ToHttpClient(), Options.Create(options), logger.Object);

        for (var i = 0; i < 5; i++)
        {
            await linkShortener.CreateShortLink(testLink);
        }

        var result = await linkShortener.CreateShortLink(testLink);

        result.Link.Should().NotBeEmpty();
        result.Shortened.Should().BeFalse();
        mockHttp.GetMatchCount(request).Should().Be(5);
    }

    [Test]
    [Ignore("disabled due to issues during app deployment")]
    public async Task CreateShortLink_NormalLinkRateLimitExceeded_ShorteningDelayed()
    {
        const string testLink = "https://app.wowssb.com/charts";
        var options = new LinkShorteningOptions
        {
            ApiKey = "1234",
            ApiUrl = "https://www.example.com/",
            RequestTimeout = 2000,
        };
        var mockHttp = new MockHttpMessageHandler();
        var request = mockHttp.When(HttpMethod.Post, options.ApiUrl + options.ApiKey).Respond("application/json", @"{ ""shortLink"": ""https://share.wowssb.com/1234"", ""previewLink"": ""https://share.wowssb.com/1234?d=1"" }");
        var logger = new Mock<ILogger<FirebaseLinkShortener>>();
        var linkShortener = new FirebaseLinkShortener(mockHttp.ToHttpClient(), Options.Create(options), logger.Object);

        var sw = Stopwatch.StartNew();
        for (var i = 0; i < 5; i++)
        {
            await linkShortener.CreateShortLink(testLink);
        }

        var result = await linkShortener.CreateShortLink(testLink);
        sw.Stop();

        sw.ElapsedMilliseconds.Should().BeGreaterThanOrEqualTo(1000);
        result.Link.Should().NotBeEmpty();
        result.Shortened.Should().BeTrue();
        mockHttp.GetMatchCount(request).Should().Be(6);
    }

    [Test]
    public void CreateShortLink_ApiKeySet_IsAvailable()
    {
        var options = new LinkShorteningOptions
        {
            ApiKey = "1234",
            ApiUrl = "https://www.example.com/",
        };
        var mockHttp = new MockHttpMessageHandler();
        var logger = new Mock<ILogger<FirebaseLinkShortener>>();
        var linkShortener = new FirebaseLinkShortener(mockHttp.ToHttpClient(), Options.Create(options), logger.Object);

        bool result = linkShortener.IsAvailable;

        result.Should().BeTrue();
    }

    [Test]
    public void CreateShortLink_ApiKeyNotSet_IsNotAvailable()
    {
        var options = new LinkShorteningOptions
        {
            ApiKey = string.Empty,
            ApiUrl = "https://www.example.com/",
        };
        var mockHttp = new MockHttpMessageHandler();
        var logger = new Mock<ILogger<FirebaseLinkShortener>>();
        var linkShortener = new FirebaseLinkShortener(mockHttp.ToHttpClient(), Options.Create(options), logger.Object);

        bool result = linkShortener.IsAvailable;

        result.Should().BeFalse();
    }
}
