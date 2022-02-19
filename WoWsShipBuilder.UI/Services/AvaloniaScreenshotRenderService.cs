using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Newtonsoft.Json;
using WoWsShipBuilder.Core.BuildCreator;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.UI.Utilities;
using WoWsShipBuilder.UI.ViewModels;
using WoWsShipBuilder.UI.Views;

namespace WoWsShipBuilder.UI.Services
{
    public class AvaloniaScreenshotRenderService : IScreenshotRenderService
    {
        public async Task CreateBuildImageAsync(Build build, Ship rawShipData, bool includeSignals, string outputPath, bool copyToClipboard)
        {
            var screenshotWindow = new ScreenshotWindow
            {
                DataContext = new ScreenshotContainerViewModel(build, rawShipData, includeSignals),
            };
            screenshotWindow.Show();

            await using var bitmapData = new MemoryStream();
            using var bitmap = ScreenshotContainerViewModel.RenderScreenshot(screenshotWindow);
            bitmap.Save(bitmapData);
            bitmapData.Seek(0, SeekOrigin.Begin);
            BuildImageProcessor.AddTextToBitmap(bitmapData, JsonConvert.SerializeObject(build), outputPath);
            if (copyToClipboard)
            {
                if (OperatingSystem.IsWindows())
                {
                    using var savedBitmap = new Bitmap(outputPath);
                    await ClipboardHelper.SetBitmapAsync(savedBitmap);
                }
            }

            screenshotWindow.Close();
        }
    }
}
