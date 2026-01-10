using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using SDUI.Animation;
using SDUI.Extensions;
using SDUI.Helpers;
using SkiaSharp;
using CheckState = SDUI.Enums.CheckState;

namespace SDUI.Controls;

public class MenuStrip : UIElementBase
{
    private const int MaxIconCacheEntries = 256;
    private readonly Dictionary<Bitmap, SKBitmap> _iconCache = new();

    private readonly Dictionary<MenuItem, AnimationManager> _itemHoverAnims = new();

    // backing fields
    private readonly int _submenuAnimationDuration = 150;
    private readonly float _submenuArrowSize = 8f;
    private ContextMenuStrip _activeDropDown;
    private MenuItem _activeDropDownOwner;
    private float _animationProgress;
    private Timer _animationTimer;
    private SKPaint? _arrowPaint;

    private SKPaint? _bgPaint;
    private SKPaint? _bottomBorderPaint;
    private SKPaint? _checkPaint;
    private SKPath? _checkPath;
    private SKPath? _chevronPath;
    private float _cornerRadius = 6f;
    private SKFont? _defaultSkFont;
    private int _defaultSkFontDpi;
    private Font? _defaultSkFontSource;
    private Color _hoverBackColor = Color.FromArgb(62, 62, 62);
    private SKPaint? _hoverBgPaint;
    private MenuItem _hoveredItem;
    private Color _hoverForeColor = Color.FromArgb(0, 122, 204);
    private float _iconSize = 16f;
    private Size _imageScalingSize = new(20, 20);
    private SKPaint? _imgPaint;
    private bool _isAnimating;
    private float _itemHeight = 24f;
    private float _itemPadding = 4f; // temel boşluk, ekstra spacing sınırlanacak
    private Color _menuBackColor = Color.FromArgb(45, 45, 45);
    private Color _menuForeColor = Color.White;
    private MenuItem _openedItem;
    private Orientation _orientation = Orientation.Horizontal;
    private bool _roundedCorners = true;
    private Color _separatorBackColor = Color.FromArgb(50, 50, 50);
    private Color _separatorColor = Color.FromArgb(80, 80, 80);
    private Color _separatorForeColor = Color.FromArgb(100, 100, 100);
    private float _separatorHeight = 1f;
    private float _separatorMargin = 4f;
    private bool _showCheckMargin = true;
    private bool _showHoverEffect = true;
    private bool _showIcons = true;
    private bool _showImageMargin = true;
    private bool _showSubmenuArrow = true;
    private bool _stretch = true;
    private Color _submenuBackColor = Color.FromArgb(37, 37, 37);
    private Color _submenuBorderColor = Color.FromArgb(80, 80, 80);
    private float _submenuCornerRadius = 8f;
    private float _submenuOffset = 2f;
    private SKPaint? _textPaint;

    public MenuStrip()
    {
        Height = (int)_itemHeight;
        BackColor = ColorScheme.BackColor;
        ForeColor = ColorScheme.ForeColor;
        InitializeAnimationTimer();
    }

    [Browsable(false)] public List<MenuItem> Items { get; } = new();

    [Category("Behavior")]
    [DefaultValue(true)]
    public bool Stretch
    {
        get => _stretch;
        set
        {
            if (_stretch == value) return;
            _stretch = value;
            Invalidate();
        }
    }

    [Category("Appearance")]
    [DefaultValue(Orientation.Horizontal)]
    public Orientation Orientation
    {
        get => _orientation;
        set
        {
            if (_orientation == value) return;
            _orientation = value;
            Invalidate();
        }
    }

    [Category("Appearance")]
    [DefaultValue(typeof(Size), "20, 20")]
    public Size ImageScalingSize
    {
        get => _imageScalingSize;
        set
        {
            if (_imageScalingSize == value) return;
            _imageScalingSize = value;
            _iconSize = Math.Min(value.Width, value.Height);
            Invalidate();
        }
    }

    [Category("Behavior")]
    [DefaultValue(true)]
    public bool ShowSubmenuArrow
    {
        get => _showSubmenuArrow;
        set
        {
            if (_showSubmenuArrow == value) return;
            _showSubmenuArrow = value;
            Invalidate();
        }
    }

    [Category("Appearance")]
    public Color MenuBackColor
    {
        get => _menuBackColor;
        set
        {
            if (_menuBackColor == value) return;
            _menuBackColor = value;
            Invalidate();
        }
    }

    [Category("Appearance")]
    public Color MenuForeColor
    {
        get => _menuForeColor;
        set
        {
            if (_menuForeColor == value) return;
            _menuForeColor = value;
            Invalidate();
        }
    }

    [Category("Appearance")]
    public Color HoverBackColor
    {
        get => _hoverBackColor;
        set
        {
            if (_hoverBackColor == value) return;
            _hoverBackColor = value;
            Invalidate();
        }
    }

    [Category("Appearance")]
    public Color HoverForeColor
    {
        get => _hoverForeColor;
        set
        {
            if (_hoverForeColor == value) return;
            _hoverForeColor = value;
            Invalidate();
        }
    }

    [Category("Appearance")]
    public Color SubmenuBackColor
    {
        get => _submenuBackColor;
        set
        {
            if (_submenuBackColor == value) return;
            _submenuBackColor = value;
            Invalidate();
        }
    }

    [Category("Appearance")]
    public Color SubmenuBorderColor
    {
        get => _submenuBorderColor;
        set
        {
            if (_submenuBorderColor == value) return;
            _submenuBorderColor = value;
            Invalidate();
        }
    }

    [Category("Appearance")]
    public Color SeparatorColor
    {
        get => _separatorColor;
        set
        {
            if (_separatorColor == value) return;
            _separatorColor = value;
            Invalidate();
        }
    }

    [Category("Layout")]
    public float ItemHeight
    {
        get => _itemHeight;
        set
        {
            if (_itemHeight == value) return;
            _itemHeight = value;
            Height = (int)value;
            Invalidate();
        }
    }

    [Category("Layout")]
    public float ItemPadding
    {
        get => _itemPadding;
        set
        {
            if (_itemPadding == value) return;
            _itemPadding = value;
            Invalidate();
        }
    }

    [Category("Appearance")]
    public bool ShowIcons
    {
        get => _showIcons;
        set
        {
            if (_showIcons == value) return;
            _showIcons = value;
            Invalidate();
        }
    }

    [Category("Behavior")]
    public bool ShowHoverEffect
    {
        get => _showHoverEffect;
        set
        {
            if (_showHoverEffect == value) return;
            _showHoverEffect = value;
            Invalidate();
        }
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

    [Category("Appearance")]
    public bool RoundedCorners
    {
        get => _roundedCorners;
        set
        {
            if (_roundedCorners == value) return;
            _roundedCorners = value;
            Invalidate();
        }
    }

    [Category("Appearance")]
    public Color SeparatorBackColor
    {
        get => _separatorBackColor;
        set
        {
            if (_separatorBackColor == value) return;
            _separatorBackColor = value;
            Invalidate();
        }
    }

    [Category("Appearance")]
    public Color SeparatorForeColor
    {
        get => _separatorForeColor;
        set
        {
            if (_separatorForeColor == value) return;
            _separatorForeColor = value;
            Invalidate();
        }
    }

    [Category("Layout")]
    public float SeparatorMargin
    {
        get => _separatorMargin;
        set
        {
            if (_separatorMargin == value) return;
            _separatorMargin = value;
            Invalidate();
        }
    }

    protected override void InvalidateFontCache()
    {
        base.InvalidateFontCache();
        _defaultSkFont?.Dispose();
        _defaultSkFont = null;
        _defaultSkFontSource = null;
        _defaultSkFontDpi = 0;
    }

    private SKFont GetDefaultSkFont()
    {
        var dpi = DeviceDpi > 0 ? DeviceDpi : 96;
        var font = Font;
        if (_defaultSkFont == null || !ReferenceEquals(_defaultSkFontSource, font) || _defaultSkFontDpi != dpi)
        {
            _defaultSkFont?.Dispose();
            _defaultSkFont = new SKFont
            {
                Size = font.Size.PtToPx(this),
                Typeface = FontManager.GetSKTypeface(font),
                Subpixel = true,
                Edging = SKFontEdging.SubpixelAntialias
            };
            _defaultSkFontSource = font;
            _defaultSkFontDpi = dpi;
        }

        return _defaultSkFont;
    }

    private SKBitmap GetCachedIconBitmap(Bitmap icon)
    {
        if (_iconCache.TryGetValue(icon, out var cached))
            return cached;

        if (_iconCache.Count >= MaxIconCacheEntries)
        {
            foreach (var kvp in _iconCache)
                kvp.Value.Dispose();
            _iconCache.Clear();
        }

        var skBitmap = icon.ToSKBitmap();
        _iconCache[icon] = skBitmap;
        return skBitmap;
    }

    private void InitializeAnimationTimer()
    {
        _animationTimer = new Timer { Interval = 16 };
        _animationTimer.Tick += AnimationTimer_Tick;
    }

    private void AnimationTimer_Tick(object sender, EventArgs e)
    {
        if (!_isAnimating)
            return;

        _animationProgress = Math.Min(1f, _animationProgress + 16f / _submenuAnimationDuration);

        if (_animationProgress >= 1f)
        {
            _isAnimating = false;
            _animationTimer.Stop();
        }

        Invalidate();
    }

    public void AddItem(MenuItem item)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));
        Items.Add(item);
        item.Parent = this;
        Invalidate();
    }

    public void RemoveItem(MenuItem item)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));
        if (Items.Remove(item))
        {
            item.Parent = null;
            Invalidate();
        }
    }

    public override void OnPaint(SKPaintSurfaceEventArgs e)
    {
        base.OnPaint(e);
        var canvas = e.Surface.Canvas;
        var bounds = ClientRectangle;

        // Flat modern background
        _bgPaint ??= new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
        _bgPaint.Color = MenuBackColor.ToSKColor();
        canvas.DrawRect(new SKRect(0, 0, bounds.Width, bounds.Height), _bgPaint);

        // Subtle bottom border
        _bottomBorderPaint ??= new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1f };
        _bottomBorderPaint.Color = SeparatorColor.ToSKColor().WithAlpha(100);
        canvas.DrawLine(0, bounds.Height - 1, bounds.Width, bounds.Height - 1, _bottomBorderPaint);

        if (Orientation == Orientation.Horizontal)
        {
            // Use ComputeItemRects to respect visibility and spacing logic
            var rects = ComputeItemRects();
            var rectIndex = 0;

            for (var i = 0; i < Items.Count; i++)
            {
                var item = Items[i];
                if (!item.Visible) continue;

                if (rectIndex >= rects.Count) break;
                var rf = rects[rectIndex++];
                var r = new SKRect(rf.Left, rf.Top, rf.Right, rf.Bottom);
                DrawMenuItem(canvas, item, r);
            }
        }
        else
        {
            var y = ItemPadding;
            for (var i = 0; i < Items.Count; i++)
            {
                var item = Items[i];
                var w = bounds.Width - ItemPadding * 2;
                var r = new SKRect(ItemPadding, y, ItemPadding + w, y + ItemHeight);
                DrawMenuItem(canvas, item, r);
                y += ItemHeight + ItemPadding;
            }
        }

        if (_activeDropDown == null || !_activeDropDown.IsOpen) _activeDropDownOwner = null;
    }

    private void DrawMenuItem(SKCanvas c, MenuItem item, SKRect bounds)
    {
        var hover = item == _hoveredItem || item == _openedItem;
        var anim = EnsureHoverAnim(item);

        if (hover)
            anim.StartNewAnimation(AnimationDirection.In);
        else
            anim.StartNewAnimation(AnimationDirection.Out);

        var prog = (float)anim.GetProgress();

        // High-quality hover background with proper anti-aliasing
        if (ShowHoverEffect && hover)
        {
            var blend = _hoverBackColor.ToSKColor();
            var alpha = (byte)(180 + 70 * prog);
            _hoverBgPaint ??= new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                FilterQuality = SKFilterQuality.High
            };
            _hoverBgPaint.Color = blend.WithAlpha(alpha);
            var rr = new SKRoundRect(bounds, 5);
            c.DrawRoundRect(rr, _hoverBgPaint);
        }

        var tx = bounds.Left + 10;

        // Checkmark area (left margin for checkbox/radio)
        float checkAreaWidth = 20;
        if (item.CheckState != CheckState.Unchecked || item.Icon != null)
        {
            var checkX = bounds.Left + 8;
            var checkY = bounds.MidY;
            var checkSize = 12f;

            if (item.CheckState == CheckState.Checked)
            {
                // Draw checkmark (✓)
                _checkPaint ??= new SKPaint
                {
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 1.8f,
                    StrokeCap = SKStrokeCap.Round,
                    StrokeJoin = SKStrokeJoin.Round
                };
                _checkPaint.Color = MenuForeColor.ToSKColor();

                _checkPath ??= new SKPath();
                _checkPath.Reset();
                _checkPath.MoveTo(checkX, checkY);
                _checkPath.LineTo(checkX + checkSize * 0.35f, checkY + checkSize * 0.35f);
                _checkPath.LineTo(checkX + checkSize, checkY - checkSize * 0.4f);
                c.DrawPath(_checkPath, _checkPaint);
            }
            else if (item.CheckState == CheckState.Indeterminate)
            {
                // Draw indeterminate box (filled square)
                _checkPaint ??= new SKPaint
                {
                    IsAntialias = true,
                    Style = SKPaintStyle.Fill
                };
                _checkPaint.Color = MenuForeColor.ToSKColor().WithAlpha(128);

                var boxSize = 8f;
                var boxRect = new SKRect(
                    checkX,
                    checkY - boxSize / 2,
                    checkX + boxSize,
                    checkY + boxSize / 2
                );
                c.DrawRect(boxRect, _checkPaint);
            }

            tx += checkAreaWidth;
        }

        // Icon
        if (ShowIcons && item.Icon != null)
        {
            var iy = bounds.Top + (_itemHeight - _iconSize) / 2;
            _imgPaint ??= new SKPaint
            {
                IsAntialias = true,
                FilterQuality = SKFilterQuality.High
            };
            var skBitmap = GetCachedIconBitmap(item.Icon);
            c.DrawBitmap(skBitmap, new SKRect(bounds.Left + 8, iy, bounds.Left + 8 + _iconSize, iy + _iconSize),
                _imgPaint);
            tx += _iconSize + 6;
        }

        // Text with high quality
        var hoverFore = !HoverForeColor.IsEmpty
            ? HoverForeColor
            : HoverBackColor.IsEmpty
                ? MenuForeColor
                : HoverBackColor.Determine();
        var textColor = hover ? hoverFore : MenuForeColor;

        var font = GetDefaultSkFont();
        _textPaint ??= new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
        _textPaint.Color = textColor.ToSKColor();
        var drawBounds = SKRect.Create(tx, bounds.Top, bounds.Right - tx, bounds.Height);
        c.DrawControlText(item.Text, drawBounds, _textPaint, font, ContentAlignment.MiddleLeft, false, true);

        // Measure text width for arrow positioning
        var textBounds = new SKRect();
        font.MeasureText(item.Text.Replace("&", ""), out textBounds);

        // WinUI3 style chevron arrow with high quality
        if (ShowSubmenuArrow && item.HasDropDown)
        {
            _arrowPaint ??= new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1.2f,
                StrokeCap = SKStrokeCap.Round,
                StrokeJoin = SKStrokeJoin.Round
            };
            _arrowPaint.Color = textColor.ToSKColor();

            var chevronSize = 5f;
            float arrowX;
            var arrowY = bounds.MidY;

            // Check if this is a ContextMenuStrip instance (not MenuStrip with vertical orientation)
            var isContextMenu = GetType() == typeof(ContextMenuStrip);

            if (Orientation == Orientation.Vertical || isContextMenu)
            {
                // Vertical: chevron at the right edge
                arrowX = bounds.Right - 14;

                _chevronPath ??= new SKPath();
                _chevronPath.Reset();
                _chevronPath.MoveTo(arrowX - chevronSize, arrowY - chevronSize);
                _chevronPath.LineTo(arrowX - chevronSize / 2, arrowY);
                _chevronPath.LineTo(arrowX - chevronSize, arrowY + chevronSize);
                c.DrawPath(_chevronPath, _arrowPaint);
            }
            else
            {
                // Horizontal: chevron after text
                arrowX = tx + textBounds.Width + 10;

                _chevronPath ??= new SKPath();
                _chevronPath.Reset();
                _chevronPath.MoveTo(arrowX - chevronSize, arrowY - chevronSize / 2);
                _chevronPath.LineTo(arrowX, arrowY + chevronSize / 2);
                _chevronPath.LineTo(arrowX + chevronSize, arrowY - chevronSize / 2);
                c.DrawPath(_chevronPath, _arrowPaint);
            }
        }
    }

    private List<RectangleF> ComputeItemRects()
    {
        var rects = new List<RectangleF>(Items.Count);
        var b = ClientRectangle;

        if (Orientation == Orientation.Horizontal)
        {
            var x = ItemPadding;
            var available = b.Width - ItemPadding * 2;
            float total = 0;
            var widths = new float[Items.Count];

            for (var i = 0; i < Items.Count; i++)
            {
                widths[i] = MeasureItemWidth(Items[i]);
                total += widths[i];
                if (i < Items.Count - 1)
                    total += ItemPadding;
            }

            float extra = 0;
            if (Stretch && Items.Count > 1 && total < available)
            {
                var rawExtra = (available - total) / (Items.Count - 1);
                var maxExtraPerGap = ItemPadding;
                extra = Math.Min(rawExtra, maxExtraPerGap);
            }

            for (var i = 0; i < Items.Count; i++)
            {
                var w = widths[i];
                rects.Add(new RectangleF(x, 0, w, ItemHeight));
                x += w + ItemPadding + (i < Items.Count - 1 ? extra : 0);
            }
        }
        else
        {
            // Dikey menüler ve ContextMenuStrip için; separator'lar da
            // ContextMenuStrip'teki satır yerleşimi ile aynı mantığı kullanmalı ki
            // hover alanı ile çizim hizalı olsun.
            var margin = this is ContextMenuStrip ? ContextMenuStrip.ShadowMargin : 0f;
            var y = margin + ItemPadding;
            var w = b.Width - margin * 2 - ItemPadding * 2;
            var x = margin + ItemPadding;

            for (var i = 0; i < Items.Count; i++)
            {
                var item = Items[i];

                // Skip invisible items when computing rects
                if (!item.Visible) continue;

                if (item.IsSeparator)
                {
                    // İnce çizgi için küçük bir satır yüksekliği ayırıyoruz.
                    var sepHeight = SeparatorMargin * 2 + 1;
                    rects.Add(new RectangleF(x, y, w, sepHeight));
                    y += sepHeight + ItemPadding;
                    continue;
                }

                rects.Add(new RectangleF(x, y, w, ItemHeight));
                y += ItemHeight + ItemPadding;
            }
        }

        return rects;
    }

    private SKRect GetItemBounds(MenuItem item)
    {
        var rects = ComputeItemRects();
        for (var i = 0; i < Items.Count; i++)
            if (Items[i] == item)
            {
                var r = rects[i];
                return new SKRect(r.Left, r.Top, r.Right, r.Bottom);
            }

        return SKRect.Empty;
    }

    internal override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        var rects = ComputeItemRects();
        MenuItem hovered = null;
        for (var i = 0; i < Items.Count; i++)
            if (rects[i].Contains(e.Location))
            {
                hovered = Items[i];
                break;
            }

        if (_hoveredItem != hovered)
        {
            _hoveredItem = hovered;
            if (_hoveredItem?.HasDropDown == true && _openedItem != _hoveredItem) OpenSubmenu(_hoveredItem);
            Invalidate();
        }
    }

    internal override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button != MouseButtons.Left) return;
        var rects = ComputeItemRects();
        for (var i = 0; i < Items.Count; i++)
            if (rects[i].Contains(e.Location))
            {
                var item = Items[i];
                if (item.HasDropDown)
                {
                    if (_openedItem == item)
                    {
                        /*keep*/
                    }
                    else
                    {
                        OpenSubmenu(item);
                    }
                }
                else
                {
                    item.OnClick();
                    CloseSubmenu();
                }

                return;
            }

        CloseSubmenu();
    }

    internal override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _hoveredItem = null;
        Invalidate();
    }

    private void OpenSubmenu(MenuItem item)
    {
        if (!item.HasDropDown)
        {
            CloseSubmenu();
            return;
        }

        if (_activeDropDownOwner == item && _activeDropDown != null && _activeDropDown.IsOpen) return;

        CloseSubmenu();
        EnsureDropDownHost();
        _activeDropDown.Items.Clear();

        foreach (var child in item.DropDownItems)
            _activeDropDown.AddItem(CloneMenuItem(child));

        SyncDropDownAppearance();

        var itemBounds = GetItemBounds(item);
        Point screenPoint;
        var vertical = Orientation == Orientation.Vertical || this is ContextMenuStrip;

        if (vertical)
        {
            // Cascading submenus must align by the *content* edge, not the shadow bounds.
            // ContextMenuStrip's background starts at ShadowMargin, so compensate here.
            var shadow = _activeDropDown is ContextMenuStrip ? ContextMenuStrip.ShadowMargin : 0f;

            // Parent content right edge in local coords.
            var parentContentRight = this is ContextMenuStrip
                ? Width - ContextMenuStrip.ShadowMargin
                : itemBounds.Right;

            // Desired child background top-left (local coords), with 1px overlap to avoid seams.
            var desiredBgLeft = parentContentRight - 1f;
            var desiredBgTop = itemBounds.Top;

            // Convert desired background origin to dropdown's control origin (includes shadow space).
            var targetX = (int)Math.Round(desiredBgLeft - shadow);
            var targetY = (int)Math.Round(desiredBgTop - shadow);
            screenPoint = PointToScreen(new Point(targetX, targetY));
        }
        else
        {
            // Below, left aligned - check if docked at bottom
            var shadow = _activeDropDown is ContextMenuStrip ? ContextMenuStrip.ShadowMargin : 0f;

            var targetX = (int)Math.Round(itemBounds.Left - shadow);
            int targetY;

            // If MenuStrip is docked at bottom, open upwards
            if (Dock == DockStyle.Bottom)
            {
                var popupSize = _activeDropDown.MeasurePreferredSize();
                // Keep background aligned even though popup includes shadow space.
                targetY = (int)Math.Round(itemBounds.Top - popupSize.Height - 4 + shadow);
            }
            else
            {
                targetY = (int)Math.Round(itemBounds.Bottom + 4 - shadow);
            }

            screenPoint = PointToScreen(new Point(targetX, targetY));
        }

        _activeDropDownOwner = item;
        _openedItem = item;

        // Initialize dropdown DPI from parent before showing
        if (_activeDropDown != null) _activeDropDown.InitializeDpi(DeviceDpi);

        _activeDropDown.Show(this, screenPoint);

        // İlk açılışta da her zaman en üst z-index'te olsun.
        if (FindForm() is UIWindow uiw)
        {
            uiw.BringToFront(_activeDropDown);

            // Re-assert top z-order after current message loop to avoid
            // first-show draw races where popup may appear behind other elements.
            try
            {
                uiw.BeginInvoke((Action)(() =>
                {
                    try
                    {
                        _activeDropDown?.BringToFront();
                        uiw.BringToFront(_activeDropDown);
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

        Invalidate();
    }

    private void CloseSubmenu()
    {
        if (_activeDropDown != null && _activeDropDown.IsOpen) _activeDropDown.Hide();
        _openedItem = null;
        _activeDropDownOwner = null;
        Invalidate();
    }

    private void EnsureDropDownHost()
    {
        if (_activeDropDown != null) return;
        _activeDropDown = new ContextMenuStrip { AutoClose = true, Dock = DockStyle.None };
        _activeDropDown.Opening += (_, _) => SyncDropDownAppearance();
        _activeDropDown.Closing += (_, _) =>
        {
            _openedItem = null;
            _activeDropDownOwner = null;
            Invalidate();
        };
    }

    private void SyncDropDownAppearance()
    {
        if (_activeDropDown == null) return;
        _activeDropDown.MenuBackColor = SubmenuBackColor;
        _activeDropDown.MenuForeColor = MenuForeColor;
        _activeDropDown.HoverBackColor = HoverBackColor;
        _activeDropDown.HoverForeColor = HoverForeColor;
        _activeDropDown.SubmenuBackColor = SubmenuBackColor;
        _activeDropDown.SeparatorColor = SeparatorColor;
        _activeDropDown.RoundedCorners = RoundedCorners;
        _activeDropDown.ItemPadding = Math.Max(ItemPadding, 6f);
        _activeDropDown.Orientation = Orientation.Vertical;
        _activeDropDown.ImageScalingSize = ImageScalingSize;
        _activeDropDown.ShowSubmenuArrow = ShowSubmenuArrow;
        _activeDropDown.ShowIcons = ShowIcons;
        _activeDropDown.ShowCheckMargin = ShowCheckMargin;
        _activeDropDown.ShowImageMargin = ShowImageMargin;
    }

    private MenuItem CloneMenuItem(MenuItem source)
    {
        if (source is MenuItemSeparator separator)
        {
            var cloneSeparator = new MenuItemSeparator
            {
                Height = separator.Height, Margin = separator.Margin, LineColor = separator.LineColor,
                ShadowColor = separator.ShadowColor
            };
            return cloneSeparator;
        }

        var clone = new MenuItem
        {
            Text = source.Text, Icon = source.Icon, Image = source.Image, ShortcutKeys = source.ShortcutKeys,
            ShowSubmenuArrow = source.ShowSubmenuArrow, ForeColor = source.ForeColor, BackColor = source.BackColor,
            Enabled = source.Enabled, Visible = source.Visible, Font = source.Font, AutoSize = source.AutoSize,
            Padding = source.Padding, Tag = source.Tag, Checked = source.Checked
        };
        foreach (var child in source.DropDownItems) clone.AddDropDownItem(CloneMenuItem(child));
        clone.Click += (_, _) =>
        {
            source.OnClick();
            _activeDropDown?.Hide();
        };
        return clone;
    }

    protected float MeasureItemWidth(MenuItem item)
    {
        if (item is MenuItemSeparator) return 20f;

        var font = GetDefaultSkFont();

        var tb = new SKRect();
        font.MeasureText(item.Text, out tb);
        var w = tb.Width;

        if (ShowIcons && item.Icon != null)
            w += _iconSize + 4;

        if (ShowSubmenuArrow && item.HasDropDown)
            w += _submenuArrowSize + 8; // Space for chevron

        // Avoid excessive trailing padding; MenuStrip already has ItemPadding spacing.
        return w + 12;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_animationTimer != null)
            {
                _animationTimer.Stop();
                _animationTimer.Tick -= AnimationTimer_Tick;
                _animationTimer.Dispose();
            }

            foreach (var anim in _itemHoverAnims.Values)
                anim?.Dispose();
            _itemHoverAnims.Clear();

            _activeDropDown?.Dispose();

            foreach (var kvp in _iconCache)
                kvp.Value?.Dispose();
            _iconCache.Clear();

            _bgPaint?.Dispose();
            _bottomBorderPaint?.Dispose();
            _hoverBgPaint?.Dispose();
            _imgPaint?.Dispose();
            _textPaint?.Dispose();
            _arrowPaint?.Dispose();
            _chevronPath?.Dispose();
            _defaultSkFont?.Dispose();
        }

        base.Dispose(disposing);
    }

    private AnimationManager EnsureHoverAnim(MenuItem item)
    {
        if (!_itemHoverAnims.TryGetValue(item, out var engine))
        {
            engine = new AnimationManager()
                { Increment = 0.28, AnimationType = AnimationType.EaseOut, InterruptAnimation = true };
            engine.OnAnimationProgress += _ => Invalidate();
            _itemHoverAnims[item] = engine;
        }

        return engine;
    }
}