using System;
using System.Threading.Tasks;
using NUnit.Framework;
using WoWsShipBuilder.Core.HttpClients;
using WoWsShipBuilder.Core.HttpResponses;

namespace WoWsShipBuilder.Core.Test.HttpTest
{
    public class TestApiResponse
    {
        [Ignore("temporarily disabled.")]
        [Test]
        public async Task RequestImageData()
        {
            var apiKey = "";
            var result = await WowsClient.Instance.Get<ImageData>(apiKey, "encyclopedia/ships", new[] { "images" });
            Console.WriteLine(result);
        }
    }
}
