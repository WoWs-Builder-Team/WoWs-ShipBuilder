using System.Diagnostics.CodeAnalysis;
using Microsoft.JSInterop;
using WoWsShipBuilder.Core.DataContainers;

namespace WoWsShipBuilder.Web.Services;

public class DepthChargeDamageDistributionChartInterop : IAsyncDisposable
{
    private readonly IJSRuntime runtime;

    private IJSObjectReference? module;

    public DepthChargeDamageDistributionChartInterop(IJSRuntime runtime)
    {
        this.runtime = runtime;
    }

    public async Task DrawDepthChargeDamageDistributionChart(string id, DepthChargeDamageDistributionChartRecord dataRecord)
    {
        await InitializeModule();
        await module.InvokeVoidAsync("DrawDepthChargeDamageDistributionChart", id, dataRecord);
    }

    [MemberNotNull(nameof(module))]
    private async Task InitializeModule()
    {
        // module is not null after this method but apparently, Roslyn does not want to recognize that.
#pragma warning disable CS8774
        module ??= await runtime.InvokeAsync<IJSObjectReference>("import", "/scripts/DepthChargeDamageDistributionChart.js");
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
