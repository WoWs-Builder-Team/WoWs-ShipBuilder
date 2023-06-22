using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using DynamicData;
using Newtonsoft.Json;
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
        var path = dataService.CombinePaths(appDataService.DefaultAppDataDirectory, "builds.json");
        var buildStrings = builds.Select(build => build.CreateShortStringFromBuild()).ToList();
        await dataService.StoreAsync(buildStrings, path);
    }

    public async Task<IEnumerable<Build>> LoadBuildsAsync()
    {
        if (savedBuilds is not null)
        {
            return savedBuilds;
        }

        string path = dataService.CombinePaths(appDataService.DefaultAppDataDirectory, "builds.json");
        if (fileSystem.File.Exists(path))
        {
            List<string>? buildList = null;
            try
            {
                buildList = await dataService.LoadAsync<List<string>>(path);
            }
            catch (JsonReaderException)
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

                savedBuilds = builds.DistinctBy(x => x.Hash).ToList();
            }

            return savedBuilds ?? Enumerable.Empty<Build>();
        }

        return Enumerable.Empty<Build>();
    }

    public async Task ImportBuildsAsync(IEnumerable<Build> builds)
    {
        savedBuilds ??= (await LoadBuildsAsync()).ToList();

        foreach (var build in builds.Where(x => AppData.ShipDictionary.ContainsKey(x.ShipIndex)))
        {
            savedBuilds.RemoveAll(x => x.Equals(build));
            savedBuilds.Insert(0, build);
        }

        await SaveBuildsAsync(savedBuilds);
    }

    public async Task SaveBuildAsync(Build build) => await ImportBuildsAsync(new List<Build> { build });

    public async Task RemoveSavedBuildAsync(Build build) => await RemoveSavedBuildsAsync(new List<Build> { build });

    public async Task RemoveSavedBuildsAsync(IEnumerable<Build> builds)
    {
        savedBuilds ??= (await LoadBuildsAsync()).ToList();
        savedBuilds.RemoveMany(builds);

        await SaveBuildsAsync(savedBuilds);
    }
}
