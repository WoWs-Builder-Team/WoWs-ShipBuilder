using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.DataProvider.Updater;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.Core.Test.LocalDataUpdaterTests
{
    public partial class LocalDataUpdaterTest
    {
        [Test]
        public async Task CheckJsonFileVersion_NoExistingData_AllFilesMarkedForDownload()
        {
            // Arrange
            var testVersionInfo = CreateTestVersionInfo(1);
            awsClientMock.Setup(x => x.DownloadVersionInfo(ServerType.Live)).ReturnsAsync(testVersionInfo);
            mockFileSystem.AddDirectory(appDataHelper.Object.GetDataPath(ServerType.Live));

            // Act
            var result = await new LocalDataUpdater(mockFileSystem, awsClientMock.Object, appDataHelper.Object).CheckJsonFileVersions(ServerType.Live);

            // Assert
            result.AvailableFileUpdates.Should()
                .HaveCount(testVersionInfo.Categories.Select(category => category.Value.Count).Sum() + 1); // +1 due to VersionInfo being added
            result.ShouldImagesUpdate.Should().BeTrue();
            result.CanImagesDeltaUpdate.Should().BeFalse();
            result.ShouldLocalizationUpdate.Should().BeTrue();
        }

        [Test]
        public async Task CheckJsonFileVersion_ExistingDataOneVersionDiff_IncrementalUpdate()
        {
            // Arrange
            var localVersionInfo = CreateTestVersionInfo(1, "0.10.9#1");
            var testVersionInfo = new VersionInfo(
                new Dictionary<string, List<FileVersion>>
                {
                    { "Ability", new List<FileVersion> { new("Common.json", 2) } },
                    { "Ship", new List<FileVersion> { new("Japan.json", 1), new("Germany.json", 1) } },
                },
                2,
                "0.10.10#1",
                localVersionInfo.VersionName);
            awsClientMock.Setup(x => x.DownloadVersionInfo(ServerType.Live)).ReturnsAsync(testVersionInfo);
            mockFileSystem.AddDirectory(appDataHelper.Object.GetDataPath(ServerType.Live));
            appDataHelper.Setup(x => x.ReadLocalVersionInfo(ServerType.Live)).Returns(localVersionInfo);

            // Act
            var result = await new LocalDataUpdater(mockFileSystem, awsClientMock.Object, appDataHelper.Object).CheckJsonFileVersions(ServerType.Live);

            // Assert
            result.AvailableFileUpdates.Should().HaveCount(2);
            result.ShouldImagesUpdate.Should().BeTrue();
            result.CanImagesDeltaUpdate.Should().BeTrue();
            result.ShouldLocalizationUpdate.Should().BeTrue();
        }

        [Test]
        public async Task CheckJsonFileVersion_ExistingData_NoDownloads()
        {
            // Arrange
            var testVersionInfo = CreateTestVersionInfo(1);
            awsClientMock.Setup(x => x.DownloadVersionInfo(ServerType.Live)).ReturnsAsync(testVersionInfo);
            mockFileSystem.AddDirectory(appDataHelper.Object.GetDataPath(ServerType.Live));
            appDataHelper.Setup(x => x.ReadLocalVersionInfo(ServerType.Live)).Returns(testVersionInfo);

            // Act
            var result = await new LocalDataUpdater(mockFileSystem, awsClientMock.Object, appDataHelper.Object).CheckJsonFileVersions(ServerType.Live);

            // Assert
            result.AvailableFileUpdates.Should().BeEmpty();
            result.ShouldImagesUpdate.Should().BeFalse();
            result.CanImagesDeltaUpdate.Should().BeFalse();
            result.ShouldLocalizationUpdate.Should().BeFalse();
        }
    }
}
