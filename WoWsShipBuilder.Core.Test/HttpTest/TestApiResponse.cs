using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using WoWsShipBuilder.Core.HttpClients;
using WoWsShipBuilder.Core.HttpResponses;

namespace WoWsShipBuilder.Core.Test.HttpTest
{
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
            Dictionary<long, string> requestShip = new();
            requestShip.Add(3751786480, "PASA518");
            requestShip.Add(4276041712, "PASB018");

            List<ImageSize> sizes = new();
            sizes.Add(ImageSize.Small);

            await WowsClient.Instance.DownloadShipsOrCamosImages(appId, requestShip, ImageType.Ship, sizes);
        }

        [Test]
        public async Task WowsClientCamoTest()
        {
            Dictionary<long, string> requestCamo = new();
            requestCamo.Add(4230934448, "PCEC061_Camo_Gamescom_2018");
            requestCamo.Add(4272877488, "PCEC021_Valentine_Tile");

            List<ImageSize> sizes = new();
            sizes.Add(ImageSize.Small);

            await WowsClient.Instance.DownloadShipsOrCamosImages(appId, requestCamo, ImageType.Camo, sizes);
        }

        [Test]
        public async Task AwsClientTest()
        {
            await AwsClient.Instance.DownloadAllImages(ImageType.Ship);
            await AwsClient.Instance.DownloadAllImages(ImageType.Camo);
        }

        [Test]
        public async Task AwsClientSpecificShipTest()
        {
            List<string> indexesShip = new();
            indexesShip.Add("PASA518");
            indexesShip.Add("PASB018");

            await AwsClient.Instance.DownloadImages(indexesShip, ImageType.Ship);
        }

        [Test]
        public async Task AwsClientSpecificCamoTest()
        {
            List<string> indexesCamo = new();
            indexesCamo.Add("PCEC061_Camo_Gamescom_2018");
            indexesCamo.Add("PCEC021_Valentine_Tile");

            await AwsClient.Instance.DownloadImages(indexesCamo, ImageType.Camo);
        }
    }
}
