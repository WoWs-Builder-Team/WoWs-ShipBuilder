using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using WoWsShipBuilder.Core.BuildCreator;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.UI.UserControls;
using WoWsShipBuilder.UI.ViewModels.ShipVm;
using WoWsShipBuilder.ViewModels.Base;
using WoWsShipBuilder.ViewModels.ShipVm;

namespace WoWsShipBuilder.UI.ViewModels
{
    internal class ScreenshotContainerViewModel : ViewModelBase
    {
        public ScreenshotContainerViewModel()
            : this(new("Test-build"), DataHelper.LoadPreviewShip(ShipClass.Cruiser, 10, Nation.Usa).Ship, false)
        {
            var ship = DataHelper.LoadPreviewShip(ShipClass.Cruiser, 10, Nation.Usa).Ship;
            var appDataService = DesktopAppDataService.PreviewInstance;

            CaptainSkillSelectorViewModel = new(ship.ShipClass, CaptainSkillSelectorViewModel.LoadParamsAsync(appDataService, ship.ShipNation).Result, true);
            SignalSelectorViewModel = new(SignalSelectorViewModel.LoadSignalList(appDataService).Result);
            UpgradePanelViewModel = new UpgradePanelViewModel(ship, UpgradePanelViewModelBase.LoadParamsAsync(appDataService).Result);
            LoadBuilds(new("Test-build"));
        }

        private ScreenshotContainerViewModel(Build build, Ship ship, bool includeSignals = true)
        {
            CaptainSkillSelectorViewModel = null!;
            SignalSelectorViewModel = null!;
            ShipModuleViewModel = new(ship.ShipUpgradeInfo);

            UpgradePanelViewModel = null!;
            ConsumableViewModel = new(ship);
            BuildName = build.BuildName;
            ShipData = ship;
            IncludeSignals = includeSignals;
            Width = includeSignals ? 1100 : 600;
        }

        public string BuildName { get; }

        public Ship ShipData { get; }

        public bool IncludeSignals { get; }

        public CaptainSkillSelectorViewModel CaptainSkillSelectorViewModel { get; private init; }

        public SignalSelectorViewModel SignalSelectorViewModel { get; private init; }

        public ShipModuleViewModel ShipModuleViewModel { get; }

        public UpgradePanelViewModelBase UpgradePanelViewModel { get; private init; }

        public ConsumableViewModel ConsumableViewModel { get; }

        public int Width { get; }

        public string EffectiveBuildName
        {
            get
            {
                int separatorIndex = BuildName.LastIndexOf(" - ", StringComparison.Ordinal);
                return separatorIndex > -1 ? BuildName[..BuildName.LastIndexOf(" - ", StringComparison.Ordinal)] : BuildName;
            }
        }

        public static async Task<ScreenshotContainerViewModel> CreateAsync(IAppDataService appDataService, Build build, Ship ship, bool includeSignals = true)
        {
            var vm = new ScreenshotContainerViewModel(build, ship, includeSignals)
            {
                CaptainSkillSelectorViewModel = new(ship.ShipClass, await CaptainSkillSelectorViewModel.LoadParamsAsync(appDataService, ship.ShipNation), true),
                SignalSelectorViewModel = new(await SignalSelectorViewModel.LoadSignalList(appDataService)),
                UpgradePanelViewModel = new UpgradePanelViewModel(ship, await UpgradePanelViewModelBase.LoadParamsAsync(appDataService)),
            };
            vm.LoadBuilds(build);
            return vm;
        }

        internal static Bitmap RenderScreenshot(Window window)
        {
            var screenshotContainer = window.FindControl<ScreenshotContainer>("ScreenshotContainer");
            var pixelSize = new PixelSize((int)screenshotContainer.Width, (int)screenshotContainer.Height);
            var bitmap = new RenderTargetBitmap(pixelSize);
            var size = pixelSize.ToSize(1);
            screenshotContainer.Measure(size);
            screenshotContainer.Arrange(new(size));
            bitmap.Render(screenshotContainer);

            return bitmap;
        }

        public static void RenderScreenshot(Window window, string outputFileName)
        {
            using var bitmap = RenderScreenshot(window);
            bitmap.Save(outputFileName);
        }

        private void LoadBuilds(Build build)
        {
            CaptainSkillSelectorViewModel.LoadBuild(build.Skills, build.Captain);
            SignalSelectorViewModel.LoadBuild(build.Signals);
            ShipModuleViewModel.LoadBuild(build.Modules);
            ConsumableViewModel.LoadBuild(build.Consumables);
            UpgradePanelViewModel.LoadBuild(build.Upgrades);
        }
    }
}
