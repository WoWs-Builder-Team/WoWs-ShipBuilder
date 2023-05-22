using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using WoWsShipBuilder.Desktop.Infrastructure.Data;
using WoWsShipBuilder.Infrastructure.Data;

namespace WoWsShipBuilder.Desktop.Infrastructure.WebView;

public class CustomizedWebView : Microsoft.AspNetCore.Components.WebView.WindowsForms.BlazorWebView
{
    public override IFileProvider CreateFileProvider(string contentRootDir)
    {
        var appDataService = Services.GetRequiredService<IAppDataService>();
        var dataService = Services.GetRequiredService<IDataService>();
        return new CompositeFileProvider(new PhysicalFileProvider(dataService.CombinePaths(appDataService.AppDataImageDirectory, "Ships")), base.CreateFileProvider(contentRootDir));
    }
}
