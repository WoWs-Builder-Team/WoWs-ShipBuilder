using System.Diagnostics.CodeAnalysis;
using Microsoft.JSInterop;
using WoWsShipBuilder.Web.Components;

namespace WoWsShipBuilder.Web.Services;

public class DispersionPlotInterop : IAsyncDisposable
{
    private readonly IJSRuntime runtime;

    private IJSObjectReference? module;

    public DispersionPlotInterop(IJSRuntime runtime)
    {
        this.runtime = runtime;
    }

    public async Task DrawDispersionPlotBatched(Dictionary<string, DispersionPlot.DrawingData> data, double plotScaling,DispersionPlot.Size fusoReference)
    {
        await InitializeModule();
        await module.InvokeVoidAsync("DrawDispersionPlotBatched", data, plotScaling, fusoReference);
    }

    [MemberNotNull(nameof(module))]
    private async Task InitializeModule()
    {
        // module is not null after this method but apparently, Roslyn does not want to recognize that.
#pragma warning disable CS8774
        module ??= await runtime.InvokeAsync<IJSObjectReference>("import", "/scripts/DispersionPlotJsHelper.js");
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
