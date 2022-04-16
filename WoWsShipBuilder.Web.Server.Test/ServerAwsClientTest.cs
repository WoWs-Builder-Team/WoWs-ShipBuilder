using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Web.Server.Services;

namespace WoWsShipBuilder.Web.Server.Test;

[TestFixture]
public class ServerAwsClientTest
{
    [Test]
    public async Task DownloadFiles()
    {
        var client = new ServerAwsClient(new());
        AppData.ShipDictionary = new();

        var versionInfo = await client.DownloadVersionInfo(ServerType.Live);
        var files = versionInfo.Categories.SelectMany(category => category.Value.Select(file => (category.Key, file.FileName))).ToList();
        await client.DownloadFiles(ServerType.Live, files);

        AppData.ShipDictionary.Should().HaveCountGreaterThan(0);
    }
}
