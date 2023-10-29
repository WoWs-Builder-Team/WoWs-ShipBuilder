using System.Diagnostics.CodeAnalysis;
using Microsoft.JSInterop;
using WoWsShipBuilder.Features.BallisticCharts.Data;

namespace WoWsShipBuilder.Features.BallisticCharts;

public sealed class ChartJsInterop : IAsyncDisposable
{
    private readonly IJSRuntime runtime;

    private IJSObjectReference? module;

    public ChartJsInterop(IJSRuntime runtime)
    {
        this.runtime = runtime;
    }

    public async Task SetupGlobalChartConfigAsync(double aspectRatio)
    {
        await this.InitializeModule();
        await this.module.InvokeVoidAsync("SetupGlobalChartConfig", aspectRatio);
    }

    public async Task CreateChartAsync(string chartId, string title, string xLabel, string yLabel, string xUnit, string yUnit)
    {
        await this.InitializeModule();
        await this.module.InvokeVoidAsync("CreateChart", chartId, title, xLabel, yLabel, xUnit, yUnit);
    }

    public async Task ChangeSuggestedMaxAsync(string chartId, double newSuggestedMax)
    {
        await this.InitializeModule();
        await this.module.InvokeVoidAsync("ChangeSuggestedMax", chartId, newSuggestedMax);
    }

    public async Task BatchAddDataAsync(List<string> chartIds, List<NewChartDataInput> newChartData)
    {
        if (newChartData.Exists(x => x.Datasets.Count != chartIds.Count))
        {
            throw new InvalidOperationException("The number of chartId is not equal to the number of dataset of each NewChartDataInput");
        }

        await this.InitializeModule();
        await this.module.InvokeVoidAsync("BatchAddData", chartIds, newChartData);
    }

    public async Task BatchRemoveDataAsync(List<string> chartId, List<string> id)
    {
        await this.InitializeModule();
        await this.module.InvokeVoidAsync("BatchRemoveData", chartId, id);
    }

    public async Task BatchUpdateDataAsync(string chartId, List<UpdateChartDataInput> updatedChartData)
    {
        await this.InitializeModule();
        await this.module.InvokeVoidAsync("BatchUpdateData", chartId, updatedChartData);
    }

    public async Task BatchUpdateDataNewLabelsAsync(string chartId, List<UpdateChartDataLabelInput> updatedChartData)
    {
        await this.InitializeModule();
        await this.module.InvokeVoidAsync("BatchUpdateDataNewLabels", chartId, updatedChartData);
    }

    public async Task MultipleBatchUpdateDataNewLabels(List<string> chartIds, List<MultipleUpdateChartDataLabelInput> multipleUpdatedChartDataList)
    {
        if (multipleUpdatedChartDataList.Exists(x => x.Datasets.Count != chartIds.Count))
        {
            throw new InvalidOperationException("The number of chartId is not equal to the number of dataset of each MultipleUpdatedChartDataList");
        }

        await this.InitializeModule();
        await this.module.InvokeVoidAsync("MultipleBatchUpdateDataNewLabels", chartIds, multipleUpdatedChartDataList);
    }

    public async Task MultipleBatchAddOrUpdateDataNewLabels(List<string> chartIds, List<MultipleUpdateChartDataLabelInput> multipleUpdatedChartDataList)
    {
        if (multipleUpdatedChartDataList.Exists(x => x.Datasets.Count != chartIds.Count))
        {
            throw new InvalidOperationException("The number of chartId is not equal to the number of dataset of each MultipleUpdatedChartDataList");
        }

        await this.InitializeModule();
        await this.module.InvokeVoidAsync("MultipleBatchAddOrUpdateDataNewLabels", chartIds, multipleUpdatedChartDataList);
    }

    [MemberNotNull(nameof(module))]
    private async Task InitializeModule()
    {
        // module is not null after this method but apparently, Roslyn does not want to recognize that.
#pragma warning disable CS8774
        this.module ??= await this.runtime.InvokeAsync<IJSObjectReference>("import", "/_content/WoWsShipBuilder.Common/scripts/ChartsHelper.js");
#pragma warning restore CS8774
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        if (this.module is not null)
        {
            try
            {
                await this.module.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // ignore js disconnections
            }
        }
    }
}
