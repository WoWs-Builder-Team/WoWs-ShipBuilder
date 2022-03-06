using System;
using System.Threading.Tasks;
using NLog;
using ReactiveUI;
using WoWsShipBuilder.Core;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.DataProvider.Updater;
using WoWsShipBuilder.Core.Translations;
using WoWsShipBuilder.ViewModels.Base;

namespace WoWsShipBuilder.ViewModels.Other
{
    public class SplashScreenViewModel : ViewModelBase
    {
        private const int TaskNumber = 4;

        private readonly ILocalDataUpdater localDataUpdater;

        private readonly ILogger logger;

        private double progress;

        public SplashScreenViewModel()
            : this(null!)
        {
        }

        public SplashScreenViewModel(ILocalDataUpdater localDataUpdater)
        {
            this.localDataUpdater = localDataUpdater;
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
                await localDataUpdater.RunDataUpdateCheck(AppData.Settings.SelectedServerType, progressTracker, forceVersionCheck);
                Localizer.Instance.UpdateLanguage(AppData.Settings.SelectedLanguage, forceVersionCheck);
                logger.Debug("Version check and update tasks completed. Launching main window.");
            }
            catch (Exception e)
            {
                logger.Error(e, "Encountered unexpected exception during version check.");
            }

            progressTracker.Report((TaskNumber, Translation.SplashScreen_Done));
        }
    }
}