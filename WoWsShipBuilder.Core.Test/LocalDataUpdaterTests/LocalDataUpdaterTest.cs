﻿using System.IO.Abstractions.TestingHelpers;
using System.Reflection;
using Moq;
using NUnit.Framework;
using WoWsShipBuilder.Common.Infrastructure;
using WoWsShipBuilder.Common.Infrastructure.Data;
using WoWsShipBuilder.Common.Infrastructure.GameData;
using WoWsShipBuilder.Common.Infrastructure.HttpClients;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.HttpClients;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.DataStructures.Versioning;

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
            mockFileSystem = new();
            awsClientMock = new();
            appDataHelper = new();
            appDataHelper.Setup(x => x.GetDataPath(ServerType.Live)).Returns(@"AppData/live");
        }

        private VersionInfo CreateTestVersionInfo(int versionCode, GameVersion gameVersion)
        {
            return new(
                new()
                {
                    { "Ability", new() { new("Common.json", versionCode) } },
                    { "Ship", new() { new("Japan.json", versionCode), new("Germany.json", versionCode) } },
                },
                versionCode,
                gameVersion)
            {
                DataStructuresVersion = Assembly.GetAssembly(typeof(Ship))!.GetName().Version!,
            };
        }
    }
}
