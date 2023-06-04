using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.JSInterop;
using WoWsShipBuilder.Features.Builds;
using WoWsShipBuilder.Infrastructure.ApplicationData;

namespace WoWsShipBuilder.Web.Infrastructure.Data;

public class WebUserDataService : IUserDataService, IAsyncDisposable
{
    private const string JsFileName = "/scripts/userDataService.js";

    private const string BuildFileName = "recentBuilds";

    private readonly IJSRuntime runtime;

    private IJSObjectReference? module;

    private List<Build>? recentBuilds;

    public WebUserDataService(IJSRuntime runtime)
    {
        this.runtime = runtime;
    }

    public async Task SaveBuildsAsync(IEnumerable<Build> builds)
    {
        await InitializeModule();
        var buildStrings = builds.Select(x => x.CreateStringFromBuild());
        await module.InvokeVoidAsync("saveData", BuildFileName, JsonSerializer.Serialize(buildStrings));
    }

    public async Task<IEnumerable<Build>> LoadBuildsAsync()
    {
        if (recentBuilds is not null)
        {
            return recentBuilds;
        }

        await InitializeModule();
        var buildStrings = await module.InvokeAsync<string?>("loadData", BuildFileName);
        var result = buildStrings is null ? Enumerable.Empty<string>() : JsonSerializer.Deserialize<IEnumerable<string>>(buildStrings);
        recentBuilds = result!.Select(x => Build.CreateBuildFromString(x)).ToList();
        return recentBuilds;
    }

    public async Task AddRecentBuildAsync(Build build)
    {
        recentBuilds ??= (await LoadBuildsAsync()).ToList();
        recentBuilds.Insert(0, build);
        if (recentBuilds.Count > 10)
        {
            recentBuilds = recentBuilds.Take(10).ToList();
        }

        await SaveBuildsAsync(recentBuilds);
    }

    public async Task RemoveRecentBuildAsync(Build build)
    {
        recentBuilds ??= (await LoadBuildsAsync()).ToList();
        recentBuilds.Remove(build);

        await SaveBuildsAsync(recentBuilds);
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
