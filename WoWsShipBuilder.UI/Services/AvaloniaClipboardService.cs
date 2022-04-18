using System.Threading.Tasks;
using Avalonia;
using WoWsShipBuilder.Core.Services;

namespace WoWsShipBuilder.UI.Services
{
    public class AvaloniaClipboardService : IClipboardService
    {
        public async Task<string> GetTextAsync()
        {
            return await Application.Current!.Clipboard!.GetTextAsync();
        }

        public async Task SetTextAsync(string text)
        {
            await Application.Current!.Clipboard!.SetTextAsync(text);
        }

        public async Task ClearAsync()
        {
            await Application.Current!.Clipboard!.ClearAsync();
        }
    }
}
