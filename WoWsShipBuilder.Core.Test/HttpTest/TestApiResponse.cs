using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.HttpClients;
using WoWsShipBuilder.Core.HttpResponses;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.Core.Test.HttpTest
{
    // TODO: fix unit tests with file system mocking.
    [Ignore("Disabled until all tests avoid doing actual http requests.")]
    [TestFixture]
    public class TestApiResponse
    {
        private readonly string appId = "6d563fd75651dbf9de009418d3ee7f56";

        // Ship ID Enterprise        Index
        // 3751786480                PASA518
        // Ship ID Iowa              Index
        // 4276041712                PASB018

        // Camo ID GamesCom 2018     Index
        // 4230934448                PCEC061
        // Camo ID Valentines        Index
        // 4272877488                PCEC021
        [Test]
        public async Task WowsClientShipTest()
        {
            var provider = CreateDefaultDataHelper().Object;
            var dataHelper = new AppDataHelper(provider);
            Dictionary<long, string> requestShip = new();
            requestShip.Add(3751786480, "PASA518");
            requestShip.Add(4276041712, "PASB018");

            List<ImageSize> sizes = new();
            sizes.Add(ImageSize.Small);

            await new WowsClient(provider, dataHelper).DownloadShipsOrCamosImages(appId, requestShip, ImageType.Ship, sizes);
        }

        [Test]
        public async Task WowsClientCamoTest()
        {
            var provider = CreateDefaultDataHelper().Object;
            var dataHelper = new AppDataHelper(provider);
            Dictionary<long, string> requestCamo = new();
            requestCamo.Add(4230934448, "PCEC061_Camo_Gamescom_2018");
            requestCamo.Add(4272877488, "PCEC021_Valentine_Tile");

            List<ImageSize> sizes = new();
            sizes.Add(ImageSize.Small);

            await new WowsClient(provider, dataHelper).DownloadShipsOrCamosImages(appId, requestCamo, ImageType.Camo, sizes);
        }

        [Test]
        public async Task AwsClientTest()
        {
            var fileSystem = new MockFileSystem();
            var clientMock = new Mock<AwsClient>(fileSystem) { CallBase = true };
            clientMock.Setup(x => x.DownloadFileAsync(It.IsAny<Uri>(), It.IsAny<string>())).Returns(Task.CompletedTask);

            AwsClient client = clientMock.Object;

            Func<Task> shipDownload = async () => await client.DownloadAllImages(ImageType.Ship);
            Func<Task> camoDownload = async () => await client.DownloadAllImages(ImageType.Camo);

            await shipDownload.Should().ThrowAsync<FileNotFoundException>();
            await camoDownload.Should().ThrowAsync<FileNotFoundException>();
        }

        [Test]
        public async Task AwsClientSpecificShipTest()
        {
            var provider = CreateDefaultDataHelper().Object;
            var dataHelper = new AppDataHelper(provider);
            var client = new AwsClient(provider, dataHelper);
            List<string> indexesShip = new();
            indexesShip.Add("PASA518");
            indexesShip.Add("PASB018");

            await client.DownloadImages(indexesShip, ImageType.Ship);
        }

        [Test]
        public async Task AwsClientSpecificCamoTest()
        {
            var provider = CreateDefaultDataHelper().Object;
            var dataHelper = new AppDataHelper(provider);
            var client = new AwsClient(provider, dataHelper);
            List<string> indexesCamo = new();
            indexesCamo.Add("PCEC061_Camo_Gamescom_2018");
            indexesCamo.Add("PCEC021_Valentine_Tile");

            await client.DownloadImages(indexesCamo, ImageType.Camo);
        }

        [Test]
        public async Task CheckFileVersion_NoVersionFileExists()
        {
            var providerMock = CreateDefaultDataHelper();
            ClearDirectory(AppDataHelper.Instance.GetDataPath(ServerType.Live));
            var clientMock = new Mock<AwsClient>(providerMock.Object) { CallBase = true };
            var versionDictionary = new Dictionary<string, List<FileVersion>>
            {
                { "Ability", new List<FileVersion> { new("Common", 1) } },
            };
            var versionInfo = new VersionInfo(versionDictionary, 1);
            clientMock.Setup(x => x.GetJsonAsync<VersionInfo>(It.IsAny<string>(), It.IsAny<JsonSerializer>()).Result).Returns(versionInfo);
            clientMock.Setup(x => x.DownloadFileAsync(It.IsAny<Uri>(), It.IsAny<string>())).Returns(Task.CompletedTask);

            await clientMock.Object.CheckFileVersion(ServerType.Live);

            clientMock.Verify(x => x.DownloadFileAsync(It.IsAny<Uri>(), It.IsRegex(".*Ability[\\\\/]Common.*")), Times.Once);
        }

        private Mock<IFileSystem> CreateDefaultDataHelper()
        {
            var provider = new Mock<IFileSystem>();
            return provider;
        }

        private void ClearDirectory(string path)
        {
            var directory = new DirectoryInfo(path);
            directory.GetFiles().ToList().ForEach(file => file.Delete());
            directory.GetDirectories().ToList().ForEach(dir => dir.Delete(true));
        }
    }
}
