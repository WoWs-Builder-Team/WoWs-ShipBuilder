using Avalonia;
using Avalonia.Controls;

namespace WoWsShipBuilder.Desktop.Utilities
{
    public class Scaling : AvaloniaObject
    {
        public static readonly AttachedProperty<double> ContentScalingProperty = AvaloniaProperty.RegisterAttached<Window, double>("ContentScaling", typeof(Scaling), 1);

        public static void SetContentScaling(AvaloniaObject element, double value)
        {
            element.SetValue(ContentScalingProperty, value);
        }

        public static double GetContentScaling(AvaloniaObject element)
        {
            return element.GetValue(ContentScalingProperty);
        }
    }
}
