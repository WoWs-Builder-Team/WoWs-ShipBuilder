namespace WoWsShipBuilder.Common.Infrastructure
{
    public interface IClipboardService
    {
        Task<string> GetTextAsync();

        Task SetTextAsync(string text);

        Task ClearAsync();
    }
}
