using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using WoWsShipBuilder.DataStructures.Versioning;
using WoWsShipBuilder.Desktop.Features.Updater;
using WoWsShipBuilder.Features.Settings;
using WoWsShipBuilder.Infrastructure.GameData;

namespace WoWsShipBuilder.Desktop.Test.LocalDataUpdaterTests;

public partial class LocalDataUpdaterTest
{
    [Test]
    public async Task ShouldUpdaterRun_NoLastCheck_True()
    {
        // Arrange
        var appSettings = new AppSettings
        {
            LastDataUpdateCheck = null,
        };

        // Act
        bool result = await new LocalDataUpdater(this.mockFileSystem, this.awsClientMock.Object, this.appDataHelper.Object, appSettings, NullLogger<LocalDataUpdater>.Instance).ShouldUpdaterRun(ServerType.Live);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public async Task ShouldUpdaterRun_LastCheckTwoHoursAgo_False()
    {
        // Arrange
        var appSettings = new AppSettings
        {
            LastDataUpdateCheck = DateTime.Now.Subtract(TimeSpan.FromHours(2)),
        };
        this.appDataHelper.Setup(x => x.GetCurrentVersionInfo(ServerType.Live)).ReturnsAsync(this.CreateTestVersionInfo(1, GameVersion.Default));

        // Act
        bool result = await new LocalDataUpdater(this.mockFileSystem, this.awsClientMock.Object, this.appDataHelper.Object, appSettings, NullLogger<LocalDataUpdater>.Instance).ShouldUpdaterRun(ServerType.Live);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public async Task ShouldUpdaterRun_LocalVersionInfoNull_True()
    {
        // Arrange
        var appSettings = new AppSettings
        {
            LastDataUpdateCheck = DateTime.Now.Subtract(TimeSpan.FromHours(2)),
        };
        this.appDataHelper.Setup(x => x.GetCurrentVersionInfo(ServerType.Live)).ReturnsAsync((VersionInfo?)null);

        // Act
        bool result = await new LocalDataUpdater(this.mockFileSystem, this.awsClientMock.Object, this.appDataHelper.Object, appSettings, NullLogger<LocalDataUpdater>.Instance).ShouldUpdaterRun(ServerType.Live);

        // Assert
        result.Should().BeTrue();
    }
}
