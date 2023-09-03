using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using DynamicData;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Web.WebView2.Core;
using Color = System.Drawing.Color;

namespace WoWsShipBuilder.Desktop.Infrastructure.WebView;

[SuppressMessage("Design", "CA1001", Justification = "Disposal happens in OnDetachedFromVisualTree")]
public class BlazorWebView : NativeControlHost
{
    private Uri? source;
    private Microsoft.AspNetCore.Components.WebView.WindowsForms.BlazorWebView? blazorWebView;
    private double zoomFactor = 1.0;
    private string? hostPage;
    private IServiceProvider serviceProvider = default!;
    private RootComponentsCollection rootComponents = new();
    private string defaultDownloadPath = string.Empty;

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

    public static readonly DirectProperty<BlazorWebView, string> DefaultDownloadFolderPathProperty
        = AvaloniaProperty.RegisterDirect<BlazorWebView, string>(
            nameof(DefaultDownloadFolderPath),
            x => x.DefaultDownloadFolderPath,
            (x, y) => x.DefaultDownloadFolderPath = y);

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

    public string DefaultDownloadFolderPath
    {
        get
        {
            if (blazorWebView is not null)
            {
                blazorWebView.WebView.CoreWebView2.Profile.DefaultDownloadFolderPath = defaultDownloadPath;
            }

            return defaultDownloadPath;
        }

        set
        {
            defaultDownloadPath = value;
            if (blazorWebView is not null)
            {
                blazorWebView.WebView.CoreWebView2.Profile.DefaultDownloadFolderPath = value;
            }
        }
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
            blazorWebView.WebView.CoreWebView2InitializationCompleted += WebViewOnCoreWebView2InitializationCompleted;
            blazorWebView.WebView.DefaultBackgroundColor = Color.FromArgb(255, 40, 40, 40);
            blazorWebView.WebView.ZoomFactor = Math.Clamp(zoomFactor, 0.1, 4.0);
            blazorWebView.RootComponents.AddRange(rootComponents);
            return new PlatformHandle(blazorWebView.Handle, "HWND");
        }

        return base.CreateNativeControlCore(parent);
    }

    private void CoreWebView2OnIsDefaultDownloadDialogOpenChanged(object? sender, object e)
    {
        if (blazorWebView?.WebView.CoreWebView2.IsDefaultDownloadDialogOpen == true)
        {
            blazorWebView.WebView.CoreWebView2.CloseDefaultDownloadDialog();
        }
    }

    private void WebViewOnCoreWebView2InitializationCompleted(object? sender, CoreWebView2InitializationCompletedEventArgs e)
    {
        // blazorWebView!.WebView.CoreWebView2.IsDefaultDownloadDialogOpenChanged += CoreWebView2OnIsDefaultDownloadDialogOpenChanged;
        DefaultDownloadFolderPath = defaultDownloadPath;
        blazorWebView!.WebView.CoreWebView2InitializationCompleted -= WebViewOnCoreWebView2InitializationCompleted;
    }

    protected override void DestroyNativeControlCore(IPlatformHandle control)
    {
        if (OperatingSystem.IsWindows())
        {
            // blazorWebView!.WebView.CoreWebView2.IsDefaultDownloadDialogOpenChanged -= CoreWebView2OnIsDefaultDownloadDialogOpenChanged;
            blazorWebView?.Dispose();
            blazorWebView = null;
        }
        else
        {
            base.DestroyNativeControlCore(control);
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        if (OperatingSystem.IsWindows())
        {
            blazorWebView?.Dispose();
            blazorWebView = null;
        }
    }
}
