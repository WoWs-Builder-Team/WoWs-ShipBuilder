using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using NUnit.Framework;
using WoWsShipBuilder.Common.Infrastructure;
using WoWsShipBuilder.Common.Infrastructure.Data;
using WoWsShipBuilder.Common.Infrastructure.GameData;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.DataStructures.Versioning;
using WoWsShipBuilder.Web.Data;
using WoWsShipBuilder.Web.Services;

namespace WoWsShipBuilder.Web.Test;

[TestFixture]
public class ServerAwsClientTest
{
    private Mock<HttpMessageHandler> messageHandlerMock = default!;

    [SetUp]
    public void Setup()
    {
        messageHandlerMock = new();
        AppData.ShipDictionary.Clear();
    }

    [Test]
    public async Task DownloadFiles()
    {
        var testVersionInfo = new VersionInfo(new() { { "Ship", new() { new("Germany.json", 1) } } }, 0, GameVersion.Default);
        const string testShipKey = "PGSA001";
        var shipDictionary = new Dictionary<string, Ship> { { testShipKey, new Ship { Index = testShipKey, Id = 1234 } } };

        messageHandlerMock.Protected().Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri!.AbsoluteUri.EndsWith("VersionInfo.json")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { Content = new StringContent(JsonConvert.SerializeObject(testVersionInfo)) });

        var shipRequestExpression = ItExpr.Is<HttpRequestMessage>(message => message.RequestUri!.AbsolutePath.Equals("/api/live/Ship/Germany.json"));
        messageHandlerMock.Protected().Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                shipRequestExpression,
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { Content = new StringContent(JsonConvert.SerializeObject(shipDictionary)) });

        var cdnOptions = new CdnOptions { Host = "https://example.com"};
        IOptions<CdnOptions>? options = Options.Create(cdnOptions);
        var client = new ServerAwsClient(new(messageHandlerMock.Object), options, NullLogger<ServerAwsClient>.Instance);

        var versionInfo = await client.DownloadVersionInfo(ServerType.Live);
        var files = versionInfo.Categories.SelectMany(category => category.Value.Select(file => (category.Key, file.FileName))).ToList();
        await client.DownloadFiles(ServerType.Live, files);

        AppData.ShipDictionary.Should().HaveCount(1);
        AppData.ShipDictionary.Should().ContainKey(testShipKey);
        messageHandlerMock.Protected().Verify("SendAsync", Times.Exactly(1), shipRequestExpression, ItExpr.IsAny<CancellationToken>());
        messageHandlerMock.Protected().Verify("SendAsync", Times.Exactly(2), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
    }
}
