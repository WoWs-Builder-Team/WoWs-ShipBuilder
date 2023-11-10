using System.IO.Abstractions.TestingHelpers;
using System.Text.Json;
using FluentAssertions;
using WoWsShipBuilder.Desktop.Infrastructure.Data;
using WoWsShipBuilder.Features.Settings;
using WoWsShipBuilder.Infrastructure.ApplicationData;

namespace WoWsShipBuilder.Desktop.Test;

[TestFixture]
public class DesktopDataServiceTest
{
    private MockFileSystem mockFileSystem = null!;

    private IDataService dataService = null!;

    [SetUp]
    public void Setup()
    {
        this.mockFileSystem = new();
        this.dataService = new DesktopDataService(this.mockFileSystem);
    }

    [Test]
    public void Store_DirectoryNotExisting_DirectoryCreated()
    {
        const string settingsDirectory = @"app";
        const string settingsPath = settingsDirectory + @"/settings.json";
        const string customDataPath = "1234";
        var testSettings = new AppSettings { AutoUpdateEnabled = false, CustomDataPath = customDataPath };

        this.dataService.Store(testSettings, settingsPath);

        this.mockFileSystem.FileExists(settingsPath).Should().BeTrue();
        this.mockFileSystem.Directory.Exists(settingsDirectory).Should().BeTrue();

        var storedFile = this.mockFileSystem.File.ReadAllText(settingsPath);
        var storedSettings = JsonSerializer.Deserialize<AppSettings>(storedFile, AppConstants.JsonSerializerOptions);
        storedSettings.Should().BeEquivalentTo(testSettings);
    }

    [Test]
    public async Task Store_FileAlreadyExists_FileReplaced()
    {
        const string settingsDirectory = @"app";
        const string settingsPath = settingsDirectory + @"/settings.json";
        const string customDataPath = "1234";
        var testSettings = new AppSettings { AutoUpdateEnabled = false, CustomDataPath = customDataPath };
        this.mockFileSystem.AddFile(settingsPath, new(JsonSerializer.Serialize(testSettings, AppConstants.JsonSerializerOptions)));
        testSettings.AutoUpdateEnabled = true;

        await this.dataService.StoreAsync(testSettings, settingsPath);

        this.mockFileSystem.FileExists(settingsPath).Should().BeTrue();
        this.mockFileSystem.Directory.Exists(settingsDirectory).Should().BeTrue();

        var storedFile = await this.mockFileSystem.File.ReadAllTextAsync(settingsPath);
        var storedSettings = JsonSerializer.Deserialize<AppSettings>(storedFile, AppConstants.JsonSerializerOptions);
        storedSettings.Should().BeEquivalentTo(testSettings);
    }
}
