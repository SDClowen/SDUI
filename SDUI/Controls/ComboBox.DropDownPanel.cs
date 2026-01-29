using SDUI.Animation;
using SDUI.Extensions;
using SDUI.Helpers;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SDUI.Controls;

public class DropDownPanel : UIElementBase
{
    protected int VERTICAL_PADDING => _owner.DropDownVerticalPadding;
    private int ITEM_MARGIN => (int)(10 * ScaleFactor);
    protected float CORNER_RADIUS => 10f * ScaleFactor;
    protected int SCROLL_BAR_WIDTH => (int)(14 * ScaleFactor);

    // Per-item hover animasyonlar�
    protected readonly Dictionary<int, AnimationManager> _itemHoverAnims = new();

    protected readonly AnimationManager _openAnimation = new()
    {
        Increment = 0.13,
        AnimationType = AnimationType.EaseOut,
        InterruptAnimation = true
    };

    protected readonly ComboBox _owner;
    protected readonly ScrollBar _scrollBar;
    protected readonly SKMaskFilter?[] _shadowMaskFilters = new SKMaskFilter?[4];

    protected readonly SKPaint?[] _shadowPaints = new SKPaint?[4];
    protected SKPaint? _bgPaint;
    protected SKPaint? _borderPaint;

    protected SKFont? _cachedFont;
    protected int _cachedFontDpi;
    protected Font? _cachedFontSource;
    protected SKPath? _clipPath;
    protected SKPaint? _highlightPaint;
    protected SKShader? _highlightShader;
    protected int _highlightShaderHeight;
    protected int _hoveredIndex = -1;
    protected SKPaint? _hoverPaint;
    protected bool _isClosing;

    protected SKPaint? _layerPaint;
    protected bool _openingUpwards;
    protected int _scrollOffset;
    protected int _selectedIndex = -1;
    protected SKPaint? _selectionPaint;
    protected SKPaint? _textPaint;
    protected int _visibleItemCount;

    public DropDownPanel(ComboBox owner)
    {
        _owner = owner;
        BackColor = Color.Transparent;
        Visible = false;
        TabStop = false;

        _scrollBar = new ScrollBar
        {
            Orientation = Orientation.Vertical,
            Visible = false,
            Minimum = 0,
            Maximum = 0,
            Value = 0,
            SmallChange = 1,
            LargeChange = 3,
            Thickness = 6,
            Radius = 6,
            AutoHide = true
        };
        _scrollBar.ValueChanged += ScrollBar_ValueChanged;
        Controls.Add(_scrollBar);

        _openAnimation.OnAnimationProgress += _ =>
        {
            Invalidate();
            var p = _openAnimation.GetProgress();
            if (_isClosing && p <= 0.001)
                Hide();
        };
    }

    public virtual int ItemHeight => Math.Max(32, _owner.ItemHeight);

    protected SKFont GetCachedFont()
    {
        var dpi = DeviceDpi > 0 ? DeviceDpi : 96;
        var font = _owner.Font;

        if (_cachedFont == null || !ReferenceEquals(_cachedFontSource, font) || _cachedFontDpi != dpi)
        {
            _cachedFont?.Dispose();
            _cachedFont = new SKFont
            {
                Size = font.Size.PtToPx(this),
                Typeface = FontManager.GetSKTypeface(font),
                Subpixel = true,
                Edging = SKFontEdging.SubpixelAntialias
            };
            _cachedFontSource = font;
            _cachedFontDpi = dpi;
        }

        return _cachedFont;
    }

    private void EnsureHighlightShader(int height)
    {
        if (_highlightShader != null && _highlightShaderHeight == height)
            return;

        _highlightShader?.Dispose();
        _highlightShader = SKShader.CreateLinearGradient(
            new SKPoint(0, 0),
            new SKPoint(0, height * 0.15f),
            new[] { SKColors.White.WithAlpha(12), SKColors.Transparent },
            null,
            SKShaderTileMode.Clamp);
        _highlightShaderHeight = height;
    }

    private void EnsureShadowResources()
    {
        for (var i = 0; i < 4; i++)
        {
            if (_shadowPaints[i] != null)
                continue;

            float blurRadius = 6 + i * 4;
            _shadowMaskFilters[i] = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, blurRadius);
            _shadowPaints[i] = new SKPaint
            {
                IsAntialias = true,
                MaskFilter = _shadowMaskFilters[i]
            };
        }
    }

    protected virtual void ScrollBar_ValueChanged(object sender, EventArgs e)
    {
        _scrollOffset = _scrollBar.Value;
        Invalidate();
    }

    protected AnimationManager EnsureItemAnim(int index)
    {
        if (!_itemHoverAnims.TryGetValue(index, out var ae))
        {
            ae = new AnimationManager()
            {
                Increment = 0.18,
                AnimationType = AnimationType.EaseInOut,
                InterruptAnimation = true
            };
            ae.OnAnimationProgress += _ => Invalidate();
            _itemHoverAnims[index] = ae;
        }

        return ae;
    }

    public virtual void ShowItems()
    {
        _hoveredIndex = -1;
        _selectedIndex = _owner.SelectedIndex;

        var totalItems = _owner.Items.Count;
        var maxVisibleItems = Math.Max(1, (Height - 2 * VERTICAL_PADDING) / ItemHeight);
        _visibleItemCount = Math.Min(totalItems, maxVisibleItems);

        var needsScrollBar = totalItems > maxVisibleItems;
        _scrollBar.Visible = needsScrollBar;

        if (needsScrollBar)
        {
            _scrollBar.Location = new Point(Width - SCROLL_BAR_WIDTH - 2, VERTICAL_PADDING);
            _scrollBar.Size = new Size(SCROLL_BAR_WIDTH, Height - 2 * VERTICAL_PADDING);
            _scrollBar.Maximum = Math.Max(0, totalItems - maxVisibleItems);
            _scrollBar.LargeChange = maxVisibleItems;
        }

        _scrollOffset = 0;
        _scrollBar.Value = 0;

        Invalidate();
    }

    public virtual void BeginOpen(bool openUpwards)
    {
        _openingUpwards = openUpwards;
        _isClosing = false;
        ShowItems();
        Visible = true;
        _openAnimation.SetProgress(0);
        _openAnimation.StartNewAnimation(AnimationDirection.In);
    }

    public virtual void BeginClose()
    {
        if (!Visible) return;
        _isClosing = true;
        _openAnimation.StartNewAnimation(AnimationDirection.Out);
    }

    internal override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        var newIndex = GetItemIndexAtPoint(e.Location);
        if (newIndex != _hoveredIndex)
        {
            if (_hoveredIndex >= 0)
                EnsureItemAnim(_hoveredIndex).StartNewAnimation(AnimationDirection.Out);

            _hoveredIndex = newIndex;
            if (_hoveredIndex >= 0)
                EnsureItemAnim(_hoveredIndex).StartNewAnimation(AnimationDirection.In);

            Invalidate();
        }
    }

    internal override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        if (_hoveredIndex >= 0)
            EnsureItemAnim(_hoveredIndex).StartNewAnimation(AnimationDirection.Out);
        _hoveredIndex = -1;
        Invalidate();
    }

    internal override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);
        if (!_scrollBar.Visible) return;
        var delta = e.Delta > 0 ? -1 : 1;
        var newValue = _scrollBar.Value + delta;
        newValue = Math.Max(_scrollBar.Minimum, Math.Min(_scrollBar.Maximum, newValue));
        _scrollBar.Value = newValue;
    }

    internal override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button == MouseButtons.Left)
        {
            var itemIndex = GetItemIndexAtPoint(e.Location);
            if (itemIndex >= 0 && itemIndex < _owner.Items.Count)
            {
                _owner.SelectedIndex = itemIndex;
                _owner.OnSelectionChangeCommitted(EventArgs.Empty);
                Hide();
            }
        }
    }

    public new void Hide()
    {
        Visible = false;
        Width = 0;
        Height = 0;
        _owner.DroppedDown = false;
        _owner._arrowAnimation.SetProgress(0);
        _owner.OnDropDownClosed(EventArgs.Empty);
        _owner.DetachWindowHandlers();
        Parent?.Controls.Remove(this);
    }

    protected virtual int GetItemIndexAtPoint(Point point)
    {
        if (point.Y < VERTICAL_PADDING || point.Y > Height - VERTICAL_PADDING)
            return -1;

        if (_scrollBar.Visible && point.X >= _scrollBar.Bounds.Left)
            return -1;

        var relativeY = point.Y - VERTICAL_PADDING;
        var itemIndex = relativeY / ItemHeight + _scrollOffset;

        return itemIndex >= 0 && itemIndex < _owner.Items.Count ? itemIndex : -1;
    }

    public override void OnPaint(SKCanvas canvas)
    {

        if (Width <= 0 || Height <= 0 || _owner.Items.Count == 0)
            return;

        var openProgress = (float)_openAnimation.GetProgress();
        if (!Visible) openProgress = 0;

        _layerPaint ??= new SKPaint { IsAntialias = true };
        _layerPaint.Color = SKColors.White.WithAlpha((byte)(255 * openProgress));
        canvas.SaveLayer(_layerPaint);

        // Subtle fade-in animasyonu
        var translateY = (_openingUpwards ? 1f - openProgress : openProgress - 1f) * 8f;
        canvas.Translate(0, translateY);

        var mainRect = new SKRect(0, 0, Width, Height);
        var mainRoundRect = new SKRoundRect(mainRect, CORNER_RADIUS);

        // Multi-layer modern shadow (ContextMenuStrip gibi)
        canvas.Save();
        EnsureShadowResources();
        for (var i = 0; i < 4; i++)
        {
            float offsetY = 2 + i * 2;
            var shadowAlpha = (byte)((25 - i * 5) * openProgress);

            var shadowPaint = _shadowPaints[i]!;
            shadowPaint.Color = SKColors.Black.WithAlpha(shadowAlpha);

            canvas.Save();
            canvas.Translate(0, offsetY);
            canvas.DrawRoundRect(mainRoundRect, shadowPaint);
            canvas.Restore();
        }

        canvas.Restore();

        // High-quality solid background
        var surfaceColor = ColorScheme.BackColor.ToSKColor().InterpolateColor(SKColors.White, 0.06f);

        _bgPaint ??= new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            FilterQuality = SKFilterQuality.High
        };
        _bgPaint.Color = surfaceColor;
        canvas.DrawRoundRect(mainRoundRect, _bgPaint);

        // Clip path

        _clipPath ??= new SKPath();
        _clipPath.Reset();
        _clipPath.AddRoundRect(mainRoundRect);
        canvas.Save();
        canvas.ClipPath(_clipPath, antialias: true);

        // Minimal �st highlight

        EnsureHighlightShader(Height);
        _highlightPaint ??= new SKPaint { IsAntialias = true };
        _highlightPaint.Shader = _highlightShader;
        canvas.DrawRect(mainRect, _highlightPaint);

        // High-quality border

        _borderPaint ??= new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1f,
            FilterQuality = SKFilterQuality.High
        };
        _borderPaint.Color = ColorScheme.BorderColor.Alpha(100).ToSKColor();
        var borderRect = new SKRoundRect(
            new SKRect(0.5f, 0.5f, Width - 0.5f, Height - 0.5f),
            CORNER_RADIUS - 0.5f);
        canvas.DrawRoundRect(borderRect, _borderPaint);

        // High-quality text paint

        var font = GetCachedFont();
        _textPaint ??= new SKPaint
        {
            IsAntialias = true,
            FilterQuality = SKFilterQuality.High,
            Style = SKPaintStyle.Fill
        };

        float contentRightInset = _scrollBar.Visible ? SCROLL_BAR_WIDTH + 6 : 0;
        float currentY = VERTICAL_PADDING;
        var startIndex = _scrollOffset;
        var endIndex = Math.Min(_owner.Items.Count, startIndex + _visibleItemCount);

        for (var i = startIndex; i < endIndex && currentY < Height - VERTICAL_PADDING; i++)
        {
            // Item rect with proper margins from dropdown edges
            float itemLeftEdge = ITEM_MARGIN;
            var itemRightEdge = Width - ITEM_MARGIN - contentRightInset;
            var itemRect = new SKRect(
                itemLeftEdge,
                currentY,
                itemRightEdge,
                currentY + ItemHeight);

            var hoverAE = EnsureItemAnim(i);
            var hProg = (float)hoverAE.GetProgress();

            var isSelected = i == _selectedIndex;
            var itemRadius = 4f;

            // High-quality selection background
            if (isSelected)
            {
                _selectionPaint ??= new SKPaint
                {
                    IsAntialias = true,
                    FilterQuality = SKFilterQuality.High,
                    Style = SKPaintStyle.Fill
                };
                _selectionPaint.Color = ColorScheme.AccentColor.Alpha(70).ToSKColor();
                var selRect = new SKRoundRect(itemRect, itemRadius);
                canvas.DrawRoundRect(selRect, _selectionPaint);
            }
            // High-quality hover effect
            else if (hProg > 0.001f)
            {
                var hoverAlpha = (byte)(30 + 45 * hProg);
                _hoverPaint ??= new SKPaint
                {
                    IsAntialias = true,
                    FilterQuality = SKFilterQuality.High,
                    Style = SKPaintStyle.Fill
                };
                _hoverPaint.Color = ColorScheme.AccentColor.Alpha(hoverAlpha).ToSKColor();
                var hoverRect = new SKRoundRect(itemRect, itemRadius);
                canvas.DrawRoundRect(hoverRect, _hoverPaint);
            }

            // High-quality text rendering
            var text = _owner.GetItemText(_owner.Items[i]);
            _textPaint.Color = ColorScheme.ForeColor.ToSKColor();

            var baseTextY = currentY + ItemHeight / 2f - (font.Metrics.Ascent + font.Metrics.Descent) / 2f;
            float textX = ITEM_MARGIN + 8;

            TextRenderingHelper.DrawText(canvas, text, textX, baseTextY, font, _textPaint);

            currentY += ItemHeight;
        }

        canvas.Restore(); // clipPath restore
        canvas.Restore(); // layerPaint restore
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _cachedFont?.Dispose();
            _cachedFont = null;
            _cachedFontSource = null;

            _layerPaint?.Dispose();
            _bgPaint?.Dispose();
            _highlightPaint?.Dispose();
            _highlightShader?.Dispose();
            _borderPaint?.Dispose();
            _textPaint?.Dispose();
            _selectionPaint?.Dispose();
            _hoverPaint?.Dispose();
            _clipPath?.Dispose();

            for (var i = 0; i < _shadowPaints.Length; i++)
            {
                _shadowPaints[i]?.Dispose();
                _shadowPaints[i] = null;
                _shadowMaskFilters[i]?.Dispose();
                _shadowMaskFilters[i] = null;
            }
        }

        base.Dispose(disposing);
    }
}