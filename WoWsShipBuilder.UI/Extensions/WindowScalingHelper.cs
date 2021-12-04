using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;

namespace WoWsShipBuilder.UI.Extensions
{
    /// <summary>
    /// Utility class that provides extension methods and utilities to resize windows based on dpi scaling.
    /// </summary>
    public static class WindowScalingHelper
    {
        public static void HandleAndCheckScaling(this ScalableWindow window)
        {
            if (window.DataContext is IScalableViewModel viewModel)
            {
                InternalScalingHandler(window, viewModel);
            }
        }

        public static void HandleAndCheckScaling<T>(this ScalableReactiveWindow<T> window) where T : class
        {
            if (window.DataContext is IScalableViewModel viewModel)
            {
                InternalScalingHandler(window, viewModel);
            }
        }

        private static void InternalScalingHandler(Window window, IScalableViewModel viewModel)
        {
            var currentScreen = window.Screens.ScreenFromVisual(window);

            (bool shouldScale, double scaling) = CheckScaling(currentScreen, window);
            if (shouldScale)
            {
                viewModel.ContentScaling = scaling;
                RepositionWindow(window, viewModel.ContentScaling, currentScreen);
            }
        }

        public static KeyValuePair<bool, double> CheckScaling(Screen currentScreen, Window currentWindow)
        {
            var shouldScale = false;
            double scalingFactor = 0;
            var screenPixelSize = currentScreen.WorkingArea.Size;
            var windowPixelSize = PixelSize.FromSize(currentWindow.Bounds.Size, currentScreen.PixelDensity);
            if (screenPixelSize.Width < windowPixelSize.Width || screenPixelSize.Height < windowPixelSize.Height)
            {
                var horizontalScaling = (screenPixelSize.Width - 25) / (double)windowPixelSize.Width;
                var verticalScaling = (screenPixelSize.Height - 25) / (double)windowPixelSize.Height;
                scalingFactor = Math.Min(horizontalScaling, verticalScaling);
                shouldScale = true;
            }

            return new(shouldScale, scalingFactor);
        }

        private static void RepositionWindow(Window window, double scaling, Screen currentScreen)
        {
            var newWidth = window.Width * scaling;
            var newHeight = window.Height * scaling;
            var pixelSize = PixelSize.FromSize(new(newWidth, newHeight), currentScreen.PixelDensity);
            var left = (currentScreen.WorkingArea.Width - pixelSize.Width) / 2;
            var top = (currentScreen.WorkingArea.Height - pixelSize.Height) / 2;
            var windowPoint = new PixelPoint(left, top);

            window.Position = windowPoint + currentScreen.WorkingArea.Position;
            window.Width = newWidth;
            window.Height = newHeight;

            window.MinWidth = window.Width;
            window.MinHeight = window.Height;
            if (window is IScalableWindow scalableWindow)
            {
                scalableWindow.ProcessResizing(pixelSize.ToSize(currentScreen.PixelDensity), PlatformResizeReason.Layout);
            }
        }
    }
}
