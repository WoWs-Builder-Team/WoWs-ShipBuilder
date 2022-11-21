using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Splat;
using WoWsShipBuilder.Core.Builds;
using WoWsShipBuilder.Core.Localization;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.Core.Settings;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.DataStructures.Versioning;

namespace WoWsShipBuilder.Core.DataProvider;

public class DesktopAppDataService : IAppDataService, IUserDataService
{
#if DEBUG
    private const string ShipBuilderName = "WoWsShipBuilderDev";
#else
    private const string ShipBuilderName = "WoWsShipBuilder";
#endif

#pragma warning disable CS8603
    private static readonly Lazy<DesktopAppDataService> InstanceValue = new(() => Locator.Current.GetService<DesktopAppDataService>() ?? PreviewInstance);
#pragma warning restore CS8603

    private readonly AppSettings appSettings;

    private readonly IDataService dataService;

    private readonly IFileSystem fileSystem;

    public DesktopAppDataService(IFileSystem fileSystem, IDataService dataService, AppSettings appSettings)
    {
        this.fileSystem = fileSystem;
        this.dataService = dataService;
        this.appSettings = appSettings;
        DefaultAppDataDirectory = dataService.CombinePaths(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ShipBuilderName);
    }

    public static DesktopAppDataService Instance => InstanceValue.Value;

    public static DesktopAppDataService PreviewInstance { get; } = new(new FileSystem(), new DesktopDataService(new FileSystem()), new());

    public string BuildImageOutputDirectory => appSettings.CustomImagePath ?? dataService.CombinePaths(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), ShipBuilderName);

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
        var files = fileSystem.Directory.GetFiles(GetLocalizationPath(serverType)).Select(file => fileSystem.FileInfo.FromFileName(file));
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

    public async Task<List<ShipSummary>> GetShipSummaryList(ServerType serverType)
    {
        string fileName = dataService.CombinePaths(GetDataPath(serverType), "Summary", "Common.json");
        return await DeserializeFile<List<ShipSummary>>(fileName) ?? new List<ShipSummary>();
    }

    public async Task<Dictionary<string, string>?> ReadLocalizationData(ServerType serverType, string language)
    {
        string fileName = dataService.CombinePaths(GetDataPath(serverType), "Localization", $"{language}.json");
        return fileSystem.File.Exists(fileName) ? await DeserializeFile<Dictionary<string, string>>(fileName) : null;
    }

    public Ship GetShipFromSummary(ShipSummary summary) => AppData.ShipDictionary[summary.Index];

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
            var localizer = Locator.Current.GetService<ILocalizer>();
            if (localizer is null)
            {
                Logging.Logger.Warn("Localizer was null, aborting build loading.");
                return;
            }

            AppData.Builds = rawBuildList?
                .Select(str => Build.CreateBuildFromString(str))
                .ToList() ?? new List<Build>();
        }
    }

    /// <summary>
    /// Helper method to create the path for a build image file.
    /// </summary>
    /// <param name="buildName">The name of the saved build.</param>
    /// <param name="shipName">The name of the ship of the build.</param>
    /// <returns>The path where the generated image should be stored.</returns>
    public string GetImageOutputPath(string buildName, string shipName)
    {
        string directory = BuildImageOutputDirectory;
        fileSystem.Directory.CreateDirectory(directory);
        return dataService.CombinePaths(directory, shipName + " - " + buildName + ".png");
    }

    public async Task LoadLocalFilesAsync(ServerType serverType)
    {
        var sw = Stopwatch.StartNew();
        Logging.Logger.Debug("Loading local files from disk.");
        AppData.ResetCaches();
        var localVersionInfo = await GetCurrentVersionInfo(serverType) ?? throw new InvalidOperationException("No local data found");
        AppData.DataVersion = localVersionInfo.CurrentVersion.MainVersion.ToString(3) + "#" + localVersionInfo.CurrentVersion.DataIteration;

        var dataRootInfo = fileSystem.DirectoryInfo.FromDirectoryName(GetDataPath(serverType));
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

    internal async Task<T?> DeserializeFile<T>(string filePath)
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
