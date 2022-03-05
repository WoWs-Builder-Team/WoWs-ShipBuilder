using Avalonia;
using Avalonia.Platform;

namespace WoWsShipBuilder.UI.Utilities
{
    public interface IScalableWindow
    {
        void ProcessResizing(Size newSize, PlatformResizeReason resizeReason);
    }
}
