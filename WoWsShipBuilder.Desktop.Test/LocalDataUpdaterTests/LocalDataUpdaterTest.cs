using System.Collections.Immutable;
using System.IO.Abstractions.TestingHelpers;
using System.Reflection;
using Moq;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.DataStructures.Versioning;
using WoWsShipBuilder.Desktop.Infrastructure.AwsClient;
using WoWsShipBuilder.Infrastructure.ApplicationData;
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
        this.mockFileSystem = new();
        this.awsClientMock = new();
        this.appDataHelper = new();
        this.appDataHelper.Setup(x => x.GetDataPath(ServerType.Live)).Returns(@"AppData/live");
    }

    private VersionInfo CreateTestVersionInfo(int versionCode, GameVersion gameVersion)
    {
        return new(
            new Dictionary<string, ImmutableList<FileVersion>>
            {
                { "Ability", ImmutableList.Create(new FileVersion("Common.json", versionCode)) },
                { "Ship", ImmutableList.Create<FileVersion>(new("Japan.json", versionCode), new("Germany.json", versionCode)) },
            }.ToImmutableDictionary(),
            versionCode,
            gameVersion)
        {
            DataStructuresVersion = Assembly.GetAssembly(typeof(Ship))!.GetName().Version!,
        };
    }
}
