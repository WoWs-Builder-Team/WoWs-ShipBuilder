using System.Diagnostics.CodeAnalysis;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using WoWsShipBuilder.Common.Features.Settings;

namespace WoWsShipBuilder.Common.Infrastructure.Utility;

public class AppSettingsHelper : IAsyncDisposable
{
    private readonly IJSRuntime runtime;

    private IJSObjectReference? module;

    public AppSettingsHelper(IJSRuntime runtime)
    {
        this.runtime = runtime;
    }

    public async ValueTask<AppSettings?> LoadSettings()
    {
        await InitializeModule();
        var settingsString = await module.InvokeAsync<string?>("getAppSettings");
        return settingsString == null ? null : JsonConvert.DeserializeObject<AppSettings>(settingsString);
    }

    public async Task SaveSettings(AppSettings appSettings)
    {
        await InitializeModule();
        await module.InvokeVoidAsync("setAppSettings", JsonConvert.SerializeObject(appSettings));
    }

    [MemberNotNull(nameof(module))]
    private async Task InitializeModule()
    {
        // module is not null after this method but apparently, Roslyn does not want to recognize that.
#pragma warning disable CS8774
        module ??= await runtime.InvokeAsync<IJSObjectReference>("import", "/_content/WoWsShipBuilder.Common/scripts/settingsHelper.js");
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
}
