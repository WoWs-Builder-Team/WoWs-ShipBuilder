using System.Diagnostics.CodeAnalysis;
using Microsoft.JSInterop;
using WoWsShipBuilder.Common.Features.Charts.Data;

namespace WoWsShipBuilder.Common.Features.Charts;

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

    public async Task BatchAddDataAsync(List<string> chartIds, List<NewChartDataInput> newChartData)
    {
        if (newChartData.Any(x => x.Datasets.Count != chartIds.Count))
        {
            throw new InvalidOperationException("The number of chartId is not equal to the number of dataset of each NewChartDataInput");
        }

        await InitializeModule();
        await module.InvokeVoidAsync("BatchAddData", chartIds, newChartData);
    }

    public async Task BatchRemoveDataAsync(List<string> chartId, List<string> id)
    {
        await InitializeModule();
        await module.InvokeVoidAsync("BatchRemoveData", chartId, id);
    }

    public async Task BatchUpdateDataAsync(string chartId, List<UpdateChartDataInput> updatedChartData)
    {
        await InitializeModule();
        await module.InvokeVoidAsync("BatchUpdateData", chartId, updatedChartData);
    }

    public async Task BatchUpdateDataNewLabelsAsync(string chartId, List<UpdateChartDataLabelInput> updatedChartData)
    {
        await InitializeModule();
        await module.InvokeVoidAsync("BatchUpdateDataNewLabels", chartId, updatedChartData);
    }

    public async Task MultipleBatchUpdateDataNewLabels(List<string> chartIds, List<MultipleUpdateChartDataLabelInput> multipleUpdatedChartDataList)
    {
        if (multipleUpdatedChartDataList.Any(x => x.Datasets.Count != chartIds.Count))
        {
            throw new InvalidOperationException("The number of chartId is not equal to the number of dataset of each MultipleUpdatedChartDataList");
        }

        await InitializeModule();
        await module.InvokeVoidAsync("MultipleBatchUpdateDataNewLabels", chartIds, multipleUpdatedChartDataList);
    }

    public async Task MultipleBatchAddOrUpdateDataNewLabels(List<string> chartIds, List<MultipleUpdateChartDataLabelInput> multipleUpdatedChartDataList)
    {
        if (multipleUpdatedChartDataList.Any(x => x.Datasets.Count != chartIds.Count))
        {
            throw new InvalidOperationException("The number of chartId is not equal to the number of dataset of each MultipleUpdatedChartDataList");
        }

        await InitializeModule();
        await module.InvokeVoidAsync("MultipleBatchAddOrUpdateDataNewLabels", chartIds, multipleUpdatedChartDataList);
    }

    [MemberNotNull(nameof(module))]
    private async Task InitializeModule()
    {
        // module is not null after this method but apparently, Roslyn does not want to recognize that.
#pragma warning disable CS8774
        module ??= await runtime.InvokeAsync<IJSObjectReference>("import", "/_content/WoWsShipBuilder.Common/scripts/ChartsHelper.js");
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
                // ignore js disconnections
            }
        }
    }
}
