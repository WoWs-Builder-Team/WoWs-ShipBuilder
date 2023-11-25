using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using DynamicData;
using MudBlazor;
using WoWsShipBuilder.Features.Builds;
using WoWsShipBuilder.Features.Builds.Components;
using WoWsShipBuilder.Infrastructure.ApplicationData;
using WoWsShipBuilder.Infrastructure.Localization;
using WoWsShipBuilder.Infrastructure.Localization.Resources;

namespace WoWsShipBuilder.Desktop.Infrastructure.Data;

public class DesktopUserDataService : IUserDataService
{
    private readonly IDataService dataService;

    private readonly IAppDataService appDataService;

    private readonly IFileSystem fileSystem;

    private readonly ISnackbar snackbar;

    private readonly IDialogService dialogService;

    private readonly ILocalizer localizer;

    private List<Build>? savedBuilds;

    public DesktopUserDataService(IDataService dataService, IAppDataService appDataService, IFileSystem fileSystem, ISnackbar snackbar, IDialogService dialogService, ILocalizer localizer)
    {
        this.dataService = dataService;
        this.appDataService = appDataService;
        this.fileSystem = fileSystem;
        this.snackbar = snackbar;
        this.dialogService = dialogService;
        this.localizer = localizer;
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
        this.snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomEnd;

        var buildsList = builds.ToList();
        var buildsUpdated = 0;
        var savingBuildCancelled = 0;

        foreach (var build in buildsList.Where(x => AppData.ShipDictionary.ContainsKey(x.ShipIndex)))
        {
            this.savedBuilds.RemoveAll(x => x.Equals(build));
            var buildToUpdate = this.savedBuilds.Find(x => x.ShipIndex.Equals(build.ShipIndex, StringComparison.Ordinal) && x.BuildName.Equals(build.BuildName, StringComparison.Ordinal));
            if (buildToUpdate != null)
            {
                DialogOptions options = new()
                {
                    NoHeader = true,
                    CloseOnEscapeKey = true,
                };
                DialogParameters parameters = new()
                {
                    ["BuildName"] = build.BuildName,
                    ["ShipIndex"] = build.ShipIndex,
                };
                var result = await (await this.dialogService.ShowAsync<UpdateSavedBuildConfirmationDialog>(string.Empty, parameters, options)).Result;
                if (!result.Canceled && (bool)result.Data)
                {
                    int index = this.savedBuilds.IndexOf(buildToUpdate);
                    this.savedBuilds.Remove(buildToUpdate);
                    this.savedBuilds.Insert(index, build);
                    this.snackbar.Add(this.localizer.SimpleAppLocalization(nameof(Translation.UserDataService_BuildUpdated)), Severity.Success);
                    buildsUpdated++;
                }
                else
                {
                    this.snackbar.Add(this.localizer.SimpleAppLocalization(nameof(Translation.UserDataService_BuildNotSaved)), Severity.Error);
                    savingBuildCancelled++;
                }
            }
            else
            {
                this.savedBuilds.Insert(0, build);
            }
        }

        if (savingBuildCancelled == buildsList.Count)
        {
            return;
        }

        if (buildsUpdated != buildsList.Count)
        {
            this.snackbar.Add(this.localizer.SimpleAppLocalization(nameof(Translation.UserDataService_BuildSaved)), Severity.Success);
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
