using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.DataProvider.Updater;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.DataStructures.Versioning;

namespace WoWsShipBuilder.Core.Test.LocalDataUpdaterTests
{
    public partial class LocalDataUpdaterTest
    {
        [Test]
        public async Task CheckJsonFileVersion_NoExistingData_AllFilesMarkedForDownload()
        {
            // Arrange
            var testVersionInfo = CreateTestVersionInfo(1, new(Version.Parse("0.11.0"), GameVersionType.Live, 1));
            awsClientMock.Setup(x => x.DownloadVersionInfo(ServerType.Live)).ReturnsAsync(testVersionInfo);
            mockFileSystem.AddDirectory(appDataHelper.Object.GetDataPath(ServerType.Live));

            // Act
            var result = await new LocalDataUpdater(mockFileSystem, awsClientMock.Object, appDataHelper.Object, new(), NullLogger<LocalDataUpdater>.Instance).CheckJsonFileVersions(ServerType.Live);

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
            var currentVersion = new GameVersion(new(0, 10, 10), GameVersionType.Live, 1);
            var previousVersion = new GameVersion(new(0, 10, 9), GameVersionType.Live, 1);
            var localVersionInfo = CreateTestVersionInfo(1, previousVersion);
            var testVersionInfo = new VersionInfo(
                new()
                {
                    { "Ability", new() { new("Common.json", 2) } },
                    { "Ship", new() { new("Japan.json", 1), new("Germany.json", 1) } },
                },
                2,
                currentVersion,
                localVersionInfo.CurrentVersion);
            awsClientMock.Setup(x => x.DownloadVersionInfo(ServerType.Live)).ReturnsAsync(testVersionInfo);
            mockFileSystem.AddDirectory(appDataHelper.Object.GetDataPath(ServerType.Live));
            appDataHelper.Setup(x => x.GetCurrentVersionInfo(ServerType.Live)).ReturnsAsync(localVersionInfo);

            // Act
            var result = await new LocalDataUpdater(mockFileSystem, awsClientMock.Object, appDataHelper.Object, new(), NullLogger<LocalDataUpdater>.Instance).CheckJsonFileVersions(ServerType.Live);

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
            var testVersionInfo = CreateTestVersionInfo(1, new(Version.Parse("0.11.0"), GameVersionType.Live, 1));
            awsClientMock.Setup(x => x.DownloadVersionInfo(ServerType.Live)).ReturnsAsync(testVersionInfo);
            mockFileSystem.AddDirectory(appDataHelper.Object.GetDataPath(ServerType.Live));
            appDataHelper.Setup(x => x.GetCurrentVersionInfo(ServerType.Live)).ReturnsAsync(testVersionInfo);

            // Act
            var result = await new LocalDataUpdater(mockFileSystem, awsClientMock.Object, appDataHelper.Object, new(), NullLogger<LocalDataUpdater>.Instance).CheckJsonFileVersions(ServerType.Live);

            // Assert
            result.AvailableFileUpdates.Should().BeEmpty();
            result.ShouldImagesUpdate.Should().BeFalse();
            result.CanImagesDeltaUpdate.Should().BeFalse();
            result.ShouldLocalizationUpdate.Should().BeFalse();
        }

        [Test]
        public async Task CheckJsonFileVersions_OneVersionDiffNewVersionNotSupported()
        {
            // Arrange
            var supportedDataVersion = Assembly.GetAssembly(typeof(Ship))!.GetName().Version!;
            var currentVersion = new GameVersion(new(0, 10, 10), GameVersionType.Live, 1);
            var previousVersion = new GameVersion(new(0, 10, 9), GameVersionType.Live, 1);
            var previousVersionCode = 1;
            var localVersionInfo = new VersionInfo(
                new()
                {
                    { "Ability", new() { new("Common.json", previousVersionCode) } },
                    { "Ship", new() { new("Japan.json", previousVersionCode), new("Germany.json", previousVersionCode) } },
                },
                previousVersionCode,
                previousVersion);
            var testVersionInfo = new VersionInfo(
                new()
                {
                    { "Ability", new() { new("Common.json", 2) } },
                    { "Ship", new() { new("Japan.json", 1), new("Germany.json", 1) } },
                },
                2,
                currentVersion,
                localVersionInfo.CurrentVersion)
            {
                DataStructuresVersion = new(supportedDataVersion.Major, supportedDataVersion.Minor + 1, supportedDataVersion.Build),
            };
            awsClientMock.Setup(x => x.DownloadVersionInfo(ServerType.Live)).ReturnsAsync(testVersionInfo);
            mockFileSystem.AddDirectory(appDataHelper.Object.GetDataPath(ServerType.Live));
            appDataHelper.Setup(x => x.GetCurrentVersionInfo(ServerType.Live)).ReturnsAsync(localVersionInfo);

            // Act
            var result = await new LocalDataUpdater(mockFileSystem, awsClientMock.Object, appDataHelper.Object, new(), NullLogger<LocalDataUpdater>.Instance).CheckJsonFileVersions(ServerType.Live);

            // Assert
            result.AvailableFileUpdates.Should().BeEmpty();
            result.ShouldLocalizationUpdate.Should().BeFalse();
        }
    }
}
