using System;
using FluentAssertions;
using NUnit.Framework;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.DataProvider.Updater;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Core.Test.LocalDataUpdaterTests
{
    public partial class LocalDataUpdaterTest
    {
        [Test]
        public void ShouldUpdaterRun_NoLastCheck_True()
        {
            // Arrange
            AppData.Settings.LastDataUpdateCheck = null;

            // Act
            bool result = new LocalDataUpdater(mockFileSystem, awsClientMock.Object, appDataHelper.Object).ShouldUpdaterRun(ServerType.Live);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public void ShouldUpdaterRun_LastCheckTwoHoursAgo_False()
        {
            // Arrange
            AppData.Settings.LastDataUpdateCheck = DateTime.Now.Subtract(TimeSpan.FromHours(2));
            appDataHelper.Setup(x => x.ReadLocalVersionInfo(ServerType.Live)).Returns(CreateTestVersionInfo(1));

            // Act
            bool result = new LocalDataUpdater(mockFileSystem, awsClientMock.Object, appDataHelper.Object).ShouldUpdaterRun(ServerType.Live);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void ShouldUpdaterRun_LocalVersionInfoNull_True()
        {
            // Arrange
            AppData.Settings.LastDataUpdateCheck = DateTime.Now.Subtract(TimeSpan.FromHours(2));
            appDataHelper.Setup(x => x.ReadLocalVersionInfo(ServerType.Live)).Returns((VersionInfo?)null);

            // Act
            bool result = new LocalDataUpdater(mockFileSystem, awsClientMock.Object, appDataHelper.Object).ShouldUpdaterRun(ServerType.Live);

            // Assert
            result.Should().BeTrue();
        }
    }
}
