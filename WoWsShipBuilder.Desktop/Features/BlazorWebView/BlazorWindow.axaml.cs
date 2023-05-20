using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;

namespace WoWsShipBuilder.Desktop.Features.BlazorWebView;

public partial class BlazorWindow : Window
{
    public BlazorWindow()
    {
        var services = (Application.Current as App)?.Services;
        var rootComponents = new RootComponentsCollection
        {
            new("#app", typeof(AppRouter), null),
        };

        Resources.Add("services", services);
        Resources.Add("rootComponents", rootComponents);

        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
