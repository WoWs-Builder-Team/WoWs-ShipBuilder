using System.Globalization;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using WoWsShipBuilder.Desktop.Infrastructure.Data;
using WoWsShipBuilder.Features.Settings;
using WoWsShipBuilder.Infrastructure.ApplicationData;

namespace WoWsShipBuilder.Desktop.Infrastructure;

public class DesktopSettingsAccessor : ISettingsAccessor
{
    private readonly IDataService dataService;

    private readonly IFileSystem fileSystem;

    private readonly ILogger<DesktopSettingsAccessor> logger;

    private readonly string settingsFile;

    public DesktopSettingsAccessor(IAppDataService appDataService, IDataService dataService, IFileSystem fileSystem, ILogger<DesktopSettingsAccessor> logger)
    {
        this.dataService = dataService;
        this.fileSystem = fileSystem;
        this.logger = logger;
        this.settingsFile = dataService.CombinePaths(appDataService.DefaultAppDataDirectory, "settings.json");
    }

    public async Task<AppSettings?> LoadSettings()
    {
        if (this.fileSystem.File.Exists(this.settingsFile))
        {
            this.logger.LogInformation("Trying to load settings from settings file...");
            return await this.dataService.LoadAsync<AppSettings>(this.settingsFile);
        }

        return null;
    }

    public AppSettings? LoadSettingsSync()
    {
        if (this.fileSystem.File.Exists(this.settingsFile))
        {
            this.logger.LogInformation("Trying to load settings from settings file...");
            return this.dataService.Load<AppSettings>(this.settingsFile);
        }

        return null;
    }

    public async Task SaveSettings(AppSettings appSettings)
    {
        await this.dataService.StoreAsync(appSettings, this.settingsFile);
        await UpdateUiThreadCultureAsync(appSettings.SelectedLanguage.CultureInfo);
    }

    public void SaveSettingsSync(AppSettings appSettings)
    {
        this.dataService.Store(appSettings, this.settingsFile);
    }

    private static async Task UpdateUiThreadCultureAsync(CultureInfo cultureInfo)
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;
        });
    }
}
