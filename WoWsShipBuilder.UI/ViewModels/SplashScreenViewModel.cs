using System;
using System.IO.Abstractions;
using System.Threading.Tasks;
using ReactiveUI;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.DataProvider.Updater;
using WoWsShipBuilder.Core.HttpClients;

namespace WoWsShipBuilder.UI.ViewModels
{
    public class SplashScreenViewModel : ViewModelBase
    {
        private const int TaskNumber = 4;

        private readonly IAwsClient awsClient;

        private readonly IFileSystem fileSystem;

        private double progress;

        public SplashScreenViewModel()
            : this(AwsClient.Instance, new FileSystem())
        {
        }

        public SplashScreenViewModel(IAwsClient awsClient, IFileSystem fileSystem)
        {
            this.awsClient = awsClient;
            this.fileSystem = fileSystem;
        }

        public double Progress
        {
            get => progress;
            set => this.RaiseAndSetIfChanged(ref progress, value);
        }

        private string downloadInfo = "SplashScreen_Init";

        public string DownloadInfo
        {
            get => downloadInfo;
            set => this.RaiseAndSetIfChanged(ref downloadInfo, value);
        }

        public async Task VersionCheck(bool forceVersionCheck = false)
        {
            IProgress<(int, string)> progressTracker = new Progress<(int state, string title)>(value =>
            {
                // ReSharper disable once PossibleLossOfFraction
                Progress = value.state * 100 / TaskNumber;
                DownloadInfo = value.title;
            });

            var updater = new LocalDataUpdater(fileSystem, awsClient, AppDataHelper.Instance);
            await updater.RunDataUpdateCheck(AppData.Settings.SelectedServerType, progressTracker, forceVersionCheck);
            Localizer.Instance.UpdateLanguage(AppData.Settings.SelectedLanguage, forceVersionCheck);

            progressTracker.Report((TaskNumber, "SplashScreen_Done"));
        }
    }
}
