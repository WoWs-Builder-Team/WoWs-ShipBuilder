using System;
using System.IO;
using System.Reactive.Disposables;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Microsoft.Extensions.Logging;
using WoWsShipBuilder.Common.Infrastructure;
using WoWsShipBuilder.Core;

namespace WoWsShipBuilder.UI.Utilities
{
    [SupportedOSPlatform("windows")]
    internal static class ClipboardHelper
    {
        private const int OleRetryCount = 10;
        private const byte BitmapClipboardFormat = 2;

        public static async Task SetBitmapAsync(IBitmap bitmap)
        {
            try
            {
                await SetBitmapUnsafeAsync(bitmap);
            }
            catch (Exception e)
            {
                Logging.Logger.LogError(e, "Error while writing image to clipboard");
            }
        }

        // based on https://github.com/AvaloniaUI/Avalonia/issues/3588
        private static async Task SetBitmapUnsafeAsync(IBitmap bitmap)
        {
            // Convert from Avalonia Bitmap to System Bitmap
            var memoryStream = new MemoryStream(1000000);
            bitmap.Save(memoryStream); // this returns a png from Skia (we could save/load it from the system bitmap to convert it to a bmp first, but this seems to work well already)

            var systemBitmap = new System.Drawing.Bitmap(memoryStream);
            var hBitmap = systemBitmap.GetHbitmap();

            var screenDc = GetDC(IntPtr.Zero);
            var sourceDc = CreateCompatibleDC(screenDc);
            SelectObject(sourceDc, hBitmap);

            var destDc = CreateCompatibleDC(screenDc);
            var compatibleBitmap = CreateCompatibleBitmap(screenDc, systemBitmap.Width, systemBitmap.Height);
            SelectObject(destDc, compatibleBitmap);

            BitBlt(destDc, 0, 0, systemBitmap.Width, systemBitmap.Height, sourceDc, 0, 0, 0x00CC0020); // SRCCOPY

            try
            {
                using (await OpenClipboard())
                {
                    EmptyClipboard();
                    SetClipboardData(BitmapClipboardFormat, compatibleBitmap);
                }
            }
            catch (Exception e)
            {
                Logging.Logger.LogError(e, "Error while accessing clipboard");
            }
        }

        private static async Task<IDisposable> OpenClipboard()
        {
            int remainingAttempts = OleRetryCount;
            while (!OpenClipboard(IntPtr.Zero))
            {
                if (--remainingAttempts == 0)
                {
                    throw new TimeoutException("Timeout during clipboard opening.");
                }

                await Task.Delay(100);
            }

            return Disposable.Create(() => CloseClipboard());
        }

        [DllImport("gdi32.dll", ExactSpelling = true)]
        private static extern IntPtr CreateCompatibleDC(IntPtr hDc);

        [DllImport("gdi32.dll", ExactSpelling = true)]
        private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int cx, int cy);

        [DllImport("gdi32.dll", SetLastError = true, ExactSpelling = true)]
        private static extern IntPtr SelectObject(IntPtr hdc, IntPtr h);

        [DllImport("gdi32.dll", SetLastError = true, ExactSpelling = true)]
        private static extern bool BitBlt(IntPtr hdc, int x, int y, int cx, int cy, IntPtr hdcSrc, int x1, int y1, uint rop);

        [DllImport("user32.dll", ExactSpelling = true)]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool CloseClipboard();

        [DllImport("user32.dll")]
        private static extern bool SetClipboardData(uint uFormat, IntPtr data);

        [DllImport("user32.dll")]
        private static extern bool EmptyClipboard();
    }
}
