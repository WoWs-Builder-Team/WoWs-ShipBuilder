namespace WoWsShipBuilder.Features.DataTransfer;

public interface IClipboardService
{
    Task<string> GetTextAsync();

    Task SetTextAsync(string text);

    Task ClearAsync();
}
