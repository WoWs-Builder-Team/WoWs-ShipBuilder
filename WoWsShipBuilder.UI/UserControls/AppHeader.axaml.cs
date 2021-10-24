using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Markup.Xaml;

namespace WoWsShipBuilder.UI.UserControls
{
    public partial class AppHeader : UserControl
    {
        private Button minimizeButton;
        private Button maximizeButton;
        private Path maximizeIcon;
        private Button closeButton;

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
            InitializeComponent();

            minimizeButton = this.FindControl<Button>("MinimizeButton");
            maximizeButton = this.FindControl<Button>("MaximizeButton");
            maximizeIcon = this.FindControl<Path>("MaximizeIcon");
            closeButton = this.FindControl<Button>("CloseButton");

            minimizeButton.Click += MinimizeWindow;
            maximizeButton.Click += MaximizeWindow;
            closeButton.Click += CloseWindow;

            SubscribeToWindowState();
        }

        public bool ShowTitle
        {
            get => GetValue(ShowTitleProperty);
            set => SetValue(ShowTitleProperty, value);
        }

        public string Title
        {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public bool ShowMinimizeButton
        {
            get => GetValue(ShowMinimizeButtonProperty);
            set => SetValue(ShowMinimizeButtonProperty, value);
        }     

        public bool ShowMaximizeButton
        {
            get => GetValue(ShowMaximizeButtonProperty);
            set => SetValue(ShowMaximizeButtonProperty, value);
        }

        public bool ShowCloseButton
        {
            get => GetValue(ShowCloseButtonProperty);
            set => SetValue(ShowCloseButtonProperty, value);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void CloseWindow(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Window hostWindow = (Window)VisualRoot!;
            hostWindow.Close();
        }

        private void MaximizeWindow(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Window hostWindow = (Window)VisualRoot!;

            if (hostWindow.WindowState == WindowState.Normal)
            {
                hostWindow.WindowState = WindowState.Maximized;
            }
            else
            {
                hostWindow.WindowState = WindowState.Normal;
            }
        }

        private void MinimizeWindow(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Window hostWindow = (Window)VisualRoot!;
            hostWindow.WindowState = WindowState.Minimized;
        }

        private async void SubscribeToWindowState()
        {
            Window hostWindow = (Window)VisualRoot!;

            while (hostWindow == null)
            {
                hostWindow = (Window)VisualRoot!;
                await Task.Delay(50);
            }

            hostWindow.GetObservable(Window.WindowStateProperty).Subscribe(s =>
            {
                if (s != WindowState.Maximized)
                {
                    maximizeIcon.Data = Avalonia.Media.Geometry.Parse("M2048 2048v-2048h-2048v2048h2048zM1843 1843h-1638v-1638h1638v1638z");
                    hostWindow.Padding = new Thickness(0, 0, 0, 0);
                }

                if (s == WindowState.Maximized)
                {
                    maximizeIcon.Data = Avalonia.Media.Geometry.Parse("M2048 1638h-410v410h-1638v-1638h410v-410h1638v1638zm-614-1024h-1229v1229h1229v-1229zm409-409h-1229v205h1024v1024h205v-1229z");

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
}
