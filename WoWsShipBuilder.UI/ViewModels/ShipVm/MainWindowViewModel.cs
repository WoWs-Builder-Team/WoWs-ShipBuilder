using System.Reactive.Linq;
using System.Threading.Tasks;
using WoWsShipBuilder.Core;
using WoWsShipBuilder.Core.BuildCreator;
using WoWsShipBuilder.Core.Data;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.Core.Translations;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.UI.Services;
using WoWsShipBuilder.ViewModels.Helper;
using WoWsShipBuilder.ViewModels.ShipVm;

namespace WoWsShipBuilder.UI.ViewModels.ShipVm
{
    public class MainWindowViewModel : MainWindowViewModelBase
    {
        private readonly IClipboardService clipboardService;

        private readonly AvaloniaScreenshotRenderService screenshotRenderService;

        public MainWindowViewModel(INavigationService navigationService, IClipboardService clipboardService, IAppDataService appDataService, MainViewModelParams viewModelParams)
            : base(navigationService, appDataService, viewModelParams)
        {
            this.clipboardService = clipboardService;
            screenshotRenderService = new();
        }

        public MainWindowViewModel()
            : this(null!, null!, DesktopAppDataService.PreviewInstance, DataHelper.GetPreviewViewModelParams(ShipClass.Destroyer, 9, Nation.Germany))
        {
        }

        public override async void OpenSaveBuild()
        {
            Logging.Logger.Info("Saving build");
            var currentBuild = new Build(CurrentShipIndex!, RawShipData.ShipNation, ShipModuleViewModel.SaveBuild(), UpgradePanelViewModel.SaveBuild(), ConsumableViewModel.SaveBuild(), CaptainSkillSelectorViewModel!.GetCaptainIndex(), CaptainSkillSelectorViewModel!.GetSkillNumberList(), SignalSelectorViewModel!.GetFlagList());
            if (CurrentBuildName != null)
            {
                currentBuild.BuildName = CurrentBuildName;
            }

            string shipName = Localizer.Instance[CurrentShipIndex!].Localization;
            var dialogResult = await BuildCreationInteraction.Handle(new(currentBuild, shipName)) ?? BuildCreationResult.Canceled;
            if (!dialogResult.Save)
            {
                return;
            }

            AppData.Settings.IncludeSignalsForImageExport = dialogResult.IncludeSignals;
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
