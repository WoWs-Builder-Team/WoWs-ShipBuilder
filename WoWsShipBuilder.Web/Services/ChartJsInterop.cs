using System.Diagnostics.CodeAnalysis;
using Microsoft.JSInterop;
using WoWsShipBuilder.Web.Utility;

namespace WoWsShipBuilder.Web.Services;

public class ChartJsInterop : IAsyncDisposable
{
    private readonly IJSRuntime runtime;

    private IJSObjectReference? module;

    public ChartJsInterop(IJSRuntime runtime)
    {
        this.runtime = runtime;
    }

    public async Task SetupGlobalChartConfigAsync(double aspectRatio)
    {
        await InitializeModule();
        await module.InvokeVoidAsync("SetupGlobalChartConfig", aspectRatio);
    }

    public async Task CreateChartAsync(string chartId, string title, string xLabel, string yLabel, string xUnit, string yUnit)
    {
        await InitializeModule();
        await module.InvokeVoidAsync("CreateChart", chartId, title, xLabel, yLabel, xUnit, yUnit);
    }

    public async Task ChangeSuggestedMaxAsync(string chartId, double newSuggestedMax)
    {
        await InitializeModule();
        await module.InvokeVoidAsync("ChangeSuggestedMax", chartId, newSuggestedMax);
    }

    public async Task BatchAddDataAsync(List<string> chartIds, List<string> labels, List<List<IEnumerable<Point>>> data, List<int> indexes)
    {
        await InitializeModule();
        await module.InvokeVoidAsync("BatchAddData", chartIds, data, labels, indexes);
    }

    public async Task BatchRemoveDataAsync(List<string> chartId, List<string> labels)
    {
        await InitializeModule();
        await module.InvokeVoidAsync("BatchRemoveData", chartId, labels);
    }

    public async Task BatchUpdateDataAsync(string chartId, List<string> labels, List<IEnumerable<Point>> datas)
    {
        await InitializeModule();
        await module.InvokeVoidAsync("BatchUpdateData", chartId, labels, datas);
    }

    [MemberNotNull(nameof(module))]
    private async Task InitializeModule()
    {
        // module is not null after this method but apparently, Roslyn does not want to recognize that.
#pragma warning disable CS8774
        module ??= await runtime.InvokeAsync<IJSObjectReference>("import", "/scripts/ChartsHelper.js");
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
