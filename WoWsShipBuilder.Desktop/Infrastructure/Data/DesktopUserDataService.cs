using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using WoWsShipBuilder.Features.Builds;
using WoWsShipBuilder.Infrastructure.ApplicationData;

namespace WoWsShipBuilder.Desktop.Infrastructure.Data;

public class DesktopUserDataService : IUserDataService
{
    private readonly IDataService dataService;

    private readonly IAppDataService appDataService;

    private readonly IFileSystem fileSystem;

    private List<Build>? recentBuilds;

    public DesktopUserDataService(IDataService dataService, IAppDataService appDataService, IFileSystem fileSystem)
    {
        this.dataService = dataService;
        this.appDataService = appDataService;
        this.fileSystem = fileSystem;
    }

    public async Task SaveBuildsAsync(IEnumerable<Build> builds)
    {
        var path = dataService.CombinePaths(appDataService.DefaultAppDataDirectory, "builds.json");
        var buildStrings = builds.Select(build => build.CreateShortStringFromBuild()).ToList();
        await dataService.StoreAsync(buildStrings, path);
    }

    public async Task<IEnumerable<Build>> LoadBuildsAsync()
    {
        if (recentBuilds is not null)
        {
            return recentBuilds;
        }

        string path = dataService.CombinePaths(appDataService.DefaultAppDataDirectory, "builds.json");
        if (fileSystem.File.Exists(path))
        {
            var rawBuildList = await dataService.LoadAsync<List<string>>(path);
            var builds = rawBuildList?
                .Select(str => Build.CreateBuildFromString(str))
                .Take(10)
                .ToList() ?? Enumerable.Empty<Build>();
            recentBuilds = builds.DistinctBy(x => x.Hash).ToList();
            return recentBuilds;
        }

        return Enumerable.Empty<Build>();
    }

    public async Task AddRecentBuildAsync(Build build)
    {
        recentBuilds ??= (await LoadBuildsAsync()).ToList();
        recentBuilds.RemoveAll(x => x.Equals(build));
        recentBuilds.Insert(0, build);
        if (recentBuilds.Count > 10)
        {
            recentBuilds = recentBuilds.Take(10).ToList();
        }

        await SaveBuildsAsync(recentBuilds);
    }
}
