using System.IO.Abstractions.TestingHelpers;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.DataProvider.Updater;

namespace WoWsShipBuilder.Core.Test.LocalDataUpdaterTests
{
    public partial class LocalDataUpdaterTest
    {
        [Test]
        public async Task ValidateData_NoLocalVersionInfo_False()
        {
            // Arrange
            var updater = new LocalDataUpdater(mockFileSystem, awsClientMock.Object, appDataHelper.Object);

            // Act
            bool result = await updater.ValidateData(ServerType.Live, @"c:\json\live");

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task ValidateData_LocalVersionInfoNoFiles_False()
        {
            // Arrange
            var versionInfo = CreateTestVersionInfo(1);
            appDataHelper.Setup(x => x.ReadLocalVersionInfo(ServerType.Live)).ReturnsAsync(versionInfo);
            mockFileSystem.AddDirectory(@"c:\json\live\Ability");
            var updater = new LocalDataUpdater(mockFileSystem, awsClientMock.Object, appDataHelper.Object);

            // Act
            bool result = await updater.ValidateData(ServerType.Live, @"c:\json\live");

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task ValidateData_LocalVersionInfoAllFiles_True()
        {
            // Arrange
            var versionInfo = CreateTestVersionInfo(1);
            appDataHelper.Setup(x => x.ReadLocalVersionInfo(ServerType.Live)).ReturnsAsync(versionInfo);
            mockFileSystem.AddFile(@"c:\json\live\Ability\Common.json", new MockFileData("test"));
            mockFileSystem.AddFile(@"c:\json\live\Ship\Japan.json", new MockFileData("test"));
            mockFileSystem.AddFile(@"c:\json\live\Ship\Germany.json", new MockFileData("test"));
            var updater = new LocalDataUpdater(mockFileSystem, awsClientMock.Object, appDataHelper.Object);

            // Act
            bool result = await updater.ValidateData(ServerType.Live, @"c:\json\live");

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public async Task ValidateData_LocalVersionInfoOneFileMissing_False()
        {
            // Arrange
            var versionInfo = CreateTestVersionInfo(1);
            appDataHelper.Setup(x => x.ReadLocalVersionInfo(ServerType.Live)).ReturnsAsync(versionInfo);
            mockFileSystem.AddFile(@"c:\json\live\Ability\Common.json", new MockFileData("test"));
            mockFileSystem.AddFile(@"c:\json\live\Ship\Japan.json", new MockFileData("test"));
            var updater = new LocalDataUpdater(mockFileSystem, awsClientMock.Object, appDataHelper.Object);

            // Act
            bool result = await updater.ValidateData(ServerType.Live, @"c:\json\live");

            // Assert
            result.Should().BeFalse();
        }
    }
}
