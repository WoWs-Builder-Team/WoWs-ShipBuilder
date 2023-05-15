using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using WoWsShipBuilder.Common.Builds;
using WoWsShipBuilder.Common.Infrastructure;
using WoWsShipBuilder.Common.Infrastructure.Data;
using WoWsShipBuilder.Common.ShipStats.ViewModels;
using WoWsShipBuilder.Core;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.UI.UserControls;
using WoWsShipBuilder.UI.ViewModels.ShipVm;
using WoWsShipBuilder.ViewModels.Base;
using WoWsShipBuilder.ViewModels.ShipVm;

namespace WoWsShipBuilder.UI.ViewModels
{
    internal class ScreenshotContainerViewModel : ViewModelBase
    {
        public ScreenshotContainerViewModel()
            : this(DesignDataHelper.CreateTestBuild(), DesignDataHelper.LoadPreviewShip(ShipClass.Cruiser, 10, Nation.Usa).Ship, false)
        {
            var ship = DesignDataHelper.LoadPreviewShip(ShipClass.Cruiser, 10, Nation.Usa).Ship;

            CaptainSkillSelectorViewModel = new(ship.ShipClass, CaptainSkillSelectorViewModel.LoadParams(ship.ShipNation), true);
            SignalSelectorViewModel = new();
            UpgradePanelViewModel = new UpgradePanelViewModel(ship, AppData.ModernizationCache);
            LoadBuilds(DesignDataHelper.CreateTestBuild());
        }

        private ScreenshotContainerViewModel(Build build, Ship ship, bool includeSignals = true)
        {
            CaptainSkillSelectorViewModel = null!;
            SignalSelectorViewModel = null!;
            ConsumableViewModel = null!;
            ShipModuleViewModel = new(ship.ShipUpgradeInfo);

            UpgradePanelViewModel = null!;
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

        public ConsumableViewModel ConsumableViewModel { get; private init; }

        public int Width { get; }

        public string EffectiveBuildName
        {
            get
            {
                int separatorIndex = BuildName.LastIndexOf(" - ", StringComparison.Ordinal);
                return separatorIndex > -1 ? BuildName[..BuildName.LastIndexOf(" - ", StringComparison.Ordinal)] : BuildName;
            }
        }

        public static ScreenshotContainerViewModel Create(Build build, Ship ship, bool includeSignals = true)
        {
            var vm = new ScreenshotContainerViewModel(build, ship, includeSignals)
            {
                CaptainSkillSelectorViewModel = new(ship.ShipClass, CaptainSkillSelectorViewModel.LoadParams(ship.ShipNation), true),
                SignalSelectorViewModel = new(),
                UpgradePanelViewModel = new UpgradePanelViewModel(ship, AppData.ModernizationCache),
                ConsumableViewModel = ConsumableViewModel.Create(ship, new List<string>(), Logging.LoggerFactory),
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
