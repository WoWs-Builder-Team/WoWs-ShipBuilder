using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WoWsShipBuilder.DataStructures.Versioning;
using WoWsShipBuilder.Features.Settings;
using WoWsShipBuilder.Infrastructure.ApplicationData;
using WoWsShipBuilder.Infrastructure.GameData;
using WoWsShipBuilder.Infrastructure.Utility;

namespace WoWsShipBuilder.Desktop.Infrastructure.Data;

public class DesktopAppDataService : IAppDataService
{
    private readonly AppSettings appSettings;

    private readonly IDataService dataService;

    private readonly IFileSystem fileSystem;

    public DesktopAppDataService(IFileSystem fileSystem, IDataService dataService, AppSettings appSettings)
    {
        this.fileSystem = fileSystem;
        this.dataService = dataService;
        this.appSettings = appSettings;
        this.DefaultAppDataDirectory = dataService.CombinePaths(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppConstants.ShipBuilderName);
    }

    public string DefaultAppDataDirectory { get; }

    public string AppDataDirectory
    {
        get
        {
            if (AppData.IsInitialized)
            {
                return this.appSettings.CustomDataPath ?? this.DefaultAppDataDirectory;
            }

            return this.DefaultAppDataDirectory;
        }
    }

    public string AppDataImageDirectory => this.dataService.CombinePaths(this.AppDataDirectory, "Images");

    public string GetDataPath(ServerType serverType)
    {
        string serverName = serverType.StringName();
        return this.dataService.CombinePaths(this.AppDataDirectory, "json", serverName);
    }

    /// <summary>
    /// Gets the localization directory for the selected server type.
    /// </summary>
    /// <param name="serverType">The selected server type.</param>
    /// <returns>The directory path of the current localization directory.</returns>
    public string GetLocalizationPath(ServerType serverType) => this.dataService.CombinePaths(this.GetDataPath(serverType), "Localization");

    /// <summary>
    /// Find the list of currently installed localizations.
    /// </summary>
    /// <param name="serverType">The selected server type.</param>
    /// <param name="includeFileType">Specifies whether the list of installed locales should contain the file extensions for each file.</param>
    /// <returns>A possibly empty list of installed locales.</returns>
    public async Task<List<string>> GetInstalledLocales(ServerType serverType, bool includeFileType = true)
    {
        // TODO: return Task.FromResult
        await Task.CompletedTask;
        this.fileSystem.Directory.CreateDirectory(this.GetLocalizationPath(serverType));
        var files = this.fileSystem.Directory.GetFiles(this.GetLocalizationPath(serverType)).Select(file => this.fileSystem.FileInfo.New(file));
        return includeFileType ? files.Select(file => file.Name).ToList() : files.Select(file => this.fileSystem.Path.GetFileNameWithoutExtension(file.Name)).ToList();
    }

    /// <summary>
    /// Read the current version info from the app data directory.
    /// </summary>
    /// <param name="serverType">The selected server type.</param>
    /// <returns>The local VersionInfo or null if none was found.</returns>
    public async Task<VersionInfo?> GetCurrentVersionInfo(ServerType serverType)
    {
        string filePath = this.dataService.CombinePaths(this.GetDataPath(serverType), "VersionInfo.json");
        return await this.DeserializeFile<VersionInfo>(filePath);
    }

    public async Task<Dictionary<string, string>?> ReadLocalizationData(ServerType serverType, string language)
    {
        string fileName = this.dataService.CombinePaths(this.GetDataPath(serverType), "Localization", $"{language}.json");
        return this.fileSystem.File.Exists(fileName) ? await this.DeserializeFile<Dictionary<string, string>>(fileName) : null;
    }

    public async Task LoadLocalFilesAsync(ServerType serverType)
    {
        var sw = Stopwatch.StartNew();
        Logging.Logger.LogDebug("Loading local files from disk");
        AppData.ResetCaches();
        var localVersionInfo = await this.GetCurrentVersionInfo(serverType) ?? throw new InvalidOperationException("No local data found");
        AppData.DataVersion = localVersionInfo.CurrentVersion.MainVersion.ToString(3) + "#" + localVersionInfo.CurrentVersion.DataIteration;

        var dataRootInfo = this.fileSystem.DirectoryInfo.New(this.GetDataPath(serverType));
        IDirectoryInfo[] categories = dataRootInfo.GetDirectories();

        // Multiple categories can be loaded simultaneously without concurrency issues because every cache is only used by one category.
        await Parallel.ForEachAsync(categories, async (category, ct) =>
        {
            if (category.Name.Contains("Localization", StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            foreach (var file in category.GetFiles())
            {
                string content = await this.fileSystem.File.ReadAllTextAsync(file.FullName, ct);
                await DataCacheHelper.AddToCache(file.Name, category.Name, content);
            }
        });

        Helpers.InitializeShipSelectorDataStructure();
        sw.Stop();
        Logging.Logger.LogDebug("Loaded local files in {}", sw.Elapsed);
    }

    public async Task<T?> DeserializeFile<T>(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("The provided file path must not be empty.");
        }

        if (!this.fileSystem.File.Exists(filePath))
        {
            Logging.Logger.LogWarning("Tried to load file {FilePath}, but it was not found", filePath);
            return default;
        }

        return await this.dataService.LoadAsync<T>(filePath);
    }
}
