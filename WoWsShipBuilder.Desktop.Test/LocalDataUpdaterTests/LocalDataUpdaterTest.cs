using System.IO.Abstractions.TestingHelpers;
using System.Reflection;
using Moq;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.DataStructures.Versioning;
using WoWsShipBuilder.Desktop.Infrastructure.AwsClient;
using WoWsShipBuilder.Infrastructure.Data;
using WoWsShipBuilder.Infrastructure.GameData;

namespace WoWsShipBuilder.Desktop.Test.LocalDataUpdaterTests;

[TestFixture]
public partial class LocalDataUpdaterTest
{
    private MockFileSystem mockFileSystem = default!;

    private Mock<IDesktopAwsClient> awsClientMock = default!;

    private Mock<IAppDataService> appDataHelper = default!;

    [SetUp]
    public void Setup()
    {
        mockFileSystem = new();
        awsClientMock = new();
        appDataHelper = new();
        appDataHelper.Setup(x => x.GetDataPath(ServerType.Live)).Returns(@"AppData/live");
    }

    private VersionInfo CreateTestVersionInfo(int versionCode, GameVersion gameVersion)
    {
        return new(
            new()
            {
                { "Ability", new() { new("Common.json", versionCode) } },
                { "Ship", new() { new("Japan.json", versionCode), new("Germany.json", versionCode) } },
            },
            versionCode,
            gameVersion)
        {
            DataStructuresVersion = Assembly.GetAssembly(typeof(Ship))!.GetName().Version!,
        };
    }
}
