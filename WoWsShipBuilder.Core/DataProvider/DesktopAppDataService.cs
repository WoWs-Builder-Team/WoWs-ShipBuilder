using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using WoWsShipBuilder.Core.Builds;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.Core.Settings;
using WoWsShipBuilder.DataStructures.Versioning;

namespace WoWsShipBuilder.Core.DataProvider;

public class DesktopAppDataService : IAppDataService, IUserDataService
{
    private readonly AppSettings appSettings;

    private readonly IDataService dataService;

    private readonly IFileSystem fileSystem;

    public DesktopAppDataService(IFileSystem fileSystem, IDataService dataService, AppSettings appSettings)
    {
        this.fileSystem = fileSystem;
        this.dataService = dataService;
        this.appSettings = appSettings;
        DefaultAppDataDirectory = dataService.CombinePaths(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppConstants.ShipBuilderName);
    }

    public string DefaultAppDataDirectory { get; }

    public string AppDataDirectory
    {
        get
        {
            if (AppData.IsInitialized)
            {
                return appSettings.CustomDataPath ?? DefaultAppDataDirectory;
            }

            return DefaultAppDataDirectory;
        }
    }

    public string AppDataImageDirectory => dataService.CombinePaths(AppDataDirectory, "Images");

    public string GetDataPath(ServerType serverType)
    {
        string serverName = serverType.StringName();
        return dataService.CombinePaths(AppDataDirectory, "json", serverName);
    }

    /// <summary>
    /// Gets the localization directory for the selected server type.
    /// </summary>
    /// <param name="serverType">The selected server type.</param>
    /// <returns>The directory path of the current localization directory.</returns>
    public string GetLocalizationPath(ServerType serverType) => dataService.CombinePaths(GetDataPath(serverType), "Localization");

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
        fileSystem.Directory.CreateDirectory(GetLocalizationPath(serverType));
        var files = fileSystem.Directory.GetFiles(GetLocalizationPath(serverType)).Select(file => fileSystem.FileInfo.New(file));
        return includeFileType ? files.Select(file => file.Name).ToList() : files.Select(file => fileSystem.Path.GetFileNameWithoutExtension(file.Name)).ToList();
    }

    /// <summary>
    /// Read the current version info from the app data directory.
    /// </summary>
    /// <param name="serverType">The selected server type.</param>
    /// <returns>The local VersionInfo or null if none was found.</returns>
    public async Task<VersionInfo?> GetCurrentVersionInfo(ServerType serverType)
    {
        string filePath = dataService.CombinePaths(GetDataPath(serverType), "VersionInfo.json");
        return await DeserializeFile<VersionInfo>(filePath);
    }

    public async Task<Dictionary<string, string>?> ReadLocalizationData(ServerType serverType, string language)
    {
        string fileName = dataService.CombinePaths(GetDataPath(serverType), "Localization", $"{language}.json");
        return fileSystem.File.Exists(fileName) ? await DeserializeFile<Dictionary<string, string>>(fileName) : null;
    }

    /// <summary>
    /// Save string compressed <see cref="Build"/> to the disk.
    /// </summary>
    public void SaveBuilds()
    {
        var path = dataService.CombinePaths(DefaultAppDataDirectory, "builds.json");
        var builds = AppData.Builds.Select(build => build.CreateStringFromBuild()).ToList();
        dataService.Store(builds, path);
    }

    public void LoadBuilds()
    {
        string path = dataService.CombinePaths(DefaultAppDataDirectory, "builds.json");
        if (fileSystem.File.Exists(path))
        {
            var rawBuildList = dataService.Load<List<string>>(path);
            AppData.Builds = rawBuildList?
                .Select(str => Build.CreateBuildFromString(str))
                .ToList() ?? new List<Build>();
        }
    }

    public async Task LoadLocalFilesAsync(ServerType serverType)
    {
        var sw = Stopwatch.StartNew();
        Logging.Logger.Debug("Loading local files from disk.");
        AppData.ResetCaches();
        var localVersionInfo = await GetCurrentVersionInfo(serverType) ?? throw new InvalidOperationException("No local data found");
        AppData.DataVersion = localVersionInfo.CurrentVersion.MainVersion.ToString(3) + "#" + localVersionInfo.CurrentVersion.DataIteration;

        var dataRootInfo = fileSystem.DirectoryInfo.New(GetDataPath(serverType));
        IDirectoryInfo[]? categories = dataRootInfo.GetDirectories();

        // Multiple categories can be loaded simultaneously without concurrency issues because every cache is only used by one category.
        await Parallel.ForEachAsync(categories, async (category, ct) =>
        {
            if (category.Name.Contains("Localization", StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            foreach (var file in category.GetFiles())
            {
                string content = await fileSystem.File.ReadAllTextAsync(file.FullName, ct);
                await DataCacheHelper.AddToCache(file.Name, category.Name, content);
            }
        });

        sw.Stop();
        Logging.Logger.Debug("Loaded local files in {}", sw.Elapsed);
    }

    public async Task<T?> DeserializeFile<T>(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("The provided file path must not be empty.");
        }

        if (!fileSystem.File.Exists(filePath))
        {
            Logging.Logger.Warn($"Tried to load file {filePath}, but it was not found.");
            return default;
        }

        return await dataService.LoadAsync<T>(filePath);
    }
}
