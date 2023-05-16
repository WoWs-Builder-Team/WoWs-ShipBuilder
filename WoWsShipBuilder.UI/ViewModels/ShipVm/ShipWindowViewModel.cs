using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WoWsShipBuilder.Common.Builds;
using WoWsShipBuilder.Common.Infrastructure;
using WoWsShipBuilder.Common.Infrastructure.Data;
using WoWsShipBuilder.Common.Infrastructure.Localization;
using WoWsShipBuilder.Common.Infrastructure.Navigation;
using WoWsShipBuilder.Common.ShipStats;
using WoWsShipBuilder.Common.ShipStats.ViewModels;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.UI.Services;
using WoWsShipBuilder.UI.Settings;
using WoWsShipBuilder.ViewModels.Helper;
using AppSettingsHelper = WoWsShipBuilder.UI.Settings.AppSettingsHelper;

namespace WoWsShipBuilder.UI.ViewModels.ShipVm
{
    public class ShipWindowViewModel : ShipViewModel
    {
        private readonly IClipboardService clipboardService;

        private readonly AvaloniaScreenshotRenderService screenshotRenderService;

        private readonly ILocalizer localizer;

        private readonly ILogger<ShipWindowViewModel> logger;

        public ShipWindowViewModel(INavigationService navigationService, IClipboardService clipboardService, ILocalizer localizer, ILogger<ShipWindowViewModel> logger, ShipViewModelParams viewModelParams)
            : base(navigationService, localizer, logger, viewModelParams)
        {
            this.clipboardService = clipboardService;
            this.localizer = localizer;
            screenshotRenderService = new();
            this.logger = logger;
        }

        public async void OpenSaveBuild()
        {
            logger.LogInformation("Saving build");
            string shipName = localizer.GetGameLocalization(CurrentShipIndex).Localization;
            // var dialogResult = await BuildCreationInteraction.Handle(new(AppSettingsHelper.Settings, shipName, CurrentBuildName)) ?? BuildCreationResult.Canceled;
            var dialogResult = BuildCreationResult.Canceled;
            if (!dialogResult.Save)
            {
                return;
            }

            var currentBuild = CreateBuild(dialogResult.BuildName!);
            var oldBuild = AppData.Builds.FirstOrDefault(existingBuild => existingBuild.BuildName.Equals(currentBuild.BuildName) && existingBuild.ShipIndex.Equals(currentBuild.ShipIndex));
            if (oldBuild != null)
            {
                logger.LogInformation("Removing old build with identical name from list of saved builds to replace with new build");
                AppData.Builds.Remove(oldBuild);
            }

            AppData.Builds.Insert(0, currentBuild);

            AppSettingsHelper.Settings.IncludeSignalsForImageExport = dialogResult.IncludeSignals;
            CurrentBuildName = currentBuild.BuildName;
            await CreateBuildImage(currentBuild, dialogResult.IncludeSignals, dialogResult.CopyImageToClipboard);

            string infoBoxContent;
            if (dialogResult.CopyImageToClipboard)
            {
                infoBoxContent = Translation.BuildCreationWindow_SavedImageToClipboard;
            }
            else
            {
                await clipboardService.SetTextAsync(currentBuild.CreateShortStringFromBuild());
                infoBoxContent = Translation.BuildCreationWindow_SavedClipboard;
            }

            await BuildCreatedInteraction.Handle(infoBoxContent);
        }

        private async Task CreateBuildImage(Build currentBuild, bool includeSignals, bool copyToClipboard)
        {
            await screenshotRenderService.CreateBuildImageAsync(currentBuild, RawShipData, includeSignals, copyToClipboard);
        }
    }
}
