﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Newtonsoft.Json;
using WoWsShipBuilder.Core.BuildCreator;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.UI.Utilities;
using WoWsShipBuilder.UI.ViewModels;
using WoWsShipBuilder.UI.Views;

namespace WoWsShipBuilder.UI.Services
{
    public class AvaloniaScreenshotRenderService : IScreenshotRenderService
    {
        public async Task CreateBuildImageAsync(Build build, Ship rawShipData, bool includeSignals, bool copyToClipboard)
        {
            string outputPath = DesktopAppDataService.Instance.GetImageOutputPath(build.BuildName, Localizer.Instance[build.ShipIndex].Localization);
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
            OpenExplorerForFile(outputPath);
        }

        private static void OpenExplorerForFile(string filePath)
        {
            if (AppData.Settings.OpenExplorerAfterImageSave)
            {
                Process.Start("explorer.exe", $"/select, \"{filePath}\"");
            }
        }
    }
}
