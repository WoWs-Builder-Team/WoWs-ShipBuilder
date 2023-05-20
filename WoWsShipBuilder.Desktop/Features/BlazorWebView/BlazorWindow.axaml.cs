using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.DependencyInjection;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.Features.Settings;
using WoWsShipBuilder.Infrastructure;

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
        var settings = services!.GetRequiredService<AppSettings>();
        var dataService = services!.GetRequiredService<IDataService>();
        Resources.Add("downloadPath", settings.CustomImagePath ?? dataService.CombinePaths(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), AppConstants.ShipBuilderName));

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
