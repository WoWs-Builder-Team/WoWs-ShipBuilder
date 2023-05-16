using System;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Newtonsoft.Json;
using Splat;
using WoWsShipBuilder.Common.Features.Builds;
using WoWsShipBuilder.Common.Infrastructure.Localization;
using WoWsShipBuilder.Core.Localization;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.UI.Extensions;
using WoWsShipBuilder.UI.Settings;
using WoWsShipBuilder.UI.Utilities;
using WoWsShipBuilder.UI.ViewModels;
using WoWsShipBuilder.UI.Views;

namespace WoWsShipBuilder.UI.Services;

public class AvaloniaScreenshotRenderService
{
    private readonly ILocalizer localizer;

    private readonly IFileSystem fileSystem;

    public AvaloniaScreenshotRenderService()
    {
        localizer = Locator.Current.GetRequiredService<ILocalizer>();
        fileSystem = Locator.Current.GetRequiredService<IFileSystem>();
    }

    public async Task CreateBuildImageAsync(Build build, Ship rawShipData, bool includeSignals, bool copyToClipboard)
    {
        string outputPath = GetImageOutputPath(build.BuildName, localizer.GetGameLocalization(build.ShipIndex).Localization);
        var screenshotWindow = new ScreenshotWindow
        {
            DataContext = ScreenshotContainerViewModel.Create(build, rawShipData, includeSignals),
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

    private string GetImageOutputPath(string buildName, string shipName)
    {
        string directory = AppSettingsHelper.BuildImageOutputDirectory;
        fileSystem.Directory.CreateDirectory(directory);
        return fileSystem.Path.Combine(directory, shipName + " - " + buildName + ".png");
    }
}
