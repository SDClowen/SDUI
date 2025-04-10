﻿using SkiaSharp;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace SDUI.SK;

public class SKControl : Control
{
    private readonly bool designMode;

    private Bitmap bitmap;
    public SKControl()
    {
        SetStyle(ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
        DoubleBuffered = true;

        designMode = DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime;
    }

    public SKSize CanvasSize => bitmap == null ? SKSize.Empty : new SKSize(bitmap.Width, bitmap.Height);

    [Category("Appearance")]
    public event EventHandler<SKPaintSurfaceEventArgs> PaintSurface;

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
}
