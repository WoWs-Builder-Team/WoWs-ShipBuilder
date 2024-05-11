using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using DynamicData;
using Microsoft.JSInterop;
using MudBlazor;
using WoWsShipBuilder.Features.Builds;
using WoWsShipBuilder.Features.Builds.Components;
using WoWsShipBuilder.Infrastructure.ApplicationData;
using WoWsShipBuilder.Infrastructure.Localization;
using WoWsShipBuilder.Infrastructure.Localization.Resources;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace WoWsShipBuilder.Web.Infrastructure.Data;

public sealed class WebUserDataService : IUserDataService, IAsyncDisposable
{
    private const string JsFileName = "/scripts/userDataService.js";

    private const string BuildFileName = "recentBuilds";

    private readonly IJSRuntime runtime;

    private readonly ISnackbar snackbar;

    private readonly IDialogService dialogService;

    private readonly ILocalizer localizer;

    private IJSObjectReference? module;

    private List<Build>? savedBuilds;

    private const int LocalStorageLimit = 1000;

    public WebUserDataService(IJSRuntime runtime, ISnackbar snackbar, IDialogService dialogService, ILocalizer localizer)
    {
        this.snackbar = snackbar;
        this.runtime = runtime;
        this.dialogService = dialogService;
        this.localizer = localizer;
    }

    public async Task SaveBuildsAsync(IEnumerable<Build> builds)
    {
        await this.InitializeModule();
        var buildStrings = builds.Select(x => x.CreateShortStringFromBuild());
        await this.module.InvokeVoidAsync("saveData", BuildFileName, JsonSerializer.Serialize(buildStrings, AppConstants.JsonSerializerOptions));
    }

    public async Task<IEnumerable<Build>> LoadBuildsAsync()
    {
        if (this.savedBuilds is not null)
        {
            return this.savedBuilds;
        }

        await this.InitializeModule();
        var buildStrings = await this.module.InvokeAsync<string?>("loadData", BuildFileName);
        if (buildStrings is not null)
        {
            IEnumerable<string>? buildList = null;
            try
            {
                buildList =  JsonSerializer.Deserialize<IEnumerable<string>>(buildStrings, AppConstants.JsonSerializerOptions);
            }
            catch (JsonException)
            {
                // silently fails
            }

            if (buildList is not null)
            {
                var builds = new List<Build>();
                var counter = 0;
                foreach (string buildString in buildList)
                {
                    try
                    {
                        var build = Build.CreateBuildFromString(buildString);
                        if (AppData.ShipDictionary.ContainsKey(build.ShipIndex))
                        {
                            builds.Add(build);
                        }
                        else
                        {
                            counter++;
                        }
                    }
                    catch (FormatException)
                    {
                        counter++;
                    }
                }
                if (counter > 0)
                {
                    this.snackbar.Add($"{counter} {this.localizer.SimpleAppLocalization(nameof(Translation.UserDataService_BuildUpdated))}", Severity.Warning);
                }

                this.savedBuilds = builds.DistinctBy(x => x.Hash).ToList();
            }
        }

        return this.savedBuilds ?? new List<Build>();
    }

    public async Task ImportBuildsAsync(IEnumerable<Build> builds)
    {
        this.savedBuilds ??= (await this.LoadBuildsAsync()).ToList();
        this.snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomEnd;

        var buildsList = builds.ToList();
        int buildsUpdated = await this.AddOrUpdateBuilds(buildsList);

        if (buildsUpdated == -1)
        {
            return;
        }

        // actual local storage limit is around 3700 builds but we limit it to 1000 for performance reasons
        switch (this.savedBuilds.Count)
        {
            case < LocalStorageLimit:
            {
                if (buildsUpdated != buildsList.Count)
                {
                    this.snackbar.Add(this.localizer.SimpleAppLocalization(nameof(Translation.UserDataService_BuildSaved)), Severity.Success);
                }
                break;
            }
            case LocalStorageLimit:
            {
                this.snackbar.Add(this.localizer.SimpleAppLocalization(nameof(Translation.UserDataService_BuildLimitReached)), Severity.Warning);
                break;
            }
            case > LocalStorageLimit:
            {
                this.snackbar.Add(this.localizer.SimpleAppLocalization(nameof(Translation.UserDataService_BuildStorageFull)), Severity.Error);
                this.savedBuilds = this.savedBuilds.Take(LocalStorageLimit).ToList();
                break;
            }
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

    public async ValueTask DisposeAsync()
    {
        if (this.module is not null)
        {
            try
            {
                await this.module.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // Ignored
            }
        }
    }

    private async Task<int> AddOrUpdateBuilds(List<Build> buildsList)
    {
        this.savedBuilds ??= (await this.LoadBuildsAsync()).ToList();

        var buildsUpdated = 0;
        var buildsNotNeedingUpdate = 0;
        var buildAdded = false;

        foreach (var build in buildsList)
        {
            buildsNotNeedingUpdate += this.savedBuilds.RemoveAll(x => x.Equals(build));
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
                var result = await (await this.dialogService.ShowAsync<OverwriteExistingBuildConfirmationDialog>(string.Empty, parameters, options)).Result;
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
                    this.snackbar.Add(this.localizer.SimpleAppLocalization(nameof(Translation.UserDataService_BuildNotUpdated)), Severity.Warning);
                }
            }
            else
            {
                this.savedBuilds.Insert(0, build);
                buildAdded = true;
            }
        }

        if (!buildAdded && (buildsUpdated == 0 || buildsNotNeedingUpdate == buildsList.Count))
        {
            return -1;
        }

        return buildsUpdated;
    }

    [MemberNotNull(nameof(module))]
    private async Task InitializeModule()
    {
        // module is not null after this method but apparently, Roslyn does not want to recognize that.
#pragma warning disable CS8774
        this.module ??= await this.runtime.InvokeAsync<IJSObjectReference>("import", JsFileName);
#pragma warning restore CS8774
    }
}
