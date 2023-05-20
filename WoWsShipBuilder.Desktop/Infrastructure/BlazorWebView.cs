using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using DynamicData;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Color = System.Drawing.Color;

namespace WoWsShipBuilder.Desktop.Infrastructure;

public class BlazorWebView : NativeControlHost
{
    private Uri? source;
    private Microsoft.AspNetCore.Components.WebView.WindowsForms.BlazorWebView? blazorWebView;
    private double zoomFactor = 1.0;
    private string? hostPage;
    private IServiceProvider serviceProvider = default!;
    private RootComponentsCollection rootComponents = new();

    /// <summary>
    /// The <see cref="AvaloniaProperty" /> which backs the <see cref="ZoomFactor" /> property.
    /// </summary>
    public static readonly DirectProperty<BlazorWebView, double> ZoomFactorProperty
        = AvaloniaProperty.RegisterDirect<BlazorWebView, double>(
            nameof(ZoomFactor),
            x => x.ZoomFactor,
            (x, y) => x.ZoomFactor = y);

    public static readonly DirectProperty<BlazorWebView, IServiceProvider> ServicesProperty
        = AvaloniaProperty.RegisterDirect<BlazorWebView, IServiceProvider>(
            nameof(Services),
            x => x.Services,
            (x, y) => x.Services = y);

    public static readonly DirectProperty<BlazorWebView, RootComponentsCollection> RootComponentsProperty
        = AvaloniaProperty.RegisterDirect<BlazorWebView, RootComponentsCollection>(
            nameof(RootComponents),
            x => x.RootComponents,
            (x, y) => x.RootComponents = y);

    public string? HostPage
    {
        get
        {
            if (blazorWebView != null)
            {
                hostPage = blazorWebView.HostPage;
            }

            return hostPage;
        }

        set
        {
            if (hostPage != value)
            {
                hostPage = value;
                if (blazorWebView != null)
                {
                    blazorWebView.HostPage = value;
                }
            }
        }
    }

    public Uri? Source
    {
        get
        {
            if (blazorWebView != null)
            {
                source = blazorWebView.WebView.Source;
            }

            return source;
        }

        set
        {
            if (source != value)
            {
                source = value;
                if (blazorWebView != null)
                {
                    blazorWebView.WebView.Source = value;
                }
            }
        }
    }

    public double ZoomFactor
    {
        get
        {
            if (blazorWebView != null)
            {
                zoomFactor = blazorWebView.WebView.ZoomFactor;
            }

            return zoomFactor;
        }

        set
        {
            if (zoomFactor != value)
            {
                zoomFactor = value;
                if (blazorWebView != null)
                {
                    blazorWebView.WebView.ZoomFactor = value;
                }
            }
        }
    }

    public IServiceProvider Services
    {
        get => serviceProvider;
        set
        {
            serviceProvider = value;
            if (blazorWebView != null)
            {
                blazorWebView.Services = serviceProvider;
            }
        }
    }

    public RootComponentsCollection RootComponents
    {
        get => rootComponents;
        set => rootComponents = value;
    }

    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        if (OperatingSystem.IsWindows())
        {
            blazorWebView = new CustomizedWebView
            {
                HostPage = hostPage,
                Services = serviceProvider,
                BackColor = Color.FromArgb(255, 40, 40, 40),
            };
            blazorWebView.WebView.DefaultBackgroundColor = Color.FromArgb(255, 40, 40, 40);
            blazorWebView.WebView.ZoomFactor = Math.Clamp(zoomFactor, 0.1, 4.0);
            blazorWebView.RootComponents.AddRange(rootComponents);
            return new PlatformHandle(blazorWebView.Handle, "HWND");
        }

        return base.CreateNativeControlCore(parent);
    }

    protected override void DestroyNativeControlCore(IPlatformHandle control)
    {
        if (OperatingSystem.IsWindows())
        {
            blazorWebView?.Dispose();
            blazorWebView = null;
        }
        else
        {
            base.DestroyNativeControlCore(control);
        }
    }

    // Helper method to dispose underlying blazor webview. Do not use until dotnet 8 because disposing the webview will deadlock.
    public void OnUnloaded()
    {
        if (OperatingSystem.IsWindows())
        {
            blazorWebView?.Dispose();
            blazorWebView = null;
        }
    }
}
