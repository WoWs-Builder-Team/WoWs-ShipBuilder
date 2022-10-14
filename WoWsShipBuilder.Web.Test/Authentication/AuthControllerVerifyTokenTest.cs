using System;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using RichardSzalay.MockHttp;
using WoWsShipBuilder.Web.Authentication;
using WoWsShipBuilder.Web.Data;
using WoWsShipBuilder.Web.Services;

namespace WoWsShipBuilder.Web.Test.Authentication;

[TestFixture]
public class AuthControllerVerifyTokenTest
{
    [Test]
    public async Task VerifyToken_ValidResponse_Success()
    {
        const string accountId = "537376379";
        const string token = "abcd1234";
        var mockHttp = new MockHttpMessageHandler();
        var options = new AdminOptions { WgApiKey = "1234" };
        var response = new WgResponse
        {
            Status = "ok", Data = new()
            {
                [accountId] = new()
                {
                    Private = new()
                    {
                        ["gold"] = 1234,
                    },
                },
            },
        };
        var request = mockHttp
            .When($"https://api.worldofwarships.eu/wows/account/info/?application_id={options.WgApiKey}&account_id={accountId}&access_token={token}&fields=private")
            .Respond("application/json", JsonSerializer.Serialize(response));
        var controller = new AuthController(mockHttp.ToHttpClient(), Options.Create(options), Mock.Of<IMetricsService>(), Mock.Of<ILogger<AuthController>>());

        bool result = await controller.VerifyToken(accountId, token);

        result.Should().BeTrue();
        mockHttp.GetMatchCount(request).Should().Be(1);
    }

    [Test]
    public async Task VerifyToken_ErrorResponse_Fail()
    {
        const string accountId = "537376379";
        const string token = "abcd1234";
        var mockHttp = new MockHttpMessageHandler();
        var options = new AdminOptions { WgApiKey = "1234" };
        var response = new WgResponse { Status = "error" };
        var request = mockHttp
            .When($"https://api.worldofwarships.eu/wows/account/info/?application_id={options.WgApiKey}&account_id={accountId}&access_token={token}&fields=private")
            .Respond("application/json", JsonSerializer.Serialize(response));
        var controller = new AuthController(mockHttp.ToHttpClient(), Options.Create(options), Mock.Of<IMetricsService>(), Mock.Of<ILogger<AuthController>>());

        bool result = await controller.VerifyToken(accountId, token);

        result.Should().BeFalse();
        mockHttp.GetMatchCount(request).Should().Be(1);
    }

    [Test]
    public async Task VerifyToken_EuAccount_CorrectApiCalled()
    {
        const string accountId = "537376379";
        const string token = "abcd1234";
        var mockHttp = new MockHttpMessageHandler();
        var options = new AdminOptions { WgApiKey = "1234" };
        var response = new WgResponse { Status = "error" };
        var request = mockHttp
            .When("https://api.worldofwarships.eu/*")
            .Respond("application/json", JsonSerializer.Serialize(response));
        var controller = new AuthController(mockHttp.ToHttpClient(), Options.Create(options), Mock.Of<IMetricsService>(), Mock.Of<ILogger<AuthController>>());

        bool result = await controller.VerifyToken(accountId, token);

        result.Should().BeFalse();
        mockHttp.GetMatchCount(request).Should().Be(1);
    }

    [Test]
    public async Task VerifyToken_NaAccount_CorrectApiCalled()
    {
        const string accountId = "1537376379";
        const string token = "abcd1234";
        var mockHttp = new MockHttpMessageHandler();
        var options = new AdminOptions { WgApiKey = "1234" };
        var response = new WgResponse { Status = "error" };
        var request = mockHttp
            .When("https://api.worldofwarships.com/*")
            .Respond("application/json", JsonSerializer.Serialize(response));
        var controller = new AuthController(mockHttp.ToHttpClient(), Options.Create(options), Mock.Of<IMetricsService>(), Mock.Of<ILogger<AuthController>>());

        bool result = await controller.VerifyToken(accountId, token);

        result.Should().BeFalse();
        mockHttp.GetMatchCount(request).Should().Be(1);
    }

    [Test]
    public async Task VerifyToken_SeaAccount_CorrectApiCalled()
    {
        const string accountId = "2537376379";
        const string token = "abcd1234";
        var mockHttp = new MockHttpMessageHandler();
        var options = new AdminOptions { WgApiKey = "1234" };
        var response = new WgResponse { Status = "error" };
        var request = mockHttp
            .When("https://api.worldofwarships.asia/*")
            .Respond("application/json", JsonSerializer.Serialize(response));
        var controller = new AuthController(mockHttp.ToHttpClient(), Options.Create(options), Mock.Of<IMetricsService>(), Mock.Of<ILogger<AuthController>>());

        bool result = await controller.VerifyToken(accountId, token);

        result.Should().BeFalse();
        mockHttp.GetMatchCount(request).Should().Be(1);
    }

    [Test]
    public async Task VerifyToken_InvalidAccountId_Exception()
    {
        const string accountId = "123456789";
        const string token = "abcd1234";
        var mockHttp = new MockHttpMessageHandler();
        var options = new AdminOptions { WgApiKey = "1234" };
        var response = new WgResponse { Status = "error" };
        var request = mockHttp
            .When("https://api.worldofwarships.asia/*")
            .Respond("application/json", JsonSerializer.Serialize(response));
        var controller = new AuthController(mockHttp.ToHttpClient(), Options.Create(options), Mock.Of<IMetricsService>(), Mock.Of<ILogger<AuthController>>());

        Func<Task<bool>> f = async () => await controller.VerifyToken(accountId, token);

        await f.Should().ThrowAsync<InvalidOperationException>();
        mockHttp.GetMatchCount(request).Should().Be(0);
    }
}
