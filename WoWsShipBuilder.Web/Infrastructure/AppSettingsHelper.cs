using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.JSInterop;
using WoWsShipBuilder.Features.Settings;
using WoWsShipBuilder.Infrastructure.ApplicationData;

namespace WoWsShipBuilder.Web.Infrastructure;

public sealed class WebSettingsAccessor : IAsyncDisposable, ISettingsAccessor
{
    private readonly IJSRuntime runtime;

    private IJSObjectReference? module;

    public WebSettingsAccessor(IJSRuntime runtime)
    {
        this.runtime = runtime;
    }

    public async Task<AppSettings?> LoadSettings()
    {
        await this.InitializeModule();
        var result = await this.module.InvokeAsync<AppSettings?>("getAppSettings");
        if (result is not null)
        {
            result.StoreBuildOnShare = false;
        }

        return result;
    }

    public async Task SaveSettings(AppSettings appSettings)
    {
        await this.InitializeModule();
        await this.module.InvokeVoidAsync("setAppSettings", appSettings);
    }

    [MemberNotNull(nameof(module))]
    private async Task InitializeModule()
    {
        // module is not null after this method but apparently, Roslyn does not want to recognize that.
#pragma warning disable CS8774
        this.module ??= await this.runtime.InvokeAsync<IJSObjectReference>("import", "/_content/WoWsShipBuilder.Common/scripts/settingsHelper.js");
#pragma warning restore CS8774
    }

    public async ValueTask DisposeAsync()
    {
        if (this.module is not null)
        {
            try
            {
                await this.module.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // Ignored
            }
        }
    }
}
