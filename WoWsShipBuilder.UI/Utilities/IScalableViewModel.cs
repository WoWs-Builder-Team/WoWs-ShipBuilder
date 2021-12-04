using Avalonia;
using Avalonia.Platform;

namespace WoWsShipBuilder.UI.Utilities
{
    internal interface IScalableViewModel
    {
        double ContentScaling { get; set; }
    }

    public interface IScalableWindow
    {
        void ProcessResizing(Size newSize, PlatformResizeReason resizeReason);
    }
}
