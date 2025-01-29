using System;
using System.Runtime.InteropServices;

namespace SDUI.Helpers
{
    public static class DpiHelper
    {
        private const int LOGPIXELSX = 88;
        private const int LOGPIXELSY = 90;

        [DllImport("gdi32.dll")]
        private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDc);

        public static int GetSystemDpi()
        {
            IntPtr screenDC = GetDC(IntPtr.Zero);
            try
            {
                int dpiX = GetDeviceCaps(screenDC, LOGPIXELSX);
                int dpiY = GetDeviceCaps(screenDC, LOGPIXELSY);
                return Math.Max(dpiX, dpiY); // Genellikle X ve Y aynıdır, ama en yükseğini alalım
            }
            finally
            {
                ReleaseDC(IntPtr.Zero, screenDC);
            }
        }

        public static float GetScaleFactor()
        {
            return GetSystemDpi() / 96f;
        }

        public static int ScaleValue(int value)
        {
            return (int)(value * GetScaleFactor());
        }

        public static float ScaleValue(float value)
        {
            return value * GetScaleFactor();
        }
    }
}