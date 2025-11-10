using System;
using System.Runtime.InteropServices;

namespace SDUI.Helpers
{
    public static class DpiHelper
    {
        private const int LOGPIXELSX = 88;
        private const int LOGPIXELSY = 90;
        private const uint MONITOR_DEFAULTTONEAREST = 0x00000002;

        private enum MonitorDpiType
        {
            EffectiveDpi = 0,
            AngularDpi = 1,
            RawDpi = 2,
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        [DllImport("gdi32.dll")]
        private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDc);

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromPoint(POINT pt, uint dwFlags);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        private static extern uint GetDpiForWindowInternal(IntPtr hwnd);

        [DllImport("shcore.dll")]
        private static extern int GetDpiForMonitor(IntPtr hmonitor, MonitorDpiType dpiType, out uint dpiX, out uint dpiY);

        public static int GetSystemDpi()
        {
            IntPtr screenDC = GetDC(IntPtr.Zero);
            try
            {
                int dpiX = GetDeviceCaps(screenDC, LOGPIXELSX);
                int dpiY = GetDeviceCaps(screenDC, LOGPIXELSY);
                return Math.Max(dpiX, dpiY);
            }
            finally
            {
                ReleaseDC(IntPtr.Zero, screenDC);
            }
        }

        public static int GetDpiForWindow(IntPtr handle)
        {
            if (handle != IntPtr.Zero)
            {
                try
                {
                    return checked((int)GetDpiForWindowInternal(handle));
                }
                catch (EntryPointNotFoundException)
                {
                    // API not available on this OS version, fallback below.
                }
                catch (DllNotFoundException)
                {
                    // API not available, fallback below.
                }
            }

            var monitor = handle != IntPtr.Zero
                ? MonitorFromWindow(handle, MONITOR_DEFAULTTONEAREST)
                : GetMonitorFromCursor();

            if (monitor != IntPtr.Zero)
            {
                try
                {
                    if (GetDpiForMonitor(monitor, MonitorDpiType.EffectiveDpi, out uint dpiX, out uint _) == 0)
                    {
                        return (int)dpiX;
                    }
                }
                catch (DllNotFoundException)
                {
                }
                catch (EntryPointNotFoundException)
                {
                }
            }

            return GetSystemDpi();
        }

        private static IntPtr GetMonitorFromCursor()
        {
            return GetCursorPos(out var pt)
                ? MonitorFromPoint(pt, MONITOR_DEFAULTTONEAREST)
                : IntPtr.Zero;
        }

        public static float GetScaleFactor(IntPtr handle)
        {
            return GetDpiForWindow(handle) / 96f;
        }

        public static float GetScaleFactor()
        {
            return GetScaleFactor(IntPtr.Zero);
        }

        public static int ScaleValue(int value, IntPtr handle)
        {
            return (int)Math.Round(value * GetScaleFactor(handle));
        }

        public static float ScaleValue(float value, IntPtr handle)
        {
            return value * GetScaleFactor(handle);
        }

        public static int ScaleValue(int value)
        {
            return ScaleValue(value, IntPtr.Zero);
        }

        public static float ScaleValue(float value)
        {
            return ScaleValue(value, IntPtr.Zero);
        }
    }
}