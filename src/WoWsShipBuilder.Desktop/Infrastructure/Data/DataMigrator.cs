using System;
using System.IO.Abstractions;
using Microsoft.Extensions.Logging;
using WoWsShipBuilder.Infrastructure.ApplicationData;

namespace WoWsShipBuilder.Desktop.Infrastructure.Data;

internal sealed class DataMigrator
{
    private readonly IFileSystem fileSystem;

    private readonly IDataService dataService;

    private readonly IAppDataService appDataService;

    private readonly ILogger<DataMigrator> logger;

    public DataMigrator(IFileSystem fileSystem, IDataService dataService, IAppDataService appDataService, ILogger<DataMigrator> logger)
    {
        this.fileSystem = fileSystem;
        this.dataService = dataService;
        this.appDataService = appDataService;
        this.logger = logger;
    }

    public void MigrateData()
    {
        const string buildFileName = "builds.json";
        const string settingsFileName = "settings.json";

        var oldAppDataDirectory = this.dataService.CombinePaths(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppConstants.ShipBuilderName);
        var newAppDataDirectory = this.appDataService.DefaultAppDataDirectory;

        this.fileSystem.Directory.CreateDirectory(newAppDataDirectory);

        var oldBuildFilePath = this.dataService.CombinePaths(oldAppDataDirectory, buildFileName);
        var newBuildFilePath = this.dataService.CombinePaths(newAppDataDirectory, buildFileName);
        if (this.fileSystem.File.Exists(oldBuildFilePath) && !this.fileSystem.File.Exists(newBuildFilePath))
        {
            this.logger.LogInformation("Migrating builds.json file to new path");
            this.fileSystem.File.Copy(oldBuildFilePath, newBuildFilePath);
        }

        var oldSettingsFilePath = this.dataService.CombinePaths(oldAppDataDirectory, settingsFileName);
        var newSettingsFilePath = this.dataService.CombinePaths(newAppDataDirectory, settingsFileName);
        if (this.fileSystem.File.Exists(oldSettingsFilePath) && !this.fileSystem.File.Exists(newSettingsFilePath))
        {
            this.logger.LogInformation("Migrating settings.json file to new path");
            this.fileSystem.File.Copy(oldSettingsFilePath, newSettingsFilePath);
        }
    }
}
