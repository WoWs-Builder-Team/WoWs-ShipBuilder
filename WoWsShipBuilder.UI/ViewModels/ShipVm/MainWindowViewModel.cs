using System.Reactive.Linq;
using System.Threading.Tasks;
using Splat;
using WoWsShipBuilder.Core;
using WoWsShipBuilder.Core.BuildCreator;
using WoWsShipBuilder.Core.Data;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilder.Core.Localization;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.UI.Services;
using WoWsShipBuilder.UI.Settings;
using WoWsShipBuilder.ViewModels.Helper;
using WoWsShipBuilder.ViewModels.ShipVm;

namespace WoWsShipBuilder.UI.ViewModels.ShipVm
{
    public class MainWindowViewModel : MainWindowViewModelBase
    {
        private readonly IClipboardService clipboardService;

        private readonly AvaloniaScreenshotRenderService screenshotRenderService;

        public MainWindowViewModel(INavigationService navigationService, IClipboardService clipboardService, IAppDataService appDataService, MainViewModelParams viewModelParams)
            : base(navigationService, appDataService, AppSettingsHelper.LocalizerInstance, AppSettingsHelper.Settings, viewModelParams)
        {
            this.clipboardService = clipboardService;
            screenshotRenderService = new();
        }

        public MainWindowViewModel()
            : this(null!, null!, DesktopAppDataService.PreviewInstance, DataHelper.GetPreviewViewModelParams(ShipClass.Destroyer, 9, Nation.Germany))
        {
        }

        public async void OpenSaveBuild()
        {
            Logging.Logger.Info("Saving build");
            var currentBuild = CreateBuild(CurrentBuildName);
            string shipName = Locator.Current.GetServiceSafe<ILocalizer>().GetGameLocalization(CurrentShipIndex!).Localization;
            var dialogResult = await BuildCreationInteraction.Handle(new(AppSettingsHelper.Settings, currentBuild, shipName)) ?? BuildCreationResult.Canceled;
            if (!dialogResult.Save)
            {
                return;
            }

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
                await clipboardService.SetTextAsync(currentBuild.CreateStringFromBuild());
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
