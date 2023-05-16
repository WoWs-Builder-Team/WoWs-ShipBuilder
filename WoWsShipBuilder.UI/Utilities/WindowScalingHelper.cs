﻿using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using Microsoft.Extensions.Logging;
using WoWsShipBuilder.Core;
using WoWsShipBuilder.Infrastructure;

namespace WoWsShipBuilder.UI.Utilities
{
    /// <summary>
    /// Utility class that provides extension methods and utilities to resize windows based on dpi scaling.
    /// </summary>
    public static class WindowScalingHelper
    {
        public static void HandleAndCheckScaling(this ScalableWindow window)
        {
            InternalScalingHandler(window);
        }

        public static void HandleAndCheckScaling<T>(this ScalableReactiveWindow<T> window) where T : class
        {
            InternalScalingHandler(window);
        }

        private static void InternalScalingHandler(Window window)
        {
            var currentScreen = window.Screens.ScreenFromVisual(window);

            (bool shouldScale, double scaling) = CheckScaling(currentScreen, window);
            if (shouldScale)
            {
                window.SetValue(Scaling.ContentScalingProperty, scaling);
                RepositionWindow(window, scaling, currentScreen);
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

            Logging.Logger.LogInformation("Checked scaling for window {Title} with result {ShouldScale} and factor {ScalingFactor}", currentWindow.Title, shouldScale, scalingFactor);
            return new(shouldScale, scalingFactor);
        }

        private static void RepositionWindow(Window window, double scaling, Screen currentScreen)
        {
            Logging.Logger.LogInformation("Rearranging window position and scaling window size");
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
            else
            {
                Logging.Logger.LogWarning("Window resizing was triggered on a window that is not scalable");
            }
        }
    }
}
