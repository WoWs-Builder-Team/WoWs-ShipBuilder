using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Newtonsoft.Json;
using Splat;
using WoWsShipBuilder.Core.Builds;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilder.Core.Localization;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.UI.Settings;
using WoWsShipBuilder.UI.Utilities;
using WoWsShipBuilder.UI.ViewModels;
using WoWsShipBuilder.UI.Views;

namespace WoWsShipBuilder.UI.Services
{
    public class AvaloniaScreenshotRenderService
    {
        public async Task CreateBuildImageAsync(Build build, Ship rawShipData, bool includeSignals, bool copyToClipboard)
        {
            string outputPath = DesktopAppDataService.Instance.GetImageOutputPath(build.BuildName, Locator.Current.GetServiceSafe<ILocalizer>().GetGameLocalization(build.ShipIndex).Localization);
            var screenshotWindow = new ScreenshotWindow
            {
                DataContext = ScreenshotContainerViewModel.Create(Locator.Current.GetServiceSafe<IAppDataService>(), build, rawShipData, includeSignals),
            };
            screenshotWindow.Show();

            await using var bitmapData = new MemoryStream();
            using var bitmap = ScreenshotContainerViewModel.RenderScreenshot(screenshotWindow);
            bitmap.Save(bitmapData);
            bitmapData.Seek(0, SeekOrigin.Begin);
            BuildImageProcessor.AddTextToBitmap(bitmapData, JsonConvert.SerializeObject(build), outputPath);
            if (copyToClipboard && OperatingSystem.IsWindows())
            {
                using var savedBitmap = new Bitmap(outputPath);
                await ClipboardHelper.SetBitmapAsync(savedBitmap);
            }

            screenshotWindow.Close();
            OpenExplorerForFile(outputPath);
        }

        private static void OpenExplorerForFile(string filePath)
        {
            if (AppSettingsHelper.Settings.OpenExplorerAfterImageSave)
            {
                Process.Start("explorer.exe", $"/select, \"{filePath}\"");
            }
        }
    }
}
