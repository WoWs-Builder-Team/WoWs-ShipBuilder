using System;
using System.Threading.Tasks;
using NLog;
using ReactiveUI;
using WoWsShipBuilder.Core;
using WoWsShipBuilder.Core.DataProvider.Updater;
using WoWsShipBuilder.Core.Localization;
using WoWsShipBuilder.Core.Settings;
using WoWsShipBuilder.Core.Translations;
using WoWsShipBuilder.ViewModels.Base;

namespace WoWsShipBuilder.ViewModels.Other;

public class SplashScreenViewModel : ViewModelBase
{
    private const int TaskNumber = 4;

    private readonly ILocalDataUpdater localDataUpdater;

    private readonly ILocalizationProvider localizationProvider;

    private readonly AppSettings appSettings;

    private readonly ILogger logger;

    private double progress;

    public SplashScreenViewModel()
        : this(null!, null!, new())
    {
    }

    public SplashScreenViewModel(ILocalDataUpdater localDataUpdater, ILocalizationProvider localizationProvider, AppSettings appSettings)
    {
        this.localDataUpdater = localDataUpdater;
        this.localizationProvider = localizationProvider;
        this.appSettings = appSettings;
        logger = Logging.GetLogger("SplashScreen");
    }

    public double Progress
    {
        get => progress;
        set => this.RaiseAndSetIfChanged(ref progress, value);
    }

    private string downloadInfo = Translation.SplashScreen_Init;

    public string DownloadInfo
    {
        get => downloadInfo;
        set => this.RaiseAndSetIfChanged(ref downloadInfo, value);
    }

    public async Task VersionCheck(bool forceVersionCheck = false)
    {
        logger.Debug("Checking gamedata versions...");
        IProgress<(int, string)> progressTracker = new Progress<(int state, string title)>(value =>
        {
            // ReSharper disable once PossibleLossOfFraction
            Progress = value.state * 100 / TaskNumber;
            DownloadInfo = value.title;
        });

        try
        {
            await localDataUpdater.RunDataUpdateCheck(appSettings.SelectedServerType, progressTracker, forceVersionCheck);
            await localizationProvider.RefreshDataAsync(appSettings.SelectedLanguage);
            logger.Debug("Version check and update tasks completed. Launching main window.");
        }
        catch (Exception e)
        {
            logger.Error(e, "Encountered unexpected exception during version check.");
        }

        progressTracker.Report((TaskNumber, Translation.SplashScreen_Done));
    }
}
