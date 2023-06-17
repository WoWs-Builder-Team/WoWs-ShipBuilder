using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using DynamicData;
using Microsoft.JSInterop;
using MudBlazor;
using WoWsShipBuilder.Features.Builds;
using WoWsShipBuilder.Infrastructure.ApplicationData;

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
        var result = buildStrings is null ? Enumerable.Empty<string>() : JsonSerializer.Deserialize<IEnumerable<string>>(buildStrings);
        savedBuilds = result!.Select(x => Build.CreateBuildFromString(x)).ToList();
        return savedBuilds;
    }

    public async Task ImportBuildsAsync(IEnumerable<Build> builds)
    {
        savedBuilds ??= (await LoadBuildsAsync()).ToList();

        foreach (var build in builds)
        {
            savedBuilds.RemoveAll(x => x.Equals(build));
            savedBuilds.Insert(0, build);
        }

        // actual local storage limit is around 3700 builds but we limit it to 1000 for performance reasons
        switch (savedBuilds.Count)
        {
            case < 1000:
            {
                snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomStart;
                snackbar.Add("Builds successfully saved.", Severity.Success);
                break;
            }
            case 1000:
            {
                snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomStart;
                snackbar.Add("Builds storage limit reached. Next addition will replace the oldest saved build.", Severity.Warning);
                break;
            }
            case > 1000:
            {
                snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomStart;
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
