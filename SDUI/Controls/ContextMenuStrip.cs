using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SDUI.Animation;
using SDUI.Extensions;
using SDUI.Helpers;
using SkiaSharp;

namespace SDUI.Controls;

public class ContextMenuStrip : MenuStrip
{
    internal const float ShadowMargin = 7f; // Shadow için ekstra alan (daha subtle)
    private const int MaxIconCacheEntries = 256;
    private const float CheckMarginWidth = 22f;
    private readonly AnimationManager _fadeInAnimation;

    private readonly Dictionary<Bitmap, SKBitmap> _iconCache = new();
    private readonly Dictionary<MenuItem, AnimationManager> _itemHoverAnims = new();
    private readonly SKMaskFilter?[] _shadowMaskFilters = new SKMaskFilter?[2];

    private readonly SKPaint?[] _shadowPaints = new SKPaint?[2];
    private SKPaint? _arrowPaint;

    // Cached Skia resources (avoid per-frame allocations)
    private SKPaint? _bgPaint;
    private SKPaint? _borderPaint;
    private SKPath? _chevronPath;

    private SKFont? _defaultSkFont;
    private int _defaultSkFontDpi;
    private Font? _defaultSkFontSource;
    private MenuItem _hoveredItem;
    private SKPaint? _hoverPaint;
    private SKPaint? _iconPaint;
    private EventHandler _ownerDeactivateHandler;
    private KeyEventHandler _ownerKeyDownHandler;
    private MouseEventHandler _ownerMouseDownHandler;
    private bool _ownerPreviousKeyPreview;
    private UIWindow _ownerWindow;
    private SKPaint? _separatorPaint;

    private bool _showCheckMargin = true;
    private bool _showImageMargin = true;

    private SKPaint? _textPaint;

    public ContextMenuStrip()
    {
        Visible = false;
        AutoSize = false;
        TabStop = false;
        Orientation = Orientation.Vertical;

        // Dikey (dropdown) menü öğeleri için biraz daha yüksek satır
        // ve daha ferah padding.
        ItemHeight = 28f;
        ItemPadding = 6f;

        _fadeInAnimation = new AnimationManager
        {
            Increment = 0.30,
            AnimationType = AnimationType.EaseOut,
            Singular = true,
            InterruptAnimation = false
        };
        _fadeInAnimation.OnAnimationProgress += _ => Invalidate();
    }

    [Category("Layout")]
    [DefaultValue(true)]
    public bool ShowCheckMargin
    {
        get => _showCheckMargin;
        set
        {
            if (_showCheckMargin == value) return;
            _showCheckMargin = value;
            Invalidate();
        }
    }

    [Category("Layout")]
    [DefaultValue(true)]
    public bool ShowImageMargin
    {
        get => _showImageMargin;
        set
        {
            if (_showImageMargin == value) return;
            _showImageMargin = value;
            Invalidate();
        }
    }

    [Category("Behavior")]
    [DefaultValue(true)]
    public bool AutoClose { get; set; } = true;

    [Browsable(false)] public bool IsOpen { get; private set; }

    [Browsable(false)] public UIElementBase SourceElement { get; private set; }

    public event CancelEventHandler Opening;
    public event CancelEventHandler Closing;

    public Size MeasurePreferredSize()
    {
        return GetPrefSize();
    }

    public void Show(UIElementBase element, Point location)
    {
        if (IsOpen) return;
        var owner = ResolveOwner(element);
        if (owner == null) return;


        var canceling = new CancelEventArgs();
        Opening?.Invoke(this, canceling);
        if (canceling.Cancel)
            return;

        SourceElement = element;
        _ownerWindow = owner;

        if (!_ownerWindow.Controls.Contains(this))
            _ownerWindow.Controls.Add(this);

        // Konumu ve boyutu belirle, sonra z-order'ı en üste çek.
        PositionDropDown(location);
        Visible = true;

        // WinForms z-order + SDUI'nin kendi ZOrder sistemini güncelle.
        BringToFront();
        if (_ownerWindow is UIWindow uiw)
        {
            uiw.BringToFront(this);

            // Ensure z-order is reasserted after current message processing to avoid
            // race where other controls draw over the popup on the first show.
            try
            {
                uiw.BeginInvoke((Action)(() =>
                {
                    try
                    {
                        BringToFront();
                        uiw.BringToFront(this);
                        uiw.Invalidate();
                    }
                    catch
                    {
                    }
                }));
            }
            catch
            {
            }
        }

        AttachHandlers();

        _fadeInAnimation.SetProgress(0);
        _fadeInAnimation.StartNewAnimation(AnimationDirection.In);

        _ownerWindow.Invalidate();
        IsOpen = true;
    }

    public void Show(Point location)
    {
        Show(null, location);
    }

    public new void Hide()
    {
        if (!IsOpen) return;

        var canceling = new CancelEventArgs();
        Closing?.Invoke(this, canceling);
        if (canceling.Cancel)
            return;

        DetachHandlers();
        Visible = false;
        _ownerWindow?.Invalidate();
        _ownerWindow = null;
        SourceElement = null;
        IsOpen = false;
    }

    private void PositionDropDown(Point screenLocation)
    {
        if (_ownerWindow == null) return;

        var windowLocation = _ownerWindow.PointToClient(screenLocation);
        var size = GetPrefSize();
        var client = _ownerWindow.ClientRectangle;

        const int MARGIN = 8;
        var maxWidth = client.Width - MARGIN * 2;
        var maxHeight = client.Height - MARGIN * 2;

        size.Width = Math.Min(size.Width, maxWidth);
        size.Height = Math.Min(size.Height, maxHeight);

        var targetX = windowLocation.X;
        var targetY = windowLocation.Y;

        if (targetX + size.Width > client.Right - MARGIN)
        {
            var leftPos = targetX - size.Width - MARGIN;
            if (leftPos >= client.Left + MARGIN)
                targetX = leftPos;
            else
                targetX = client.Right - size.Width - MARGIN;
        }

        if (targetY + size.Height > client.Bottom - MARGIN)
        {
            var topPos = targetY - size.Height;
            if (topPos >= client.Top + MARGIN)
                targetY = topPos;
            else
                targetY = client.Bottom - size.Height - MARGIN;
        }

        targetX = Math.Max(client.Left + MARGIN, Math.Min(targetX, client.Right - size.Width - MARGIN));
        targetY = Math.Max(client.Top + MARGIN, Math.Min(targetY, client.Bottom - size.Height - MARGIN));

        Location = new Point(targetX, targetY);
        Size = size;
    }

    private UIWindow ResolveOwner(UIElementBase element)
    {
        if (Parent is UIWindow w) return w;
        if (element != null)
        {
            if (element.ParentWindow is UIWindow pw) return pw;
            if (element.FindForm() is UIWindow fw) return fw;
        }

        if (Form.ActiveForm is UIWindow aw) return aw;
        return Application.OpenForms.OfType<UIWindow>().FirstOrDefault();
    }

    private void AttachHandlers()
    {
        if (_ownerWindow == null || !AutoClose) return;
        _ownerMouseDownHandler ??= OnOwnerMouseDown;
        _ownerDeactivateHandler ??= OnOwnerDeactivate;
        _ownerKeyDownHandler ??= OnOwnerKeyDown;
        _ownerWindow.MouseDown += _ownerMouseDownHandler;
        _ownerPreviousKeyPreview = _ownerWindow.KeyPreview;
        _ownerWindow.KeyPreview = true;
        _ownerWindow.Deactivate += _ownerDeactivateHandler;
        _ownerWindow.KeyDown += _ownerKeyDownHandler;
    }

    private void DetachHandlers()
    {
        if (_ownerWindow == null) return;
        if (_ownerMouseDownHandler != null) _ownerWindow.MouseDown -= _ownerMouseDownHandler;
        if (_ownerDeactivateHandler != null) _ownerWindow.Deactivate -= _ownerDeactivateHandler;
        if (_ownerKeyDownHandler != null) _ownerWindow.KeyDown -= _ownerKeyDownHandler;
        _ownerWindow.KeyPreview = _ownerPreviousKeyPreview;
    }

    private void OnOwnerMouseDown(object sender, MouseEventArgs e)
    {
        if (!IsOpen || !AutoClose) return;
        if (!Bounds.Contains(e.Location)) Hide();
    }

    private void OnOwnerDeactivate(object sender, EventArgs e)
    {
        if (!IsOpen || !AutoClose) return;
        Hide();
    }

    private void OnOwnerKeyDown(object sender, KeyEventArgs e)
    {
        if (!IsOpen || !AutoClose) return;
        if (e.KeyCode == Keys.Escape)
        {
            Hide();
            e.Handled = true;
        }
    }

    private Size GetPrefSize()
    {
        const float MinContentWidth = 180f; // Minimum içerik genişliği

        // İçerik genişlik/yükseklik hesabı (shadow hariç)
        var contentWidth = ItemPadding * 2;
        var contentHeight = ItemPadding; // Üst padding

        foreach (var item in Items)
        {
            // Respect MenuItem.Visible — skip hidden items from size calculations
            if (!item.Visible)
                continue;

            if (item.IsSeparator)
            {
                contentHeight += SeparatorMargin * 2 + 1 + ItemPadding;
            }
            else
            {
                contentWidth = Math.Max(contentWidth, MeasureItemWidth(item) + ItemPadding * 2);
                contentHeight += ItemHeight + ItemPadding;
            }
        }

        // Minimum genişlik garantisi
        contentWidth = Math.Max(contentWidth, MinContentWidth);

        // En alttaki öğenin border ile kesilmemesi için ekstra alan yok,
        // çünkü son item'dan sonra zaten ItemPadding var.

        // Shadow için her yönden ekstra alan ekle
        var totalWidth = contentWidth + ShadowMargin * 2;
        var totalHeight = contentHeight + ShadowMargin * 2;

        return new Size((int)Math.Ceiling(totalWidth), (int)Math.Ceiling(totalHeight));
    }

    internal override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        var y = ShadowMargin + ItemPadding;
        _hoveredItem = null;
        foreach (var item in Items)
        {
            if (item.IsSeparator)
            {
                y += SeparatorMargin * 2 + 1 + ItemPadding;
                continue;
            }

            var w = Width - ShadowMargin * 2 - ItemPadding * 2;
            var r = new RectangleF(ShadowMargin + ItemPadding, y, w, ItemHeight);
            if (r.Contains(e.Location))
            {
                _hoveredItem = item;
                break;
            }

            y += ItemHeight + ItemPadding;
        }

        Invalidate();
    }

    internal override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _hoveredItem = null;
        Invalidate();
    }

    private AnimationManager EnsureItemHoverAnim(MenuItem item)
    {
        if (!_itemHoverAnims.TryGetValue(item, out var engine))
        {
            engine = new AnimationManager
                { Increment = 0.25, AnimationType = AnimationType.EaseOut, Singular = true, InterruptAnimation = true };
            engine.OnAnimationProgress += _ => Invalidate();
            _itemHoverAnims[item] = engine;
        }

        return engine;
    }

    public override void OnPaint(SKPaintSurfaceEventArgs e)
    {
        base.OnPaint(e);
        var canvas = e.Surface.Canvas;
        var bounds = ClientRectangle;
        canvas.Clear(SKColors.Transparent);

        EnsureSkiaCaches();

        var fadeProgress = (float)_fadeInAnimation.GetProgress();
        var fadeAlpha = (byte)(fadeProgress * 255);
        const float CORNER_RADIUS = 10f;

        // İçerik alanı (shadow margin kadar içeride)
        var contentRect = new SKRect(
            ShadowMargin,
            ShadowMargin,
            bounds.Width - ShadowMargin,
            bounds.Height - ShadowMargin);

        // Multi-layer shadow system (extra subtle)
        canvas.Save();
        EnsureShadowResources();
        for (var i = 0; i < 2; i++)
        {
            var offsetY = 0.75f + i * 0.85f;
            var shadowAlpha = (byte)((8 - i * 3) * fadeProgress);

            var shadowPaint = _shadowPaints[i]!;
            shadowPaint.Color = SKColors.Black.WithAlpha(shadowAlpha);

            canvas.Save();
            canvas.Translate(0, offsetY);
            canvas.DrawRoundRect(contentRect, CORNER_RADIUS, CORNER_RADIUS, shadowPaint);
            canvas.Restore();
        }

        canvas.Restore();

        // High-quality background
        _bgPaint!.Color = MenuBackColor.ToSKColor().WithAlpha(fadeAlpha);
        canvas.DrawRoundRect(contentRect, CORNER_RADIUS, CORNER_RADIUS, _bgPaint);

        // Border
        _borderPaint!.Color = SeparatorColor.ToSKColor().WithAlpha((byte)(fadeAlpha * 0.35f));
        var borderRect = new SKRect(
            contentRect.Left + 0.5f,
            contentRect.Top + 0.5f,
            contentRect.Right - 0.5f,
            contentRect.Bottom - 0.5f);
        canvas.DrawRoundRect(borderRect, CORNER_RADIUS, CORNER_RADIUS, _borderPaint);

        var y = contentRect.Top + ItemPadding;

        foreach (var item in Items)
        {
            // Skip hidden items — visibility should control drawing and layout
            if (!item.Visible)
                continue;

            if (item.IsSeparator)
            {
                _separatorPaint!.Color = SeparatorColor.ToSKColor().WithAlpha(fadeAlpha);
                canvas.DrawLine(
                    contentRect.Left + ItemPadding + 8,
                    y + SeparatorMargin,
                    contentRect.Right - ItemPadding - 8,
                    y + SeparatorMargin,
                    _separatorPaint);
                y += SeparatorMargin * 2 + 1 + ItemPadding;
                continue;
            }

            var w = contentRect.Width - ItemPadding * 2;
            var itemRect = new SKRect(
                contentRect.Left + ItemPadding,
                y,
                contentRect.Left + ItemPadding + w,
                y + ItemHeight);

            var isHovered = item == _hoveredItem;
            var anim = EnsureItemHoverAnim(item);

            if (isHovered) anim.StartNewAnimation(AnimationDirection.In);
            else anim.StartNewAnimation(AnimationDirection.Out);

            var hoverProgress = (float)anim.GetProgress();

            if (hoverProgress > 0.001f || isHovered)
            {
                var hoverAlpha = (byte)Math.Max(180, 180 + 70 * hoverProgress);
                _hoverPaint!.Color = HoverBackColor.ToSKColor().WithAlpha((byte)(fadeAlpha * hoverAlpha / 255f));
                canvas.DrawRoundRect(itemRect, 7, 7, _hoverPaint);
            }

            var textX = itemRect.Left + 12;

            // Reserve space for check mark if enabled
            if (ShowCheckMargin)
            {
                if (item.Checked)
                {
                    var cx = itemRect.Left + 12 + CheckMarginWidth / 2f;
                    var cy = itemRect.MidY;
                    var s = Math.Min(8f, ItemHeight / 3f);
                    _arrowPaint!.Color = MenuForeColor.ToSKColor().WithAlpha(fadeAlpha);
                    var chk = new SKPath();
                    chk.MoveTo(cx - s, cy);
                    chk.LineTo(cx - s / 2f, cy + s / 2f);
                    chk.LineTo(cx + s, cy - s);
                    canvas.DrawPath(chk, _arrowPaint);
                    chk.Dispose();
                }

                textX += CheckMarginWidth;
            }

            var imageAreaWidth = ImageScalingSize.Width + 8;

            if (ShowImageMargin)
            {
                if (ShowIcons && item.Icon != null)
                {
                    var iconY = itemRect.Top + (ItemHeight - ImageScalingSize.Height) / 2;
                    var iconBitmap = GetCachedIconBitmap(item.Icon);
                    _iconPaint!.Color = SKColors.White.WithAlpha(fadeAlpha);
                    canvas.DrawBitmap(iconBitmap,
                        new SKRect(textX, iconY, textX + ImageScalingSize.Width, iconY + ImageScalingSize.Height),
                        _iconPaint);
                }

                textX += imageAreaWidth;
            }
            else
            {
                if (ShowIcons && item.Icon != null)
                {
                    var iconY = itemRect.Top + (ItemHeight - ImageScalingSize.Height) / 2;
                    var iconBitmap = GetCachedIconBitmap(item.Icon);
                    _iconPaint!.Color = SKColors.White.WithAlpha(fadeAlpha);
                    canvas.DrawBitmap(iconBitmap,
                        new SKRect(textX, iconY, textX + ImageScalingSize.Width, iconY + ImageScalingSize.Height),
                        _iconPaint);
                    textX += ImageScalingSize.Width + 8;
                }
            }

            var hoverFore = !HoverForeColor.IsEmpty
                ? HoverForeColor
                : HoverBackColor.IsEmpty
                    ? MenuForeColor
                    : HoverBackColor.Determine();
            var textColor = isHovered ? hoverFore : MenuForeColor;

            var font = GetDefaultSkFont();
            _textPaint!.Color = textColor.ToSKColor().WithAlpha(fadeAlpha);
            var textBounds = SKRect.Create(textX, itemRect.Top, itemRect.Right - textX, itemRect.Height);
            canvas.DrawControlText(item.Text, textBounds, _textPaint, font, ContentAlignment.MiddleLeft, false, true);

            if (ShowSubmenuArrow && item.HasDropDown)
            {
                var chevronSize = 5f;
                var chevronX = itemRect.Right - 14;
                var chevronY = itemRect.MidY;

                _arrowPaint!.Color = textColor.ToSKColor().WithAlpha(fadeAlpha);
                _chevronPath!.Reset();
                _chevronPath.MoveTo(chevronX - chevronSize, chevronY - chevronSize);
                _chevronPath.LineTo(chevronX - chevronSize / 2, chevronY);
                _chevronPath.LineTo(chevronX - chevronSize, chevronY + chevronSize);
                canvas.DrawPath(_chevronPath, _arrowPaint);
            }

            y += ItemHeight + ItemPadding;
        }
    }

    private void EnsureSkiaCaches()
    {
        _bgPaint ??= new SKPaint { IsAntialias = true, FilterQuality = SKFilterQuality.High };
        _borderPaint ??= new SKPaint
            { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1, FilterQuality = SKFilterQuality.High };
        _separatorPaint ??= new SKPaint { IsAntialias = true, StrokeWidth = 1 };
        _hoverPaint ??= new SKPaint { IsAntialias = true, FilterQuality = SKFilterQuality.High };
        _iconPaint ??= new SKPaint { IsAntialias = true, FilterQuality = SKFilterQuality.High };
        _textPaint ??= new SKPaint { IsAntialias = true };
        _arrowPaint ??= new SKPaint
        {
            IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1.2f, StrokeCap = SKStrokeCap.Round,
            StrokeJoin = SKStrokeJoin.Round
        };
        _chevronPath ??= new SKPath();
    }

    private void EnsureShadowResources()
    {
        // Two-layer shadow system
        var blur0 = 3.0f;
        var blur1 = 5.75f;

        if (_shadowMaskFilters[0] == null)
            _shadowMaskFilters[0] = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, blur0);
        if (_shadowMaskFilters[1] == null)
            _shadowMaskFilters[1] = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, blur1);

        if (_shadowPaints[0] == null)
            _shadowPaints[0] = new SKPaint { IsAntialias = true, MaskFilter = _shadowMaskFilters[0] };
        if (_shadowPaints[1] == null)
            _shadowPaints[1] = new SKPaint { IsAntialias = true, MaskFilter = _shadowMaskFilters[1] };
    }

    private SKBitmap GetCachedIconBitmap(Bitmap icon)
    {
        if (_iconCache.TryGetValue(icon, out var cached))
            return cached;

        if (_iconCache.Count >= MaxIconCacheEntries)
        {
            foreach (var pair in _iconCache)
                pair.Value.Dispose();
            _iconCache.Clear();
        }

        var bmp = icon.ToSKBitmap();
        _iconCache[icon] = bmp;
        return bmp;
    }

    // Include reserved margins for checks and images when measuring dropdown item width
    protected new float MeasureItemWidth(MenuItem item)
    {
        if (item is MenuItemSeparator) return 20f;

        var w = base.MeasureItemWidth(item);

        if (ShowCheckMargin)
            w += CheckMarginWidth;

        if (ShowImageMargin)
            if (!(ShowIcons && item.Icon != null))
                w += ImageScalingSize.Width + 8;

        return w;
    }

    private SKFont GetDefaultSkFont()
    {
        var dpi = DeviceDpi > 0 ? DeviceDpi : 96;
        if (_defaultSkFont == null || !ReferenceEquals(_defaultSkFontSource, Font) || _defaultSkFontDpi != dpi)
        {
            _defaultSkFont?.Dispose();
            _defaultSkFont = new SKFont
            {
                Size = Font.Size.PtToPx(this),
                Typeface = FontManager.GetSKTypeface(Font),
                Subpixel = true,
                Edging = SKFontEdging.SubpixelAntialias
            };
            _defaultSkFontSource = Font;
            _defaultSkFontDpi = dpi;
        }

        return _defaultSkFont;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _defaultSkFont?.Dispose();
            _defaultSkFont = null;

            _bgPaint?.Dispose();
            _bgPaint = null;
            _borderPaint?.Dispose();
            _borderPaint = null;
            _separatorPaint?.Dispose();
            _separatorPaint = null;
            _hoverPaint?.Dispose();
            _hoverPaint = null;
            _iconPaint?.Dispose();
            _iconPaint = null;
            _textPaint?.Dispose();
            _textPaint = null;
            _arrowPaint?.Dispose();
            _arrowPaint = null;
            _chevronPath?.Dispose();
            _chevronPath = null;

            for (var i = 0; i < _shadowPaints.Length; i++)
            {
                _shadowPaints[i]?.Dispose();
                _shadowPaints[i] = null;
            }

            for (var i = 0; i < _shadowMaskFilters.Length; i++)
            {
                _shadowMaskFilters[i]?.Dispose();
                _shadowMaskFilters[i] = null;
            }

            foreach (var pair in _iconCache)
                pair.Value.Dispose();
            _iconCache.Clear();
        }

        base.Dispose(disposing);
    }
}