using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using DynamicData;
using Microsoft.JSInterop;
using MudBlazor;
using WoWsShipBuilder.Features.Builds;
using WoWsShipBuilder.Infrastructure.ApplicationData;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace WoWsShipBuilder.Web.Infrastructure.Data;

public sealed class WebUserDataService : IUserDataService, IAsyncDisposable
{
    private const string JsFileName = "/scripts/userDataService.js";

    private const string BuildFileName = "recentBuilds";

    private readonly IJSRuntime runtime;

    private readonly ISnackbar snackbar;

    private IJSObjectReference? module;

    private List<Build>? savedBuilds;

    public WebUserDataService(IJSRuntime runtime, ISnackbar snackbar)
    {
        this.snackbar = snackbar;
        this.runtime = runtime;
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
                    this.snackbar.Add($"{counter} builds could not be loaded.", Severity.Warning);
                }

                this.savedBuilds = builds.DistinctBy(x => x.Hash).ToList();
            }
        }

        return this.savedBuilds ?? new List<Build>();
    }

    public async Task ImportBuildsAsync(IEnumerable<Build> builds)
    {
        this.savedBuilds ??= (await this.LoadBuildsAsync()).ToList();

        foreach (var build in builds)
        {
            this.savedBuilds.RemoveAll(x => x.Equals(build));
            this.savedBuilds.Insert(0, build);
        }

        this.snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomEnd;

        // actual local storage limit is around 3700 builds but we limit it to 1000 for performance reasons
        switch (this.savedBuilds.Count)
        {
            case < 1000:
            {
                this.snackbar.Add("Builds have been saved.", Severity.Success);
                break;
            }
            case 1000:
            {
                this.snackbar.Add("Builds storage limit reached. Next addition will replace the oldest saved build.", Severity.Warning);
                break;
            }
            case > 1000:
            {
                this.snackbar.Add("Builds storage is full. The oldest saved build has been replaced.", Severity.Error);
                this.savedBuilds = this.savedBuilds.Take(1000).ToList();
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

    [MemberNotNull(nameof(module))]
    private async Task InitializeModule()
    {
        // module is not null after this method but apparently, Roslyn does not want to recognize that.
#pragma warning disable CS8774
        this.module ??= await this.runtime.InvokeAsync<IJSObjectReference>("import", JsFileName);
#pragma warning restore CS8774
    }
}
