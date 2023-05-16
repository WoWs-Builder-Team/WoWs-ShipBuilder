using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WoWsShipBuilder.Desktop.Services;
using WoWsShipBuilder.Desktop.Settings;
using WoWsShipBuilder.Desktop.ViewModels.Helper;
using WoWsShipBuilder.Features.Builds;
using WoWsShipBuilder.Features.ShipStats;
using WoWsShipBuilder.Infrastructure;
using WoWsShipBuilder.Infrastructure.Data;
using WoWsShipBuilder.Infrastructure.Localization;
using WoWsShipBuilder.Infrastructure.Localization.Resources;
using ShipViewModel = WoWsShipBuilder.Features.ShipStats.ViewModels.ShipViewModel;

namespace WoWsShipBuilder.Desktop.ViewModels.ShipVm
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
