using SkiaSharp;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SDUI.Controls;

public class SKControl : Control
{
    public float DPI => DeviceDpi /96f;

    #region TODO
    public bool UseVisualStyleBackColor { get; set; }
    #endregion

    private readonly bool designMode;

    // Persistent rendering resources
    private IntPtr _pixelPtr = IntPtr.Zero;
    private int _stride;
    private SKImageInfo _info;
    private SKSurface? _surface; // reused between paints
    private Bitmap? _gdiBitmap; // wraps the pixel buffer for GDI blit

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

    public SKSize CanvasSize => _info.Size;

    [Category("Appearance")]
    public event EventHandler<SKPaintSurfaceEventArgs> PaintSurface;

    protected virtual void OnPaintSurface(SKPaintSurfaceEventArgs e) => PaintSurface?.Invoke(this, e);

    protected override void OnPaint(PaintEventArgs e)
    {
        if (designMode)
        {
            base.OnPaint(e);
            return;
        }

        EnsureSurface();

        if (_surface == null || _info.Width ==0 || _info.Height ==0)
            return;

        // Paint parent background onto destination first for proper transparency
        CheckBoxRenderer.DrawParentBackground(e.Graphics, ClientRectangle, this);

        // Prepare canvas for this frame
        _surface.Canvas.Clear(SKColors.Transparent);

        // Raise event for client drawing
        OnPaintSurface(new SKPaintSurfaceEventArgs(_surface, _info));

        _surface.Canvas.Flush();

        if (_gdiBitmap != null)
        {
            e.Graphics.DrawImageUnscaled(_gdiBitmap,0,0);
        }
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        RecreateSurface();
        Invalidate();
        if (AutoSize)
            AdjustSize();
    }

    // Handle per-monitor DPI change (Control does not expose OnDpiChanged override)
    public event EventHandler? DpiChanged; // consumers can subscribe

    protected virtual void OnDpiChanged(float newDpi, Rectangle suggestedBounds)
    {
        if (!suggestedBounds.IsEmpty)
        {
            try { Bounds = suggestedBounds; } catch { }
        }
        RecreateSurface();
        Invalidate();
        DpiChanged?.Invoke(this, EventArgs.Empty);
    }

    protected override void WndProc(ref Message m)
    {
        const int WM_DPICHANGED =0x02E0;
        if (m.Msg == WM_DPICHANGED)
        {
            try
            {
                var rect = Marshal.PtrToStructure<RECT>(m.LParam);
                var suggested = Rectangle.FromLTRB(rect.Left, rect.Top, rect.Right, rect.Bottom);
                OnDpiChanged((float)HIWORD(m.WParam), suggested); // new DPI (Y)
            }
            catch
            {
                OnDpiChanged(DPI *96f, Rectangle.Empty);
            }
        }
        base.WndProc(ref m);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT { public int Left; public int Top; public int Right; public int Bottom; }
    private static int HIWORD(IntPtr ptr) => ((int)(long)ptr >>16) &0xFFFF;

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
        FreeSurface();
    }

    private void EnsureSurface()
    {
        if (_surface != null)
            return;
        RecreateSurface();
    }

    private void RecreateSurface()
    {
        FreeSurface();
        if (Width <=0 || Height <=0)
            return;

        _info = new SKImageInfo(Width, Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
        _stride = _info.RowBytes;

        // Allocate unmanaged pixel buffer
        _pixelPtr = Marshal.AllocHGlobal(_stride * _info.Height);
        // Clear initial buffer (transparent) using managed array copy to avoid unsafe code
        var clear = new byte[_stride * _info.Height];
        Marshal.Copy(clear,0, _pixelPtr, clear.Length);

        _surface = SKSurface.Create(_info, _pixelPtr, _stride);

        // Wrap with GDI Bitmap for blitting (premultiplied alpha format)
        _gdiBitmap = new Bitmap(_info.Width, _info.Height, _stride, PixelFormat.Format32bppPArgb, _pixelPtr);

        // No background drawing here; it is drawn per-frame onto destination Graphics
    }

    private void FreeSurface()
    {
        _surface?.Dispose();
        _surface = null;
        _gdiBitmap?.Dispose();
        _gdiBitmap = null;
        if (_pixelPtr != IntPtr.Zero)
        {
            Marshal.FreeHGlobal(_pixelPtr);
            _pixelPtr = IntPtr.Zero;
        }
        _info = SKImageInfo.Empty;
        _stride =0;
    }
}
