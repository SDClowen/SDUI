using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SkiaSharp;
using SDUI.Extensions;
using SDUI.Animation;

namespace SDUI.Controls;

public class ContextMenuStrip : MenuStrip
{
    internal const float ShadowMargin = 7f; // Shadow için ekstra alan (daha subtle)

    private UIElementBase _sourceElement;
    private bool _isOpen;
    private UIWindow _ownerWindow;
    private bool _autoClose = true;
    private MouseEventHandler _ownerMouseDownHandler;
    private EventHandler _ownerDeactivateHandler;
    private KeyEventHandler _ownerKeyDownHandler;
    private bool _ownerPreviousKeyPreview;
    private MenuItem _hoveredItem;
    private readonly AnimationManager _fadeInAnimation;
    private readonly Dictionary<MenuItem, AnimationManager> _itemHoverAnims = new();

    // Cached Skia resources (avoid per-frame allocations)
    private SKPaint? _bgPaint;
    private SKPaint? _borderPaint;
    private SKPaint? _separatorPaint;
    private SKPaint? _hoverPaint;
    private SKPaint? _iconPaint;
    private SKPaint? _textPaint;
    private SKPaint? _arrowPaint;
    private SKPath? _chevronPath;

    private readonly SKPaint?[] _shadowPaints = new SKPaint?[2];
    private readonly SKMaskFilter?[] _shadowMaskFilters = new SKMaskFilter?[2];

    private SKFont? _defaultSkFont;
    private Font? _defaultSkFontSource;
    private int _defaultSkFontDpi;

    private readonly Dictionary<Bitmap, SKBitmap> _iconCache = new();
    private const int MaxIconCacheEntries = 256;

    public event EventHandler Opening;
    public event EventHandler Closing;

    public ContextMenuStrip()
    {
        Visible = false;
        AutoSize = false;
        TabStop = false;
        Orientation = SDUI.Orientation.Vertical;

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

    [Category("Behavior")][DefaultValue(true)] public bool AutoClose { get => _autoClose; set => _autoClose = value; }
    [Browsable(false)] public bool IsOpen => _isOpen;
    [Browsable(false)] public UIElementBase SourceElement => _sourceElement;

    public Size MeasurePreferredSize() => GetPrefSize();

    public void Show(UIElementBase element, Point location)
    {
        if (_isOpen) return;
        var owner = ResolveOwner(element);
        if (owner == null) return;

        Opening?.Invoke(this, EventArgs.Empty);
        _sourceElement = element;
        _ownerWindow = owner;

        if (!_ownerWindow.Controls.Contains(this))
            _ownerWindow.Controls.Add(this);

        // Konumu ve boyutu belirle, sonra z-order'ı en üste çek.
        PositionDropDown(location);
        Visible = true;

        // WinForms z-order + SDUI'nin kendi ZOrder sistemini güncelle.
        BringToFront();
        if (_ownerWindow is UIWindow uiw)
            uiw.BringToFront(this);
        AttachHandlers();

        _fadeInAnimation.SetProgress(0);
        _fadeInAnimation.StartNewAnimation(AnimationDirection.In);

        _ownerWindow.Invalidate();
        _isOpen = true;
    }

    public void Show(Point location) => Show(null, location);

    public void Hide()
    {
        if (!_isOpen) return;

        Closing?.Invoke(this, EventArgs.Empty);
        DetachHandlers();
        Visible = false;
        _ownerWindow?.Invalidate();
        _ownerWindow = null;
        _sourceElement = null;
        _isOpen = false;
    }

    private void PositionDropDown(Point screenLocation)
    {
        if (_ownerWindow == null) return;

        var windowLocation = _ownerWindow.PointToClient(screenLocation);
        var size = GetPrefSize();
        var client = _ownerWindow.ClientRectangle;

        const int MARGIN = 8;
        int maxWidth = client.Width - MARGIN * 2;
        int maxHeight = client.Height - MARGIN * 2;

        size.Width = Math.Min(size.Width, maxWidth);
        size.Height = Math.Min(size.Height, maxHeight);

        int targetX = windowLocation.X;
        int targetY = windowLocation.Y;

        if (targetX + size.Width > client.Right - MARGIN)
        {
            int leftPos = targetX - size.Width - MARGIN;
            if (leftPos >= client.Left + MARGIN)
                targetX = leftPos;
            else
                targetX = client.Right - size.Width - MARGIN;
        }

        if (targetY + size.Height > client.Bottom - MARGIN)
        {
            int topPos = targetY - size.Height;
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

    private void OnOwnerMouseDown(object sender, MouseEventArgs e) { if (!_isOpen || !_autoClose) return; if (!Bounds.Contains(e.Location)) Hide(); }
    private void OnOwnerDeactivate(object sender, EventArgs e) { if (!_isOpen || !_autoClose) return; Hide(); }
    private void OnOwnerKeyDown(object sender, KeyEventArgs e) { if (!_isOpen || !_autoClose) return; if (e.KeyCode == Keys.Escape) { Hide(); e.Handled = true; } }

    private Size GetPrefSize()
    {
        const float MinContentWidth = 180f; // Minimum içerik genişliği
        
        // İçerik genişlik/yükseklik hesabı (shadow hariç)
        float contentWidth = ItemPadding * 2;
        float contentHeight = ItemPadding; // Üst padding
        
        foreach (var item in Items)
        {
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
        float totalWidth = contentWidth + ShadowMargin * 2;
        float totalHeight = contentHeight + ShadowMargin * 2;

        return new Size((int)Math.Ceiling(totalWidth), (int)Math.Ceiling(totalHeight));
    }

    internal override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        float y = ShadowMargin + ItemPadding;
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
            if (r.Contains(e.Location)) { _hoveredItem = item; break; }
            y += ItemHeight + ItemPadding;
        }
        Invalidate();
    }

    internal override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); _hoveredItem = null; Invalidate(); }

    private AnimationManager EnsureItemHoverAnim(MenuItem item)
    {
        if (!_itemHoverAnims.TryGetValue(item, out var engine))
        {
            engine = new AnimationManager { Increment = 0.25, AnimationType = AnimationType.EaseOut, Singular = true, InterruptAnimation = true };
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

        float fadeProgress = (float)_fadeInAnimation.GetProgress();
        byte fadeAlpha = (byte)(fadeProgress * 255);
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
        for (int i = 0; i < 2; i++)
        {
            float offsetY = 0.75f + i * 0.85f;
            byte shadowAlpha = (byte)((8 - i * 3) * fadeProgress);

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

        float y = contentRect.Top + ItemPadding;

        foreach (var item in Items)
        {
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

            bool isHovered = item == _hoveredItem;
            var anim = EnsureItemHoverAnim(item);

            if (isHovered) anim.StartNewAnimation(AnimationDirection.In);
            else anim.StartNewAnimation(AnimationDirection.Out);

            float hoverProgress = (float)anim.GetProgress();

            if (hoverProgress > 0.001f || isHovered)
            {
                byte hoverAlpha = (byte)(Math.Max(180, 180 + 70 * hoverProgress));
                _hoverPaint!.Color = HoverBackColor.ToSKColor().WithAlpha((byte)(fadeAlpha * hoverAlpha / 255f));
                canvas.DrawRoundRect(itemRect, 7, 7, _hoverPaint);
            }

            float textX = itemRect.Left + 12;

            if (ShowIcons && item.Icon != null)
            {
                var iconY = itemRect.Top + (ItemHeight - ImageScalingSize.Height) / 2;
                var iconBitmap = GetCachedIconBitmap(item.Icon);
                _iconPaint!.Color = SKColors.White.WithAlpha(fadeAlpha);
                canvas.DrawBitmap(iconBitmap, new SKRect(textX, iconY, textX + ImageScalingSize.Width, iconY + ImageScalingSize.Height), _iconPaint);
                textX += ImageScalingSize.Width + 8;
            }

            var hoverFore = !HoverForeColor.IsEmpty
                ? HoverForeColor
                : (HoverBackColor.IsEmpty ? MenuForeColor : HoverBackColor.Determine());
            var textColor = isHovered ? hoverFore : MenuForeColor;

            var font = GetDefaultSkFont();
            _textPaint!.Color = textColor.ToSKColor().WithAlpha(fadeAlpha);
            var textBounds = SKRect.Create(textX, itemRect.Top, itemRect.Right - textX, itemRect.Height);
            canvas.DrawControlText(item.Text, textBounds, _textPaint, font, ContentAlignment.MiddleLeft, false, true);

            if (ShowSubmenuArrow && item.HasDropDown)
            {
                float chevronSize = 5f;
                float chevronX = itemRect.Right - 14;
                float chevronY = itemRect.MidY;

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
        _borderPaint ??= new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1, FilterQuality = SKFilterQuality.High };
        _separatorPaint ??= new SKPaint { IsAntialias = true, StrokeWidth = 1 };
        _hoverPaint ??= new SKPaint { IsAntialias = true, FilterQuality = SKFilterQuality.High };
        _iconPaint ??= new SKPaint { IsAntialias = true, FilterQuality = SKFilterQuality.High };
        _textPaint ??= new SKPaint { IsAntialias = true };
        _arrowPaint ??= new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1.2f, StrokeCap = SKStrokeCap.Round, StrokeJoin = SKStrokeJoin.Round };
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

    private SKFont GetDefaultSkFont()
    {
        int dpi = DeviceDpi > 0 ? DeviceDpi : 96;
        if (_defaultSkFont == null || !ReferenceEquals(_defaultSkFontSource, Font) || _defaultSkFontDpi != dpi)
        {
            _defaultSkFont?.Dispose();
            _defaultSkFont = new SKFont
            {
                Size = Font.Size.PtToPx(this),
                Typeface = SDUI.Helpers.FontManager.GetSKTypeface(Font),
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

            for (int i = 0; i < _shadowPaints.Length; i++)
            {
                _shadowPaints[i]?.Dispose();
                _shadowPaints[i] = null;
            }
            for (int i = 0; i < _shadowMaskFilters.Length; i++)
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