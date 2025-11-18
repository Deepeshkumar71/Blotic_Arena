using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BloticArena.Services
{
    public static class IconExtractor
    {
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr ExtractIcon(IntPtr hInst, string lpszExeFileName, int nIconIndex);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);

        public static ImageSource? ExtractIconFromFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return null;

            try
            {
                // Extract icon from executable
                IntPtr hIcon = ExtractIcon(IntPtr.Zero, filePath, 0);
                
                if (hIcon == IntPtr.Zero || hIcon == new IntPtr(1))
                    return null;

                try
                {
                    // Convert to WPF ImageSource
                    using (Icon icon = Icon.FromHandle(hIcon))
                    {
                        using (Bitmap bitmap = icon.ToBitmap())
                        {
                            IntPtr hBitmap = bitmap.GetHbitmap();
                            try
                            {
                                var imageSource = Imaging.CreateBitmapSourceFromHBitmap(
                                    hBitmap,
                                    IntPtr.Zero,
                                    Int32Rect.Empty,
                                    BitmapSizeOptions.FromEmptyOptions());

                                imageSource.Freeze();
                                return imageSource;
                            }
                            finally
                            {
                                DeleteObject(hBitmap);
                            }
                        }
                    }
                }
                finally
                {
                    DestroyIcon(hIcon);
                }
            }
            catch
            {
                return null;
            }
        }

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);
    }
}
