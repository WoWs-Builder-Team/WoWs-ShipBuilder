using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using WoWsShipBuilder.Core.BuildCreator;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.UI.UserControls;
using WoWsShipBuilder.ViewModels.Base;
using WoWsShipBuilder.ViewModels.ShipVm;

namespace WoWsShipBuilder.UI.ViewModels
{
    internal class ScreenshotContainerViewModel : ViewModelBase
    {
        public ScreenshotContainerViewModel()
            : this(new("Test-build"), DataHelper.LoadPreviewShip(ShipClass.Cruiser, 10, Nation.Usa).Ship, false)
        {
        }

        public ScreenshotContainerViewModel(Build build, Ship ship, bool includeSignals = true)
        {
            CaptainSkillSelectorViewModel = new(ship.ShipClass, ship.ShipNation, true);
            CaptainSkillSelectorViewModel.LoadBuild(build.Skills, build.Captain);
            SignalSelectorViewModel = new();
            SignalSelectorViewModel.LoadBuild(build.Signals);
            ShipModuleViewModel = new(ship.ShipUpgradeInfo);
            ShipModuleViewModel.LoadBuild(build.Modules);
            UpgradePanelViewModel = new(ship);
            UpgradePanelViewModel.LoadBuild(build.Upgrades);
            ConsumableViewModel = new(ship, 0);
            ConsumableViewModel.LoadBuild(build.Consumables);
            BuildName = build.BuildName;
            ShipData = ship;
            IncludeSignals = includeSignals;
            Width = includeSignals ? 1100 : 600;
        }

        public string BuildName { get; }

        public Ship ShipData { get; }

        public bool IncludeSignals { get; }

        public CaptainSkillSelectorViewModel CaptainSkillSelectorViewModel { get; }

        public SignalSelectorViewModel SignalSelectorViewModel { get; }

        public ShipModuleViewModel ShipModuleViewModel { get; }

        public UpgradePanelViewModelBase UpgradePanelViewModel { get; }

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
    }
}
