using System.Diagnostics.CodeAnalysis;
using Microsoft.JSInterop;

namespace WoWsShipBuilder.Common.Infrastructure;

public class MouseEventInterop : IAsyncDisposable
{
    private readonly IJSRuntime runtime;

    private IJSObjectReference? module;

    public MouseEventInterop(IJSRuntime runtime)
    {
        this.runtime = runtime;
    }

    public async Task PreventMiddleClickDefault(string id)
    {
        await InitializeModule();
        await module.InvokeVoidAsync("PreventMiddleClickDefault", id);
    }

    public async Task PreventMiddleClickDefaultBatched(IEnumerable<string> ids)
    {
        await InitializeModule();
        await module.InvokeVoidAsync("PreventMiddleClickDefaultBatched", ids);
    }

    [MemberNotNull(nameof(module))]
    private async Task InitializeModule()
    {
        // module is not null after this method but apparently, Roslyn does not want to recognize that.
#pragma warning disable CS8774
        module ??= await runtime.InvokeAsync<IJSObjectReference>("import", "/_content/WoWsShipBuilder.Common/scripts/MouseEventHelper.js");
#pragma warning restore CS8774
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
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
}
