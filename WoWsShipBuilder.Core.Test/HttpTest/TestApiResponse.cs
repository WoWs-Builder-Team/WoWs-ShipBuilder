using System;
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

        [Ignore("temporarily disabled.")]
        [Test]
        public async Task RequestImageData()
        {
            var apiKey = "";
            var result = await WowsClient.Instance.Get<ImageData>(apiKey, "encyclopedia/ships", new[] { "images" });
            Console.WriteLine(result);
        }

        // Ship ID Enterprise        Index
        // 3751786480                PASA518
        // Ship ID Iowa              Index
        // 4276041712                PASB018

        // Camo ID GamesCom 2018     Index
        // 4230934448                PCEC061
        // Camo ID Valentines        Index
        // 4272877488                PCEC021
        [Test]
        public void AwsClientShipTest()
        {
            Dictionary<long, string> requestShip = new();
            requestShip.Add(3751786480, "PASA518");
            requestShip.Add(4276041712, "PASB018");

            List<ImageSize> sizes = new();
            sizes.Add(ImageSize.Small);

            var wowsClientShip = WowsClient.Instance.DownloadShipsOrCamosImages(appId, requestShip, ImageType.Ship, sizes);
        }

        [Test]
        public void AwsClientCamoTest()
        {
            Dictionary<long, string> requestCamo = new();
            requestCamo.Add(4230934448, "PCEC061");
            requestCamo.Add(4272877488, "PCEC021");

            List<ImageSize> sizes = new();
            sizes.Add(ImageSize.Small);

            var wowsClientCamo = WowsClient.Instance.DownloadShipsOrCamosImages(appId, requestCamo, ImageType.Camo, sizes);
        }

        [Test]
        public void WowsClientTest()
        {
        }
    }
}
