using System.Diagnostics.CodeAnalysis;
using DynamicData;
using Microsoft.JSInterop;
using MudBlazor;
using Newtonsoft.Json;
using WoWsShipBuilder.Features.Builds;
using WoWsShipBuilder.Infrastructure.ApplicationData;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace WoWsShipBuilder.Web.Infrastructure.Data;

public class WebUserDataService : IUserDataService, IAsyncDisposable
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
        await InitializeModule();
        var buildStrings = builds.Select(x => x.CreateShortStringFromBuild());
        await module.InvokeVoidAsync("saveData", BuildFileName, JsonSerializer.Serialize(buildStrings));
    }

    public async Task<IEnumerable<Build>> LoadBuildsAsync()
    {
        if (savedBuilds is not null)
        {
            return savedBuilds;
        }

        await InitializeModule();
        var buildStrings = await module.InvokeAsync<string?>("loadData", BuildFileName);
        if (buildStrings is not null)
        {
            IEnumerable<string>? buildList = null;
            try
            {
                buildList =  JsonSerializer.Deserialize<IEnumerable<string>>(buildStrings);
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
                    snackbar.Add($"{counter} builds could not be loaded.", Severity.Warning);
                }

                savedBuilds = builds.DistinctBy(x => x.Hash).ToList();
            }
        }

        return savedBuilds ?? new List<Build>();
    }

    public async Task ImportBuildsAsync(IEnumerable<Build> builds)
    {
        savedBuilds ??= (await LoadBuildsAsync()).ToList();

        foreach (var build in builds)
        {
            savedBuilds.RemoveAll(x => x.Equals(build));
            savedBuilds.Insert(0, build);
        }

        snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomEnd;

        // actual local storage limit is around 3700 builds but we limit it to 1000 for performance reasons
        switch (savedBuilds.Count)
        {
            case < 1000:
            {
                snackbar.Add("Builds have been saved.", Severity.Success);
                break;
            }
            case 1000:
            {
                snackbar.Add("Builds storage limit reached. Next addition will replace the oldest saved build.", Severity.Warning);
                break;
            }
            case > 1000:
            {
                snackbar.Add("Builds storage is full. The oldest saved build has been replaced.", Severity.Error);
                savedBuilds = savedBuilds.Take(1000).ToList();
                break;
            }
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

    public async ValueTask DisposeAsync()
    {
        if (module is not null)
        {
            try
            {
                await module.DisposeAsync();
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
        module ??= await runtime.InvokeAsync<IJSObjectReference>("import", JsFileName);
#pragma warning restore CS8774
    }
}
