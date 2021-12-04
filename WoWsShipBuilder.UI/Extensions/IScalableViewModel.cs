using Avalonia;
using Avalonia.Platform;

namespace WoWsShipBuilder.UI.Extensions
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
