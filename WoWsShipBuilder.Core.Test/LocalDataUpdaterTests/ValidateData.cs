using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.DataProvider.Updater;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Core.Test.LocalDataUpdaterTests
{
    public partial class LocalDataUpdaterTest
    {
        [Test]
        public async Task ValidateData_NoLocalVersionInfo_False()
        {
            // Arrange
            var updater = new LocalDataUpdater(mockFileSystem, awsClientMock.Object, appDataHelper.Object, new());

            // Act
            var result = await updater.ValidateData(ServerType.Live, @"c:\json\live");

            // Assert
            result.ValidationStatus.Should().BeFalse();
        }

        [Test]
        public async Task ValidateData_LocalVersionInfoNoFiles_False()
        {
            // Arrange
            var versionInfo = CreateTestVersionInfo(1);
            appDataHelper.Setup(x => x.GetLocalVersionInfo(ServerType.Live)).ReturnsAsync(versionInfo);
            mockFileSystem.AddDirectory(@"c:\json\live\Ability");
            var updater = new LocalDataUpdater(mockFileSystem, awsClientMock.Object, appDataHelper.Object, new());

            // Act
            var result = await updater.ValidateData(ServerType.Live, @"c:\json\live");

            // Assert
            result.ValidationStatus.Should().BeFalse();
        }

        [Test]
        public async Task ValidateData_LocalVersionInfoAllFiles_True()
        {
            // Arrange
            var versionCode = 1;
            var checksum = CreateHashForContent("test");
            var versionInfo = new VersionInfo(
                new()
                {
                    { "Ability", new() { new("Common.json", versionCode, checksum) } },
                    { "Ship", new() { new("Japan.json", versionCode, checksum), new("Germany.json", versionCode, checksum) } },
                },
                versionCode);
            appDataHelper.Setup(x => x.GetLocalVersionInfo(ServerType.Live)).ReturnsAsync(versionInfo);
            mockFileSystem.AddFile(@"c:\json\live\Ability\Common.json", new("test"));
            mockFileSystem.AddFile(@"c:\json\live\Ship\Japan.json", new("test"));
            mockFileSystem.AddFile(@"c:\json\live\Ship\Germany.json", new("test"));
            var updater = new LocalDataUpdater(mockFileSystem, awsClientMock.Object, appDataHelper.Object, new());

            // Act
            var result = await updater.ValidateData(ServerType.Live, @"c:\json\live");

            // Assert
            result.ValidationStatus.Should().BeTrue();
        }

        [Test]
        public async Task ValidateData_LocalVersionInfoOneFileMissing_False()
        {
            // Arrange
            var versionInfo = CreateTestVersionInfo(1);
            appDataHelper.Setup(x => x.GetLocalVersionInfo(ServerType.Live)).ReturnsAsync(versionInfo);
            mockFileSystem.AddFile(@"c:\json\live\Ability\Common.json", new MockFileData("test"));
            mockFileSystem.AddFile(@"c:\json\live\Ship\Japan.json", new MockFileData("test"));
            var updater = new LocalDataUpdater(mockFileSystem, awsClientMock.Object, appDataHelper.Object, new());

            // Act
            var result = await updater.ValidateData(ServerType.Live, @"c:\json\live");

            // Assert
            result.ValidationStatus.Should().BeFalse();
        }

        private static string CreateHashForContent(string content)
        {
            using var memoryStream = new MemoryStream();
            using var writer = new StreamWriter(memoryStream);
            writer.Write(content);
            writer.Flush();
            memoryStream.Seek(0, SeekOrigin.Begin);
            return FileVersion.ComputeChecksum(memoryStream);
        }
    }
}
