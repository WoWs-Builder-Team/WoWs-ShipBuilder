using System.Threading.Tasks;

namespace WoWsShipBuilder.Core.Services
{
    public interface IClipboardService
    {
        Task<string> GetTextAsync();

        Task SetTextAsync(string text);

        Task ClearAsync();
    }
}
