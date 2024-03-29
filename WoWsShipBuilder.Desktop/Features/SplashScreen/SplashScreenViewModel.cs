using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ReactiveUI;
using WoWsShipBuilder.Desktop.Features.Updater;
using WoWsShipBuilder.Features.Settings;
using WoWsShipBuilder.Infrastructure.ApplicationData;
using WoWsShipBuilder.Infrastructure.Localization;
using WoWsShipBuilder.Infrastructure.Localization.Resources;
using WoWsShipBuilder.Infrastructure.Utility;

namespace WoWsShipBuilder.Desktop.Features.SplashScreen;

public partial class SplashScreenViewModel : ReactiveObject
{
    private const int TaskNumber = 4;

    private readonly ILocalDataUpdater localDataUpdater;

    private readonly ILocalizationProvider localizationProvider;

    private readonly IAppDataService appDataService;

    private readonly AppSettings appSettings;

    private readonly ILogger<SplashScreenViewModel> logger;

    [Observable]
    private int progress;

    [Observable]
    private string downloadInfo = nameof(Translation.SplashScreen_Init);

    public SplashScreenViewModel()
        : this(null!, null!, null!, new(), NullLogger<SplashScreenViewModel>.Instance)
    {
    }

    public SplashScreenViewModel(ILocalDataUpdater localDataUpdater, ILocalizationProvider localizationProvider, IAppDataService appDataService, AppSettings appSettings, ILogger<SplashScreenViewModel> logger)
    {
        this.localDataUpdater = localDataUpdater;
        this.localizationProvider = localizationProvider;
        this.appDataService = appDataService;
        this.appSettings = appSettings;
        this.logger = logger;
    }

    public async Task VersionCheck(bool forceVersionCheck = false, bool throwOnException = false)
    {
        this.logger.LogDebug("Checking gamedata versions...");
        IProgress<(int, string)> progressTracker = new Progress<(int state, string title)>(value =>
        {
            // ReSharper disable once PossibleLossOfFraction
            this.Progress = value.state * 100 / TaskNumber;
            this.DownloadInfo = value.title;
        });

        try
        {
            await this.localDataUpdater.RunDataUpdateCheck(this.appSettings.SelectedServerType, progressTracker, forceVersionCheck);
            await this.localizationProvider.RefreshDataAsync(this.appSettings.SelectedServerType, this.appSettings.SelectedLanguage);
            await this.appDataService.LoadLocalFilesAsync(this.appSettings.SelectedServerType);
            this.logger.LogDebug("Version check and update tasks completed. Launching main window");
        }
        catch (Exception e)
        {
            if (throwOnException)
            {
                this.logger.LogWarning(e, "Encountered unexpected exception during version check");
                throw;
            }

            this.logger.LogError(e, "Encountered unexpected exception during version check");
        }

        progressTracker.Report((TaskNumber, nameof(Translation.SplashScreen_Done)));
    }
}
