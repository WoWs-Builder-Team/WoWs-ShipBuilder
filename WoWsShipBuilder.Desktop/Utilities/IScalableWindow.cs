using Avalonia;
using Avalonia.Platform;

namespace WoWsShipBuilder.Desktop.Utilities
{
    public interface IScalableWindow
    {
        void ProcessResizing(Size newSize, PlatformResizeReason resizeReason);
    }
}
