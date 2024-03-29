﻿using Microsoft.JSInterop;

namespace WoWsShipBuilder.Web.Infrastructure;

using WoWsShipBuilder.Infrastructure.DataTransfer;

/// <summary>
/// Implementation of the clipboard service for the web app.
/// Allows to set and read text from the clipboard.
/// </summary>
public class WebClipboardService : IClipboardService
{
    private readonly IJSRuntime jsRuntime;

    public WebClipboardService(IJSRuntime jsRuntime)
    {
        this.jsRuntime = jsRuntime;
    }

    public async Task<string> GetTextAsync()
    {
        return await this.jsRuntime.InvokeAsync<string>("navigator.clipboard.readText");
    }

    public async Task SetTextAsync(string text)
    {
        await this.jsRuntime.InvokeVoidAsync("navigator.clipboard.writeText", text);
    }

    public Task ClearAsync()
    {
        return Task.CompletedTask;
    }
}
