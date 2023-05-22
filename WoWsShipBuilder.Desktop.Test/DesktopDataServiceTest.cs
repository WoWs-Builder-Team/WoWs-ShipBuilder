using System.IO.Abstractions.TestingHelpers;
using FluentAssertions;
using Newtonsoft.Json;
using WoWsShipBuilder.Desktop.Infrastructure.Data;
using WoWsShipBuilder.Features.Settings;

namespace WoWsShipBuilder.Desktop.Test;

[TestFixture]
public class DesktopDataServiceTest
{
    private MockFileSystem mockFileSystem = null!;

    private IDataService dataService = null!;

    [SetUp]
    public void Setup()
    {
        mockFileSystem = new();
        dataService = new DesktopDataService(mockFileSystem);
    }

    [Test]
    public void Store_DirectoryNotExisting_DirectoryCreated()
    {
        const string settingsDirectory = @"app";
        const string settingsPath = settingsDirectory + @"/settings.json";
        const string customDataPath = "1234";
        var testSettings = new AppSettings { AutoUpdateEnabled = false, CustomDataPath = customDataPath };

        dataService.Store(testSettings, settingsPath);

        mockFileSystem.FileExists(settingsPath).Should().BeTrue();
        mockFileSystem.Directory.Exists(settingsDirectory).Should().BeTrue();

        var storedFile = mockFileSystem.File.ReadAllText(settingsPath);
        var storedSettings = JsonConvert.DeserializeObject<AppSettings>(storedFile);
        storedSettings.Should().BeEquivalentTo(testSettings);
    }

    [Test]
    public async Task Store_FileAlreadyExists_FileReplaced()
    {
        const string settingsDirectory = @"app";
        const string settingsPath = settingsDirectory + @"/settings.json";
        const string customDataPath = "1234";
        var testSettings = new AppSettings { AutoUpdateEnabled = false, CustomDataPath = customDataPath };
        mockFileSystem.AddFile(settingsPath, new(JsonConvert.SerializeObject(testSettings)));
        testSettings.AutoUpdateEnabled = true;

        await dataService.StoreAsync(testSettings, settingsPath);

        mockFileSystem.FileExists(settingsPath).Should().BeTrue();
        mockFileSystem.Directory.Exists(settingsDirectory).Should().BeTrue();

        var storedFile = await mockFileSystem.File.ReadAllTextAsync(settingsPath);
        var storedSettings = JsonConvert.DeserializeObject<AppSettings>(storedFile);
        storedSettings.Should().BeEquivalentTo(testSettings);
    }
}
