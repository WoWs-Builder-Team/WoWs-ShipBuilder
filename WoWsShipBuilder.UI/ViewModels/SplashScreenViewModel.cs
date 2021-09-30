using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.HttpClients;
using WoWsShipBuilder.Core.HttpResponses;

namespace WoWsShipBuilder.UI.ViewModels
{
    public class SplashScreenViewModel : ViewModelBase
    {
        private const int TaskNumber = 4;

        private double progress;

        public double Progress
        {
            get => progress;
            set => this.RaiseAndSetIfChanged(ref progress, value);
        }

        private string downloadInfo = "placeholder";

        public string DownloadInfo
        {
            get => downloadInfo;
            set => this.RaiseAndSetIfChanged(ref downloadInfo, value);
        }

        public async Task VersionCheck()
        {
            IProgress<(int, string)> progressTracker = new Progress<(int state, string title)>(value =>
            {
                // ReSharper disable once PossibleLossOfFraction
                Progress = value.state * 100 / TaskNumber;
                DownloadInfo = value.title;
            });

            await JsonVersionCheck(progressTracker);
            await DownloadImages(progressTracker);
            progressTracker.Report((TaskNumber, "done"));
        }

        private async Task JsonVersionCheck(IProgress<(int, string)> progressTracker)
        {
            progressTracker.Report((1, "jsonData"));
            await AwsClient.Instance.CheckFileVersion(ServerType.Live, AppDataHelper.Instance);
        }

        private async Task DownloadImages(IProgress<(int, string)> progressTracker)
        {
            string imageBasePath = Path.Combine(AppDataHelper.Instance.AppDataDirectory, "Images");
            var shipImageDirectory = new DirectoryInfo(Path.Combine(imageBasePath, "Ships"));
            if (!shipImageDirectory.Exists || !shipImageDirectory.GetFiles().Any())
            {
                progressTracker.Report((2, "shipImages"));
                await AwsClient.Instance.DownloadAllImages(ImageType.Ship, AppDataHelper.Instance);
            }

            var camoImageDirectory = new DirectoryInfo(Path.Combine(imageBasePath, "Camos"));
            if (!camoImageDirectory.Exists || !camoImageDirectory.GetFiles().Any())
            {
                progressTracker.Report((3, "camoImages"));
                await AwsClient.Instance.DownloadAllImages(ImageType.Camo, AppDataHelper.Instance);
            }
        }
    }
}
