using System.Collections.Immutable;
using System.IO.Abstractions.TestingHelpers;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using WoWsShipBuilder.DataStructures.Versioning;
using WoWsShipBuilder.Desktop.Features.Updater;
using WoWsShipBuilder.Infrastructure.GameData;

namespace WoWsShipBuilder.Desktop.Test.LocalDataUpdaterTests;

public partial class LocalDataUpdaterTest
{
    [Test]
    public async Task ValidateData_NoLocalVersionInfo_False()
    {
        // Arrange
        var updater = new LocalDataUpdater(this.mockFileSystem, this.awsClientMock.Object, this.appDataHelper.Object, new(), NullLogger<LocalDataUpdater>.Instance);

        // Act
        var result = await updater.ValidateData(ServerType.Live, @"json/live");

        // Assert
        result.ValidationStatus.Should().BeFalse();
    }

    [Test]
    public async Task ValidateData_LocalVersionInfoNoFiles_False()
    {
        // Arrange
        var versionInfo = this.CreateTestVersionInfo(1, GameVersion.Default);
        this.appDataHelper.Setup(x => x.GetCurrentVersionInfo(ServerType.Live)).ReturnsAsync(versionInfo);
        this.mockFileSystem.AddDirectory(@"json/live/Ability");
        var updater = new LocalDataUpdater(this.mockFileSystem, this.awsClientMock.Object, this.appDataHelper.Object, new(), NullLogger<LocalDataUpdater>.Instance);

        // Act
        var result = await updater.ValidateData(ServerType.Live, @"json/live");

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
            new Dictionary<string, ImmutableList<FileVersion>>
            {
                { "Ability", ImmutableList.Create(new FileVersion("Common.json", versionCode, checksum)) },
                { "Ship", ImmutableList.Create<FileVersion>(new("Japan.json", versionCode, checksum), new("Germany.json", versionCode, checksum)) },
            }.ToImmutableDictionary(),
            versionCode,
            GameVersion.Default);
        this.appDataHelper.Setup(x => x.GetCurrentVersionInfo(ServerType.Live)).ReturnsAsync(versionInfo);
        this.mockFileSystem.AddFile(@"json/live/Ability/Common.json", new("test"));
        this.mockFileSystem.AddFile(@"json/live/Ship/Japan.json", new("test"));
        this.mockFileSystem.AddFile(@"json/live/Ship/Germany.json", new("test"));
        var updater = new LocalDataUpdater(this.mockFileSystem, this.awsClientMock.Object, this.appDataHelper.Object, new(), NullLogger<LocalDataUpdater>.Instance);

        // Act
        var result = await updater.ValidateData(ServerType.Live, @"json/live");

        // Assert
        result.ValidationStatus.Should().BeTrue();
    }

    [Test]
    public async Task ValidateData_LocalVersionInfoOneFileMissing_False()
    {
        // Arrange
        var versionInfo = this.CreateTestVersionInfo(1, GameVersion.Default);
        this.appDataHelper.Setup(x => x.GetCurrentVersionInfo(ServerType.Live)).ReturnsAsync(versionInfo);
        this.mockFileSystem.AddFile(@"json/live/Ability/Common.json", new MockFileData("test"));
        this.mockFileSystem.AddFile(@"json/live/Ship/Japan.json", new MockFileData("test"));
        var updater = new LocalDataUpdater(this.mockFileSystem, this.awsClientMock.Object, this.appDataHelper.Object, new(), NullLogger<LocalDataUpdater>.Instance);

        // Act
        var result = await updater.ValidateData(ServerType.Live, @"json/live");

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
