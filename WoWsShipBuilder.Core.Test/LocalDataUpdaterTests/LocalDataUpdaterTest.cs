using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using Moq;
using NUnit.Framework;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.HttpClients;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.Core.Test.LocalDataUpdaterTests
{
    [TestFixture]
    public partial class LocalDataUpdaterTest
    {
        private MockFileSystem mockFileSystem = default!;

        private Mock<IAwsClient> awsClientMock = default!;

        private Mock<AppDataHelper> appDataHelper = default!;

        [SetUp]
        public void Setup()
        {
            mockFileSystem = new MockFileSystem();
            awsClientMock = new Mock<IAwsClient>();
            appDataHelper = new Mock<AppDataHelper>(MockBehavior.Loose, mockFileSystem);
        }

        private VersionInfo CreateTestVersionInfo(int versionCode, string versionName = "")
        {
            return new VersionInfo(
                new Dictionary<string, List<FileVersion>>
                {
                    { "Ability", new List<FileVersion> { new("Common.json", versionCode) } },
                    { "Ship", new List<FileVersion> { new("Japan.json", versionCode), new("Germany.json", versionCode) } },
                },
                versionCode,
                versionName);
        }
    }
}
