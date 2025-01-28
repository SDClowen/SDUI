using SkiaSharp;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace SDUI.Controls;

public class SKControl : Control
{
    public float DPI => DeviceDpi / 96f;

    #region TODO
    public bool UseVisualStyleBackColor { get; set; }
    #endregion

    private readonly bool designMode;

    private Bitmap bitmap;

    private bool _autoSize;
    [Category("Layout")]
    [DefaultValue(false)]
    public override bool AutoSize
    {
        get => _autoSize;
        set
        {
            if (_autoSize == value)
                return;

            _autoSize = value;
            if (value)
                AdjustSize();
        }
    }

    private AutoSizeMode _autoSizeMode = AutoSizeMode.GrowAndShrink;
    [Category("Layout")]
    [DefaultValue(AutoSizeMode.GrowAndShrink)]
    public virtual AutoSizeMode AutoSizeMode
    {
        get => _autoSizeMode;
        set
        {
            if (_autoSizeMode == value)
                return;

            _autoSizeMode = value;
            if (AutoSize)
                AdjustSize();
        }
    }

    private bool _useMnemonic = true;
    [Category("Behavior")]
    [DefaultValue(true)]
    public bool UseMnemonic
    {
        get => _useMnemonic;
        set
        {
            if (_useMnemonic == value)
                return;

            _useMnemonic = value;
            Invalidate();
        }
    }

    private bool _autoEllipsis;
    [Category("Behavior")]
    [DefaultValue(false)]
    public bool AutoEllipsis
    {
        get => _autoEllipsis;
        set
        {
            if (_autoEllipsis == value)
                return;

            _autoEllipsis = value;
            Invalidate();
        }
    }

    private ContentAlignment _textAlign = ContentAlignment.MiddleCenter;
    [Category("Appearance")]
    [DefaultValue(ContentAlignment.MiddleCenter)]
    public ContentAlignment TextAlign
    {
        get => _textAlign;
        set
        {
            if (_textAlign == value)
                return;

            _textAlign = value;
            Invalidate();
        }
    }

    private string _text = string.Empty;
    [Category("Appearance")]
    [DefaultValue("")]
    public override string Text
    {
        get => _text;
        set
        {
            if (_text == value)
                return;

            _text = value;
            OnTextChanged(EventArgs.Empty);
            if (AutoSize)
                AdjustSize();
            Invalidate();
        }
    }

    [Category("Appearance")]
    public Image Image { get; set; }

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

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (AutoSize)
            AdjustSize();
    }

    protected virtual void AdjustSize()
    {
        if (!AutoSize)
            return;

        var proposedSize = GetPreferredSize(Size.Empty);
        if (AutoSizeMode == AutoSizeMode.GrowOnly)
        {
            proposedSize.Width = Math.Max(Width, proposedSize.Width);
            proposedSize.Height = Math.Max(Height, proposedSize.Height);
        }

        if (Size != proposedSize)
            Size = proposedSize;
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
