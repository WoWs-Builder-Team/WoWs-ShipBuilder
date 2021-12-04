using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.ReactiveUI;
using WoWsShipBuilder.UI.Extensions;

namespace WoWsShipBuilder.UI.Utilities
{
    public class ScalableWindow : Window, IScalableWindow
    {
        public void ProcessResizing(Size newSize, PlatformResizeReason resizeReason) => HandleResized(newSize, resizeReason);
    }

    public class ScalableReactiveWindow<T> : ReactiveWindow<T>, IScalableWindow where T : class
    {
        public void ProcessResizing(Size newSize, PlatformResizeReason resizeReason) => HandleResized(newSize, resizeReason);
    }
}
