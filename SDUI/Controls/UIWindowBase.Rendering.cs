using SDUI.Native.Windows;
using SkiaSharp;
using System;
using System.Runtime.InteropServices;
using static SDUI.Native.Windows.Methods;

namespace SDUI.Controls;

public partial class UIWindowBase
{
    /// <summary>
    /// Handles WM_PAINT message - creates Skia surface and renders to native HDC.
    /// Uses Memory DC + BitBlt for zero-copy rendering.
    /// </summary>
    private IntPtr HandlePaint(IntPtr hWnd)
    {
        PAINTSTRUCT ps;
        var hdc = BeginPaint(hWnd, out ps);
        
        if (hdc == IntPtr.Zero)
            return IntPtr.Zero;

        try
        {
            // Get client dimensions
            var clientRect = new Rect();
            GetClientRect(hWnd, ref clientRect);
            
            int width = clientRect.Right - clientRect.Left;
            int height = clientRect.Bottom - clientRect.Top;

            if (width <= 0 || height <= 0)
                return IntPtr.Zero;

            // Create memory DC
            var memDC = CreateCompatibleDC(hdc);
            if (memDC == IntPtr.Zero)
                return IntPtr.Zero;

            try
            {
                // Create BITMAPINFO for DIB section
                var bmi = new BITMAPINFO();
                bmi.biSize = Marshal.SizeOf<BITMAPINFOHEADER>();
                bmi.biWidth = width;
                bmi.biHeight = -height; // Negative for top-down DIB
                bmi.biPlanes = 1;
                bmi.biBitCount = 32;
                bmi.biCompression = 0; // BI_RGB

                // Create DIB section - Skia will render directly to these pixels
                IntPtr pixels;
                var hBitmap = CreateDIBSection(hdc, ref bmi, 0, out pixels, IntPtr.Zero, 0);
                
                if (hBitmap == IntPtr.Zero || pixels == IntPtr.Zero)
                    return IntPtr.Zero;

                try
                {
                    var oldBitmap = SelectObject(memDC, hBitmap);

                    // Create Skia surface directly on DIB section pixels (zero copy!)
                    var info = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
                    int rowBytes = width * 4;
                    
                    using (var surface = SKSurface.Create(info, pixels, rowBytes))
                    {
                        if (surface != null)
                        {
                            var canvas = surface.Canvas;
                            
                            // Clear and render
                            canvas.Clear(BackColor);
                            
                            // Virtual method for derived classes to override
                            OnPaintCanvas(canvas, info);
                            
                            canvas.Flush();
                        }
                    }

                    // Hardware-accelerated blit from memory DC to screen
                    BitBlt(hdc, 0, 0, width, height, memDC, 0, 0, SRCCOPY);

                    SelectObject(memDC, oldBitmap);
                    DeleteObject(hBitmap);
                }
                catch
                {
                    if (hBitmap != IntPtr.Zero)
                        DeleteObject(hBitmap);
                    throw;
                }
            }
            finally
            {
                DeleteDC(memDC);
            }
        }
        finally
        {
            EndPaint(hWnd, ref ps);
        }

        return IntPtr.Zero;
    }

    /// <summary>
    /// Virtual method for derived classes to implement custom rendering.
    /// </summary>
    protected virtual void OnPaintCanvas(SKCanvas canvas, SKImageInfo info)
    {
        // Base implementation draws nothing - derived classes override
    }

    #region Native Structures and Methods for GDI Drawing

    /// <summary>
    /// Invalidates the window and requests a repaint on the next message loop iteration.
    /// </summary>
    public virtual void InvalidateWindow()
    {
        if (!IsHandleCreated || IsDisposed || Disposing)
            return;

        InvalidateRect(Handle, new Rect(), false);
    }

    /// <summary>
    /// Converts a window-relative rectangle to screen coordinates.
    /// </summary>
    public SKRect RectangleToScreen(SKRect clientRect)
    {
        var topLeft = PointToScreen(clientRect.Location);
        return SKRect.Create(topLeft, clientRect.Size);
    }

    /// <summary>
    /// Converts a screen-space rectangle to window client coordinates.
    /// </summary>
    public SKRect RectangleToClient(SKRect screenRect)
    {
        var topLeft = PointToClient(screenRect.Location);
        return SKRect.Create(topLeft, screenRect.Size);
    }

    /// <summary>
    /// Gets the window rectangle in screen coordinates (including frame/borders).
    /// </summary>
    public SKRectI GetWindowRect()
    {
        if (!IsHandleCreated)
            return SKRectI.Empty;

        Rect rect;
        Methods.GetWindowRect(Handle, out rect);
        
        return SKRectI.Create(
            rect.Left,
            rect.Top,
            rect.Right - rect.Left,
            rect.Bottom - rect.Top);
    }

    /// <summary>
    /// Gets the client rectangle in screen coordinates.
    /// </summary>
    public SKRect GetClientRectScreen()
    {
        if (!IsHandleCreated)
            return SKRect.Empty;

        return RectangleToScreen(ClientRectangle);
    }

    /// <summary>
    /// Forces an immediate synchronous paint of the window.
    /// Only use when absolutely necessary - prefer Invalidate() for normal updates.
    /// </summary>
    public virtual void Update()
    {
        if (!IsHandleCreated || IsDisposed || Disposing)
            return;

        UpdateWindow(Handle);
    }

    #endregion

    #region Native Structures and Methods for GDI Drawing

    [StructLayout(LayoutKind.Sequential)]
    private struct BITMAPINFO
    {
        public int biSize;
        public int biWidth;
        public int biHeight;
        public short biPlanes;
        public short biBitCount;
        public int biCompression;
        public int biSizeImage;
        public int biXPelsPerMeter;
        public int biYPelsPerMeter;
        public int biClrUsed;
        public int biClrImportant;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct BITMAPINFOHEADER
    {
        public int biSize;
        public int biWidth;
        public int biHeight;
        public short biPlanes;
        public short biBitCount;
        public int biCompression;
        public int biSizeImage;
        public int biXPelsPerMeter;
        public int biYPelsPerMeter;
        public int biClrUsed;
        public int biClrImportant;
    }

    [DllImport("user32.dll")]
    private static extern IntPtr BeginPaint(IntPtr hWnd, out PAINTSTRUCT lpPaint);

    [DllImport("user32.dll")]
    private static extern bool EndPaint(IntPtr hWnd, ref PAINTSTRUCT lpPaint);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateDIBSection(
        IntPtr hdc,
        ref BITMAPINFO pbmi,
        uint iUsage,
        out IntPtr ppvBits,
        IntPtr hSection,
        uint dwOffset);

    [DllImport("gdi32.dll")]
    private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr hObject);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteDC(IntPtr hdc);

    [DllImport("gdi32.dll")]
    private static extern bool BitBlt(
        IntPtr hdcDest,
        int nXDest,
        int nYDest,
        int nWidth,
        int nHeight,
        IntPtr hdcSrc,
        int nXSrc,
        int nYSrc,
        int dwRop);

    private const int SRCCOPY = 0x00CC0020;

    [DllImport("user32.dll")]
    private static extern bool UpdateWindow(IntPtr hWnd);

    [StructLayout(LayoutKind.Sequential)]
    private struct PAINTSTRUCT
    {
        public IntPtr hdc;
        public bool fErase;
        public Rect rcPaint;
        public bool fRestore;
        public bool fIncUpdate;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] rgbReserved;
    }

    #endregion
}
