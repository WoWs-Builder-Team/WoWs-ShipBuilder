using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using WoWsShipBuilder.Infrastructure.DataTransfer;

namespace WoWsShipBuilder.Desktop.Infrastructure;

public class AvaloniaClipboardService : IClipboardService
{
    private static Window? MainWindow => Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop ? desktop.MainWindow : null;

    public async Task<string> GetTextAsync()
    {
        return await MainWindow!.Clipboard!.GetTextAsync() ?? string.Empty;
    }

    public async Task SetTextAsync(string text)
    {
        await MainWindow!.Clipboard!.SetTextAsync(text);
    }

    public async Task ClearAsync()
    {
        await MainWindow!.Clipboard!.ClearAsync();
    }
}
