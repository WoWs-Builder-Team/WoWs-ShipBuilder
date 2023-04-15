using System;
using System.IO.Abstractions.TestingHelpers;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.HttpClients;
using WoWsShipBuilder.Core.Services;

namespace WoWsShipBuilder.Core.Test.HttpTest
{
    [TestFixture]
    public class AwsClientTest
    {
        private Mock<HttpMessageHandler> messageHandlerMock = default!;

        private MockFileSystem mockFileSystem = default!;

        private DesktopAppDataService appDataHelper = default!;

        private IDataService mockDataService = default!;

        [SetUp]
        public void Setup()
        {
            messageHandlerMock = new Mock<HttpMessageHandler>();
            mockFileSystem = new MockFileSystem();
            mockDataService = new DesktopDataService(mockFileSystem);
            appDataHelper = new DesktopAppDataService(mockFileSystem, mockDataService, new());
            messageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync((HttpRequestMessage _, CancellationToken _) => new HttpResponseMessage(HttpStatusCode.OK));
        }

        [Test]
        public async Task DownloadFile_LocalDirectoryDoesNotExist()
        {
            var client = new AwsClient(mockDataService, appDataHelper, NullLogger<AwsClient>.Instance, messageHandlerMock.Object);
            var filePath = @"json/live/VersionInfo.json";
            var requestUri = new Uri("https://cloudfront/api/live/VersionInfo.json");
            mockFileSystem.FileExists(filePath).Should().BeFalse();

            await client.DownloadFileAsync(requestUri, filePath);

            mockFileSystem.FileExists(filePath).Should().BeTrue();
            messageHandlerMock.Protected().Verify("SendAsync", Times.Exactly(1), ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == requestUri), ItExpr.IsAny<CancellationToken>());
        }

        [Test]
        public async Task DownloadFile_LocalDirectoryDoesNotExist_OneRetry()
        {
            messageHandlerMock.Protected()
                .SetupSequence<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            var client = new AwsClient(mockDataService, appDataHelper, NullLogger<AwsClient>.Instance, messageHandlerMock.Object);
            var filePath = @"json/live/VersionInfo.json";
            var requestUri = new Uri("https://cloudfront/api/live/VersionInfo.json");
            mockFileSystem.FileExists(filePath).Should().BeFalse();

            await client.DownloadFileAsync(requestUri, filePath);

            mockFileSystem.FileExists(filePath).Should().BeTrue();
            messageHandlerMock.Protected().Verify("SendAsync", Times.Exactly(2), ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == requestUri), ItExpr.IsAny<CancellationToken>());
        }
    }
}
