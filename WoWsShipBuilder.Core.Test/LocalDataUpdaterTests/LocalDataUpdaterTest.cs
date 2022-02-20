using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using Moq;
using NUnit.Framework;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.HttpClients;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Core.Test.LocalDataUpdaterTests
{
    [TestFixture]
    public partial class LocalDataUpdaterTest
    {
        private MockFileSystem mockFileSystem = default!;

        private Mock<IAwsClient> awsClientMock = default!;

        private Mock<IAppDataService> appDataHelper = default!;

        [SetUp]
        public void Setup()
        {
            mockFileSystem = new MockFileSystem();
            awsClientMock = new Mock<IAwsClient>();
            appDataHelper = new Mock<IAppDataService>();
            appDataHelper.Setup(x => x.GetDataPath(ServerType.Live)).Returns(@"C:\AppData\live");
        }

        private VersionInfo CreateTestVersionInfo(int versionCode, GameVersion? gameVersion = null)
        {
            return new VersionInfo(
                new Dictionary<string, List<FileVersion>>
                {
                    { "Ability", new List<FileVersion> { new("Common.json", versionCode) } },
                    { "Ship", new List<FileVersion> { new("Japan.json", versionCode), new("Germany.json", versionCode) } },
                },
                versionCode,
                gameVersion);
        }
    }
}
