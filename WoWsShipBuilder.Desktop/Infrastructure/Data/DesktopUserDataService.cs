using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using DynamicData;
using WoWsShipBuilder.Features.Builds;
using WoWsShipBuilder.Infrastructure.ApplicationData;

namespace WoWsShipBuilder.Desktop.Infrastructure.Data;

public class DesktopUserDataService : IUserDataService
{
    private readonly IDataService dataService;

    private readonly IAppDataService appDataService;

    private readonly IFileSystem fileSystem;

    private List<Build>? savedBuilds;

    public DesktopUserDataService(IDataService dataService, IAppDataService appDataService, IFileSystem fileSystem)
    {
        this.dataService = dataService;
        this.appDataService = appDataService;
        this.fileSystem = fileSystem;
    }

    public async Task SaveBuildsAsync(IEnumerable<Build> builds)
    {
        var path = this.dataService.CombinePaths(this.appDataService.DefaultAppDataDirectory, "builds.json");
        var buildStrings = builds.Select(build => build.CreateShortStringFromBuild()).ToList();
        await this.dataService.StoreAsync(buildStrings, path);
    }

    public async Task<IEnumerable<Build>> LoadBuildsAsync()
    {
        if (this.savedBuilds is not null)
        {
            return this.savedBuilds;
        }

        string path = this.dataService.CombinePaths(this.appDataService.DefaultAppDataDirectory, "builds.json");

        if (!this.fileSystem.File.Exists(path))
        {
            return Enumerable.Empty<Build>();
        }

        List<string>? buildList = null;
        try
        {
            buildList = await this.dataService.LoadAsync<List<string>>(path);
        }
        catch (JsonException)
        {
            // silently fails
        }

        if (buildList is not null)
        {
            var builds = new List<Build>();
            foreach (string buildString in buildList)
            {
                try
                {
                    var build = Build.CreateBuildFromString(buildString);
                    if (AppData.ShipDictionary.ContainsKey(build.ShipIndex))
                    {
                        builds.Add(build);
                    }
                }
                catch (FormatException)
                {
                    // silently fails
                }
            }

            this.savedBuilds = builds.DistinctBy(x => x.Hash).ToList();
        }

        return this.savedBuilds ?? Enumerable.Empty<Build>();
    }

    public async Task ImportBuildsAsync(IEnumerable<Build> builds)
    {
        this.savedBuilds ??= (await this.LoadBuildsAsync()).ToList();

        foreach (var build in builds.Where(x => AppData.ShipDictionary.ContainsKey(x.ShipIndex)))
        {
            this.savedBuilds.RemoveAll(x => x.Equals(build));
            this.savedBuilds.Insert(0, build);
        }

        await this.SaveBuildsAsync(this.savedBuilds);
    }

    public async Task SaveBuildAsync(Build build) => await this.ImportBuildsAsync(new List<Build> { build });

    public async Task RemoveSavedBuildAsync(Build build) => await this.RemoveSavedBuildsAsync(new List<Build> { build });

    public async Task RemoveSavedBuildsAsync(IEnumerable<Build> builds)
    {
        this.savedBuilds ??= (await this.LoadBuildsAsync()).ToList();
        this.savedBuilds.RemoveMany(builds);

        await this.SaveBuildsAsync(this.savedBuilds);
    }
}
