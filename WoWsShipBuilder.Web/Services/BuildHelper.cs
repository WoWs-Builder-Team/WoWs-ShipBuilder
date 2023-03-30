namespace WoWsShipBuilder.Web.Services;

using System.Diagnostics.CodeAnalysis;
using Microsoft.JSInterop;
using Newtonsoft.Json;

public class BuildHelper : IAsyncDisposable
{
    private readonly IJSRuntime runtime;

    private IJSObjectReference? module;

    public BuildHelper(IJSRuntime runtime)
    {
        this.runtime = runtime;
    }

    public async ValueTask<List<BuildHelperContainer>?> RetrieveBuildContainers()
    {
        await InitializeModule();
        var containers = await module.InvokeAsync<string?>("loadContainers");
        await module.InvokeVoidAsync("deleteContainersFile");
        return containers is null ? null : JsonConvert.DeserializeObject<List<BuildHelperContainer>>(containers);
    }

    public async Task StoreBuildContainers(List<BuildHelperContainer> containers)
    {
        await InitializeModule();
        await module.InvokeVoidAsync("saveContainers", JsonConvert.SerializeObject(containers));
    }

    [MemberNotNull(nameof(module))]
    private async Task InitializeModule()
    {
        // module is not null after this method but apparently, Roslyn does not want to recognize that.
#pragma warning disable CS8774
        module ??= await runtime.InvokeAsync<IJSObjectReference>("import", "/scripts/buildHelper.js");
#pragma warning restore CS8774
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
            }
        }
    }

    public sealed record BuildHelperContainer(string ShipIndex, string BuildString, List<(string, float)>? Modifiers, IEnumerable<int>? ActivatedConsumables);
}
