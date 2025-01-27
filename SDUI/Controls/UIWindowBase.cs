﻿using SDUI.Helpers;
using SDUI.SK;
using SkiaSharp;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static SDUI.NativeMethods;

namespace SDUI.Controls;

public class UIWindowBase : Form
{
    protected bool enableFullDraggable;
    private int dwmMargin = 1;
    private bool right = false;
    private Point location;


    public int DwmMargin
    {
        get => dwmMargin;
        set => dwmMargin = value;
    }

    /// <summary>
    /// Get DPI
    /// </summary>
    public float DPI => DeviceDpi / 96.0f;

    /// <summary>
    /// Has aero enabled by windows <c>true</c>; otherwise <c>false</c>
    /// </summary>
    private bool _aeroEnabled
    {
        get
        {
            if (Environment.OSVersion.Version.Major >= 6)
            {
                int enabled = 0;
                DwmIsCompositionEnabled(ref enabled);
                return enabled == 1;
            }

            return false;
        }
    }

    private Bitmap bitmap;
    private readonly bool designMode;
    public SKSize CanvasSize => bitmap == null ? SKSize.Empty : new SKSize(bitmap.Width, bitmap.Height);

    [Category("Appearance")]
    public event EventHandler<SKPaintSurfaceEventArgs> PaintSurface;
    public UIWindowBase()
    {
        SetStyle(ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
        DoubleBuffered = true;

        designMode = DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime;

        BackColor = Color.FromArgb(0, 0, 0, 0);
        ResizeRedraw = true;
    }

    protected virtual void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        PaintSurface?.Invoke(this, e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        if (designMode)
            return;

        base.OnPaint(e);
        CheckBoxRenderer.DrawParentBackground(e.Graphics, ClientRectangle, this);

        // get the bitmap
        var info = CreateBitmap();

        if (info.Width == 0 || info.Height == 0)
            return;

        var data = bitmap.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.WriteOnly, bitmap.PixelFormat);

        // create the surface
        using (var surface = SKSurface.Create(info, data.Scan0, data.Stride))
        {
            // start drawing
            OnPaintSurface(new SKPaintSurfaceEventArgs(surface, info));

            surface.Canvas.Flush();
        }

        // write the bitmap to the graphics
        bitmap.UnlockBits(data);
        e.Graphics.DrawImage(bitmap, 0, 0);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        FreeBitmap();
    }

    private SKImageInfo CreateBitmap()
    {
        var info = new SKImageInfo(Width, Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);

        if (bitmap == null || bitmap.Width != info.Width || bitmap.Height != info.Height)
        {
            FreeBitmap();

            if (info.Width != 0 && info.Height != 0)
                bitmap = new Bitmap(info.Width, info.Height, PixelFormat.Format32bppPArgb);
        }

        return info;
    }

    private void FreeBitmap()
    {
        if (bitmap != null)
        {
            bitmap.Dispose();
            bitmap = null;
        }
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (msg.Msg == 256 | msg.Msg == 260)
            if (keyData == Keys.Escape)
                Close();

        return base.ProcessCmdKey(ref msg, keyData);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        right = e.Button == MouseButtons.Right;
        if (right)
        {
            location = e.Location;
            Invalidate();
        }

        if (!enableFullDraggable)
            return;

        if (e.Button == MouseButtons.Left)
            DragForm(Handle);
    }

    protected void DragForm(IntPtr handle)
    {
        ReleaseCapture();
        SendMessage(handle, WM_SYSCOMMAND, SC_MOVE + HTCAPTION, 0);
    }

    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;
            if (!_aeroEnabled)
                cp.ClassStyle |= CS_DROPSHADOW;

            cp.ClassStyle |= CS_DBLCLKS;

            var style = (uint)cp.Style;

            if (DesignMode)
                style &= ~(uint)SetWindowLongFlags.WS_CAPTION;

            style &= ~(uint)SetWindowLongFlags.WS_SYSMENU;
            style &= ~(uint)SetWindowLongFlags.WS_THICKFRAME;
            style &= ~(uint)SetWindowLongFlags.WS_MINIMIZE;
            style &= ~(uint)SetWindowLongFlags.WS_MINIMIZEBOX;
            style &= ~(uint)SetWindowLongFlags.WS_MAXIMIZE;
            style &= ~(uint)SetWindowLongFlags.WS_MAXIMIZEBOX;
            //style &= ~(uint)SetWindowLongFlags.WS_BORDER;
            style |= (uint)SetWindowLongFlags.WS_TILED;
            cp.Style = (int)style;

            return cp;
        }
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        if (DesignMode)
            return;

        // Otherwise, it will not be applied.
        if (StartPosition == FormStartPosition.CenterScreen)
            CenterToScreen();

        if (BackColor != ColorScheme.BackColor)
            BackColor = ColorScheme.BackColor;
    }

    private const int htLeft = 10;
    private const int htRight = 11;
    private const int htTop = 12;
    private const int htTopLeft = 13;
    private const int htTopRight = 14;
    private const int htBottom = 15;
    private const int htBottomLeft = 16;
    private const int htBottomRight = 17;
    protected override void WndProc(ref Message m)
    {
        if (DesignMode)
        {
            base.WndProc(ref m);
            return;
        }

        switch (m.Msg)
        {
            case WM_NCHITTEST:
                {
                    if (WindowState != FormWindowState.Maximized)
                    {
                        int gripDist = 10;

                        var pt = PointToClient(Cursor.Position);

                        Size clientSize = ClientSize;
                        ///allow resize on the lower right corner
                        if (pt.X >= clientSize.Width - gripDist && pt.Y >= clientSize.Height - gripDist && clientSize.Height >= gripDist)
                        {
                            m.Result = (IntPtr)(IsMirrored ? htBottomLeft : htBottomRight);
                            return;
                        }
                        ///allow resize on the lower left corner
                        if (pt.X <= gripDist && pt.Y >= clientSize.Height - gripDist && clientSize.Height >= gripDist)
                        {
                            m.Result = (IntPtr)(IsMirrored ? htBottomRight : htBottomLeft);
                            return;
                        }
                        ///allow resize on the upper right corner
                        if (pt.X <= gripDist && pt.Y <= gripDist && clientSize.Height >= gripDist)
                        {
                            m.Result = (IntPtr)(IsMirrored ? htTopRight : htTopLeft);
                            return;
                        }
                        ///allow resize on the upper left corner
                        if (pt.X >= clientSize.Width - gripDist && pt.Y <= gripDist && clientSize.Height >= gripDist)
                        {
                            m.Result = (IntPtr)(IsMirrored ? htTopLeft : htTopRight);
                            return;
                        }
                        ///allow resize on the top border
                        if (pt.Y <= 2 && clientSize.Height >= 2)
                        {
                            m.Result = (IntPtr)htTop;
                            return;
                        }
                        ///allow resize on the bottom border
                        if (pt.Y >= clientSize.Height - gripDist && clientSize.Height >= gripDist)
                        {
                            m.Result = (IntPtr)htBottom;
                            return;
                        }
                        ///allow resize on the left border
                        if (pt.X <= gripDist && clientSize.Height >= gripDist)
                        {
                            m.Result = (IntPtr)htLeft;
                            return;
                        }
                        ///allow resize on the right border
                        if (pt.X >= clientSize.Width - gripDist && clientSize.Height >= gripDist)
                        {
                            m.Result = (IntPtr)htRight;
                            return;
                        }
                    }

                    if ((int)m.Result == HTCLIENT)     // drag the form
                        m.Result = (IntPtr)HTCAPTION;

                    break;
                }
            case WM_NCCALCSIZE:

                var handle = Handle;

                var lpwp = (WINDOWPOS)Marshal.PtrToStructure(m.LParam, typeof(WINDOWPOS));
                if (lpwp.HWND == IntPtr.Zero)
                    return;

                if ((lpwp.flags & (SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_NOREDRAW)) != 0)
                {
                    return;
                }
                // TEMPORARY CODE
                // if (OS.IsAppThemed ()) {
                // OS.InvalidateRect (handle, null, true);
                // return result;
                // }
                int bits = GetWindowLong(handle, WindowLongIndexFlags.GWL_STYLE).ToInt32();
                if ((bits & TCS_MULTILINE) != 0)
                {
                    InvalidateRect(handle, new Rect(), true);
                    return;
                }

                var rect = new Rect();
                SetRect(rect, 0, 0, lpwp.cx, lpwp.cy);

                SendMessage(handle, WM_NCCALCSIZE, 0, ref rect);
                int newWidth = rect.Right - rect.Left;
                int newHeight = rect.Bottom - rect.Top;
                GetClientRect(handle, ref rect);
                int oldWidth = rect.Right - rect.Left;
                int oldHeight = rect.Bottom - rect.Top;
                if (newWidth == oldWidth && newHeight == oldHeight)
                {
                    return;
                }
                var inset = new Rect();
                SendMessage(handle, TCM_ADJUSTRECT, 0, ref inset);
                int marginX = -inset.Right, marginY = -inset.Bottom;
                if (newWidth != oldWidth)
                {
                    int left = oldWidth;
                    if (newWidth < oldWidth)
                        left = newWidth;
                    SetRect(rect, left - marginX, 0, newWidth, newHeight);
                    InvalidateRect(handle, rect, true);
                }
                if (newHeight != oldHeight)
                {
                    int bottom = oldHeight;
                    if (newHeight < oldHeight)
                        bottom = newHeight;
                    if (newWidth < oldWidth)
                        oldWidth -= marginX;
                    SetRect(rect, 0, bottom - marginY, oldWidth, newHeight);
                    InvalidateRect(handle, rect, true);

                }
                return;
        }

        base.WndProc(ref m);
    }

    public void ChangeControlsTheme(Control control)
    {
        control.Enabled = false;

        var isDark = ColorScheme.BackColor.IsDark();
        if (control is ListView)
        {
            if (WindowsHelper.IsModern)
            {
                SendMessage(control.Handle, 0x0127, (1 << 16) | (1 & 0xffff), 0);

                AllowDarkModeForWindow(control.Handle, true);
            }
        }

        if (control is RichTextBox || control is ListBox)
        {
            try
            {
                control.BackColor = ColorScheme.BackColor;
                control.ForeColor = ColorScheme.ForeColor;
            }
            catch (Exception)
            {
            }
        }

        WindowsHelper.UseImmersiveDarkMode(control.Handle, isDark);

        foreach (Control subControl in control.Controls)
        {
            ChangeControlsTheme(subControl);
        }
        control.Enabled = true;
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);

        if (DesignMode)
            return;

        WindowsHelper.ApplyRoundCorner(this.Handle);

        if (_aeroEnabled)
        {
            var v = 2;

            DwmSetWindowAttribute(Handle, DWMWINDOWATTRIBUTE.DWMWA_NCRENDERING_POLICY, ref v, 4);
            var margins = new MARGINS()
            {
                Bottom = dwmMargin,
                Left = dwmMargin,
                Right = dwmMargin,
                Top = dwmMargin
            };

            DwmExtendFrameIntoClientArea(this.Handle, ref margins);
        }

        SetWindowPos(Handle, IntPtr.Zero, 0, 0, 0, 0, SetWindowPosFlags.SWP_FRAMECHANGED | SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOZORDER | SetWindowPosFlags.SWP_NOOWNERZORDER | SetWindowPosFlags.SWP_NOACTIVATE);
    }

    protected override void OnBackColorChanged(EventArgs e)
    {
        base.OnBackColorChanged(e);
        if (DesignMode)
            return;

        ChangeControlsTheme(this);

        if (!WindowsHelper.IsModern)
            return;

        WindowsHelper.UseImmersiveDarkMode(Handle, ColorScheme.BackColor.IsDark());

        if (ColorScheme.BackColor.IsDark())
        {
            var flag = DWMSBT_TABBEDWINDOW;
            DwmSetWindowAttribute(
                Handle,
                DWMWINDOWATTRIBUTE.DWMWA_SYSTEMBACKDROP_TYPE,
                ref flag,
                 Marshal.SizeOf<int>());
        }

    }
}