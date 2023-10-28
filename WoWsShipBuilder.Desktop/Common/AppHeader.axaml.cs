using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace WoWsShipBuilder.Desktop.Common;

public partial class AppHeader : UserControl
{
    public static readonly StyledProperty<bool> ShowTitleProperty =
        AvaloniaProperty.Register<AppHeader, bool>(nameof(ShowTitle), true);

    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<AppHeader, string>(nameof(Title), "WoWs Ship Builder");

    public static readonly StyledProperty<bool> ShowMinimizeButtonProperty =
        AvaloniaProperty.Register<AppHeader, bool>(nameof(ShowMinimizeButton), true);

    public static readonly StyledProperty<bool> ShowMaximizeButtonProperty =
        AvaloniaProperty.Register<AppHeader, bool>(nameof(ShowMaximizeButton), false);

    public static readonly StyledProperty<bool> ShowCloseButtonProperty =
        AvaloniaProperty.Register<AppHeader, bool>(nameof(ShowCloseButton), true);

    public AppHeader()
    {
        this.InitializeComponent();
    }

    public bool ShowTitle
    {
        get => this.GetValue(ShowTitleProperty);
        set => this.SetValue(ShowTitleProperty, value);
    }

    public string Title
    {
        get => this.GetValue(TitleProperty);
        set => this.SetValue(TitleProperty, value);
    }

    public bool ShowMinimizeButton
    {
        get => this.GetValue(ShowMinimizeButtonProperty);
        set => this.SetValue(ShowMinimizeButtonProperty, value);
    }

    public bool ShowMaximizeButton
    {
        get => this.GetValue(ShowMaximizeButtonProperty);
        set => this.SetValue(ShowMaximizeButtonProperty, value);
    }

    public bool ShowCloseButton
    {
        get => this.GetValue(ShowCloseButtonProperty);
        set => this.SetValue(ShowCloseButtonProperty, value);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        this.MinimizeButton.Click += this.MinimizeWindow;
        this.MaximizeButton.Click += this.MaximizeWindow;
        this.CloseButton.Click += this.CloseWindow;

        this.SubscribeToWindowState();
    }

    private void CloseWindow(object? sender, RoutedEventArgs e)
    {
        if (Design.IsDesignMode)
        {
            return;
        }

        Window hostWindow = (Window)this.VisualRoot!;
        hostWindow.Close();
    }

    private void MaximizeWindow(object? sender, RoutedEventArgs e)
    {
        if (Design.IsDesignMode)
        {
            return;
        }

        Window hostWindow = (Window)this.VisualRoot!;

        if (hostWindow.WindowState == WindowState.Normal)
        {
            hostWindow.WindowState = WindowState.Maximized;
        }
        else
        {
            hostWindow.WindowState = WindowState.Normal;
        }
    }

    private void MinimizeWindow(object? sender, RoutedEventArgs e)
    {
        if (Design.IsDesignMode)
        {
            return;
        }

        Window hostWindow = (Window)this.VisualRoot!;
        hostWindow.WindowState = WindowState.Minimized;
    }

    private async void SubscribeToWindowState()
    {
        var hostWindow = (Window?)this.VisualRoot;

        while (hostWindow == null)
        {
            hostWindow = (Window)this.VisualRoot!;
            await Task.Delay(50);
        }

        hostWindow.GetObservable(Window.WindowStateProperty).Subscribe(s =>
        {
            if (s != WindowState.Maximized)
            {
                this.MaximizeIcon.Data = Avalonia.Media.Geometry.Parse("M2048 2048v-2048h-2048v2048h2048zM1843 1843h-1638v-1638h1638v1638z");
                hostWindow.Padding = new Thickness(0, 0, 0, 0);
            }

            if (s == WindowState.Maximized)
            {
                this.MaximizeIcon.Data = Avalonia.Media.Geometry.Parse("M2048 1638h-410v410h-1638v-1638h410v-410h1638v1638zm-614-1024h-1229v1229h1229v-1229zm409-409h-1229v205h1024v1024h205v-1229z");

                // This should be a more universal approach in both cases, but I found it to be less reliable, when for example double-clicking the title bar.
                hostWindow.Padding = new Thickness(
                    hostWindow.OffScreenMargin.Left,
                    hostWindow.OffScreenMargin.Top,
                    hostWindow.OffScreenMargin.Right,
                    hostWindow.OffScreenMargin.Bottom);
            }
        });
    }
}
