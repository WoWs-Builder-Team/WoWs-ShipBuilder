using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using WoWsShipBuilder.Core;
using WoWsShipBuilder.Core.Builds;
using WoWsShipBuilder.Core.Data;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Localization;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.UI.Services;
using WoWsShipBuilder.UI.Settings;
using WoWsShipBuilder.ViewModels.Helper;
using WoWsShipBuilder.ViewModels.ShipVm;

namespace WoWsShipBuilder.UI.ViewModels.ShipVm
{
    public class ShipWindowViewModel : ShipViewModelBase
    {
        private readonly IClipboardService clipboardService;

        private readonly AvaloniaScreenshotRenderService screenshotRenderService;

        private readonly ILocalizer localizer;

        public ShipWindowViewModel(INavigationService navigationService, IClipboardService clipboardService, ILocalizer localizer, ShipViewModelParams viewModelParams)
            : base(navigationService, localizer, viewModelParams)
        {
            this.clipboardService = clipboardService;
            this.localizer = localizer;
            screenshotRenderService = new();
        }

        public async void OpenSaveBuild()
        {
            Logging.Logger.Info("Saving build");
            string shipName = localizer.GetGameLocalization(CurrentShipIndex).Localization;
            var dialogResult = await BuildCreationInteraction.Handle(new(AppSettingsHelper.Settings, shipName, CurrentBuildName)) ?? BuildCreationResult.Canceled;
            if (!dialogResult.Save)
            {
                return;
            }

            var currentBuild = CreateBuild(dialogResult.BuildName!);
            var oldBuild = AppData.Builds.FirstOrDefault(existingBuild => existingBuild.BuildName.Equals(currentBuild.BuildName) && existingBuild.ShipIndex.Equals(currentBuild.ShipIndex));
            if (oldBuild != null)
            {
                Logging.Logger.Info("Removing old build with identical name from list of saved builds to replace with new build.");
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
