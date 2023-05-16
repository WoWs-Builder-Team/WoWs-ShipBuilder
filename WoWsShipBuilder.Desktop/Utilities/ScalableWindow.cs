using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.ReactiveUI;

namespace WoWsShipBuilder.Desktop.Utilities
{
    public class ScalableWindow : Window, IScalableWindow
    {
        public static readonly StyledProperty<double> ContentScalingProperty = AvaloniaProperty.Register<ScalableWindow, double>(nameof(ContentScaling));

        public double ContentScaling
        {
            get => GetValue(ContentScalingProperty);
            set => SetValue(ContentScalingProperty, value);
        }

        public void ProcessResizing(Size newSize, PlatformResizeReason resizeReason) => HandleResized(newSize, resizeReason);
    }

    public class ScalableReactiveWindow<T> : ReactiveWindow<T>, IScalableWindow where T : class
    {
        public void ProcessResizing(Size newSize, PlatformResizeReason resizeReason) => HandleResized(newSize, resizeReason);
    }
}
