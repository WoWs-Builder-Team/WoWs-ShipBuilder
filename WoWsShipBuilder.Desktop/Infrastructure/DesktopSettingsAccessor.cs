using System.Globalization;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
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
        settingsFile = dataService.CombinePaths(appDataService.DefaultAppDataDirectory, "settings.json");
    }

    public async Task<AppSettings?> LoadSettings()
    {
        if (fileSystem.File.Exists(settingsFile))
        {
            logger.LogInformation("Trying to load settings from settings file...");
            return await dataService.LoadAsync<AppSettings>(settingsFile);
        }

        return null;
    }

    public AppSettings? LoadSettingsSync()
    {
        if (fileSystem.File.Exists(settingsFile))
        {
            logger.LogInformation("Trying to load settings from settings file...");
            return dataService.Load<AppSettings>(settingsFile);
        }

        return null;
    }

    public async Task SaveSettings(AppSettings appSettings)
    {
        await dataService.StoreAsync(appSettings, settingsFile);
        UpdateThreadCulture(appSettings.SelectedLanguage.CultureInfo);
    }

    public void SaveSettingsSync(AppSettings appSettings)
    {
        dataService.Store(appSettings, settingsFile);
    }

    private static void UpdateThreadCulture(CultureInfo cultureInfo)
    {
        Thread.CurrentThread.CurrentCulture = cultureInfo;
        Thread.CurrentThread.CurrentUICulture = cultureInfo;
    }
}
