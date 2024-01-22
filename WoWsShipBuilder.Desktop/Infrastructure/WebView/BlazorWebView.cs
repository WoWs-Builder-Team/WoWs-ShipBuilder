using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
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
            if (this.blazorWebView != null)
            {
                this.hostPage = this.blazorWebView.HostPage;
            }

            return this.hostPage;
        }

        set
        {
            if (this.hostPage != value)
            {
                this.hostPage = value;
                if (this.blazorWebView != null)
                {
                    this.blazorWebView.HostPage = value;
                }
            }
        }
    }

    public Uri? Source
    {
        get
        {
            if (this.blazorWebView != null)
            {
                this.source = this.blazorWebView.WebView.Source;
            }

            return this.source;
        }

        set
        {
            if (this.source != value)
            {
                this.source = value;
                if (this.blazorWebView != null)
                {
                    this.blazorWebView.WebView.Source = value;
                }
            }
        }
    }

    public double ZoomFactor
    {
        get
        {
            if (this.blazorWebView != null)
            {
                this.zoomFactor = this.blazorWebView.WebView.ZoomFactor;
            }

            return this.zoomFactor;
        }

        set
        {
            if (this.zoomFactor != value)
            {
                this.zoomFactor = value;
                if (this.blazorWebView != null)
                {
                    this.blazorWebView.WebView.ZoomFactor = value;
                }
            }
        }
    }

    public IServiceProvider Services
    {
        get => this.serviceProvider;
        set
        {
            this.serviceProvider = value;
            if (this.blazorWebView != null)
            {
                this.blazorWebView.Services = this.serviceProvider;
            }
        }
    }

    public RootComponentsCollection RootComponents
    {
        get => this.rootComponents;
        set => this.rootComponents = value;
    }

    public string DefaultDownloadFolderPath
    {
        get
        {
            if (this.blazorWebView is not null)
            {
                this.blazorWebView.WebView.CoreWebView2.Profile.DefaultDownloadFolderPath = this.defaultDownloadPath;
            }

            return this.defaultDownloadPath;
        }

        set
        {
            this.defaultDownloadPath = value;
            if (this.blazorWebView is not null)
            {
                this.blazorWebView.WebView.CoreWebView2.Profile.DefaultDownloadFolderPath = value;
            }
        }
    }

    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        if (OperatingSystem.IsWindows())
        {
            this.blazorWebView = new CustomizedWebView
            {
                HostPage = this.hostPage,
                Services = this.serviceProvider,
                BackColor = Color.FromArgb(255, 40, 40, 40),
            };
            this.blazorWebView.WebView.CoreWebView2InitializationCompleted += this.WebViewOnCoreWebView2InitializationCompleted;
            this.blazorWebView.WebView.DefaultBackgroundColor = Color.FromArgb(255, 40, 40, 40);
            this.blazorWebView.WebView.ZoomFactor = Math.Clamp(this.zoomFactor, 0.1, 4.0);
            this.blazorWebView.RootComponents.AddRange(this.rootComponents);
            return new PlatformHandle(this.blazorWebView.Handle, "HWND");
        }

        return base.CreateNativeControlCore(parent);
    }

    private void WebViewOnCoreWebView2InitializationCompleted(object? sender, CoreWebView2InitializationCompletedEventArgs e)
    {
        this.DefaultDownloadFolderPath = this.defaultDownloadPath;
        this.blazorWebView!.WebView.CoreWebView2InitializationCompleted -= this.WebViewOnCoreWebView2InitializationCompleted;
    }

    protected override void DestroyNativeControlCore(IPlatformHandle control)
    {
        if (OperatingSystem.IsWindows())
        {
            this.blazorWebView?.Dispose();
            this.blazorWebView = null;
        }
        else
        {
            base.DestroyNativeControlCore(control);
        }
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        if (OperatingSystem.IsWindows())
        {
            this.blazorWebView?.Dispose();
            this.blazorWebView = null;
        }

        base.OnUnloaded(e);
    }
}
