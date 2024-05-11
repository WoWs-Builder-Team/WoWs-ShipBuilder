using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.DataStructures.Versioning;
using WoWsShipBuilder.Infrastructure.ApplicationData;
using WoWsShipBuilder.Infrastructure.GameData;
using WoWsShipBuilder.Web.Infrastructure;

namespace WoWsShipBuilder.Web.Test;

[TestFixture]
public class ServerAwsClientTest
{
    private Mock<HttpMessageHandler> messageHandlerMock = default!;

    [SetUp]
    public void Setup()
    {
        this.messageHandlerMock = new();
        AppData.ShipDictionary.Clear();
    }

    [Test]
    public async Task DownloadFiles()
    {
        var testVersionInfo = new VersionInfo(new Dictionary<string, ImmutableList<FileVersion>> { { "Ship", ImmutableList.Create(new FileVersion("Germany.json", 1)) } }.ToImmutableDictionary(), 0, GameVersion.Default);
        const string testShipKey = "PGSA001";
        var shipDictionary = new Dictionary<string, Ship> { { testShipKey, new Ship { Index = testShipKey, Id = 1234 } } };

        this.messageHandlerMock.Protected().Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri!.AbsoluteUri.EndsWith("VersionInfo.json")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { Content = new StringContent(JsonSerializer.Serialize(testVersionInfo, AppConstants.JsonSerializerOptions)) });

        var shipRequestExpression = ItExpr.Is<HttpRequestMessage>(message => message.RequestUri!.AbsolutePath.Equals("/api/live/Ship/Germany.json"));
        this.messageHandlerMock.Protected().Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                shipRequestExpression,
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { Content = new StringContent(JsonSerializer.Serialize(shipDictionary, AppConstants.JsonSerializerOptions)) });

        var cdnOptions = new CdnOptions { Host = "https://example.com" };
        IOptions<CdnOptions> options = Options.Create(cdnOptions);
        var client = new ServerAwsClient(new(this.messageHandlerMock.Object), options, NullLogger<ServerAwsClient>.Instance);

        var versionInfo = await client.DownloadVersionInfo(ServerType.Live);
        var files = versionInfo.Categories.SelectMany(category => category.Value.Select(file => (category.Key, file.FileName))).ToList();
        await client.DownloadFiles(ServerType.Live, files);

        AppData.ShipDictionary.Should().HaveCount(1);
        AppData.ShipDictionary.Should().ContainKey(testShipKey);
        this.messageHandlerMock.Protected().Verify("SendAsync", Times.Exactly(1), shipRequestExpression, ItExpr.IsAny<CancellationToken>());
        this.messageHandlerMock.Protected().Verify("SendAsync", Times.Exactly(2), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
    }
}
