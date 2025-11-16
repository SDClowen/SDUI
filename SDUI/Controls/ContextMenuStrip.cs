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
    private UIElementBase _sourceElement;
    private bool _isOpen;
    private UIWindow _ownerWindow;
    private bool _autoClose = true;
    private MouseEventHandler _ownerMouseDownHandler;
    private EventHandler _ownerDeactivateHandler;
    private KeyEventHandler _ownerKeyDownHandler;
    private bool _ownerPreviousKeyPreview;
    private MenuItem _hoveredItem;
    private readonly Animation.AnimationEngine _fadeInAnimation;
    private readonly Dictionary<MenuItem, Animation.AnimationEngine> _itemHoverAnims = new();

    public event EventHandler Opening;
    public event EventHandler Closing;

    public ContextMenuStrip()
    {
        Visible = false;
        AutoSize = false;
        TabStop = false;
        Orientation = SDUI.Orientation.Vertical;

        _fadeInAnimation = new Animation.AnimationEngine
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

        Visible = true;
        BringToFront();
        PositionDropDown(location);
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
        float width = ItemPadding * 2;
        float height = ItemPadding;
        foreach (var item in Items)
        {
            if (item.IsSeparator)
                height += SeparatorMargin * 2 + 1;
            else
            {
                width = Math.Max(width, MeasureItemWidth(item) + ItemPadding * 2);
                height += ItemHeight + ItemPadding;
            }
        }
        return new Size((int)width, (int)height);
    }

    internal override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        float y = ItemPadding;
        _hoveredItem = null;
        foreach (var item in Items)
        {
            if (item.IsSeparator) { y += SeparatorMargin * 2 + 1 + ItemPadding; continue; }
            var w = Width - ItemPadding * 2;
            var r = new RectangleF(ItemPadding, y, w, ItemHeight);
            if (r.Contains(e.Location)) { _hoveredItem = item; break; }
            y += ItemHeight + ItemPadding;
        }
        Invalidate();
    }

    internal override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); _hoveredItem = null; Invalidate(); }

    private Animation.AnimationEngine EnsureItemHoverAnim(MenuItem item)
    {
        if (!_itemHoverAnims.TryGetValue(item, out var engine))
        {
            engine = new Animation.AnimationEngine { Increment = 0.25, AnimationType = AnimationType.EaseOut, Singular = true, InterruptAnimation = true };
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

        float fadeProgress = (float)_fadeInAnimation.GetProgress();
        byte fadeAlpha = (byte)(fadeProgress * 255);
        const float CORNER_RADIUS = 10f;

        // Multi-layer shadow system
        canvas.Save();
        for (int i = 0; i < 4; i++)
        {
            float offsetY = 2 + i * 2;
            float blurRadius = 6 + i * 4;
            byte shadowAlpha = (byte)((25 - i * 5) * fadeProgress);

            using var shadowPaint = new SKPaint
            {
                Color = SKColors.Black.WithAlpha(shadowAlpha),
                MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, blurRadius),
                IsAntialias = true
            };

            canvas.Save();
            canvas.Translate(0, offsetY);
            canvas.DrawRoundRect(new SKRect(0, 0, bounds.Width, bounds.Height), CORNER_RADIUS, CORNER_RADIUS, shadowPaint);
            canvas.Restore();
        }
        canvas.Restore();

        // High-quality background
        using (var bgPaint = new SKPaint { Color = MenuBackColor.ToSKColor().WithAlpha(fadeAlpha), IsAntialias = true, FilterQuality = SKFilterQuality.High })
        {
            canvas.DrawRoundRect(new SKRect(0, 0, bounds.Width, bounds.Height), CORNER_RADIUS, CORNER_RADIUS, bgPaint);
        }

        // Border
        using (var borderPaint = new SKPaint { Color = SeparatorColor.ToSKColor().WithAlpha((byte)(fadeAlpha * 0.35f)), Style = SKPaintStyle.Stroke, StrokeWidth = 1, IsAntialias = true, FilterQuality = SKFilterQuality.High })
        {
            var borderRect = new SKRect(0.5f, 0.5f, bounds.Width - 0.5f, bounds.Height - 0.5f);
            canvas.DrawRoundRect(borderRect, CORNER_RADIUS, CORNER_RADIUS, borderPaint);
        }

        float y = ItemPadding;

        foreach (var item in Items)
        {
            if (item.IsSeparator)
            {
                using var sepPaint = new SKPaint { Color = SeparatorColor.ToSKColor().WithAlpha(fadeAlpha), IsAntialias = true, StrokeWidth = 1 };
                canvas.DrawLine(ItemPadding + 8, y + SeparatorMargin, bounds.Width - ItemPadding - 8, y + SeparatorMargin, sepPaint);
                y += SeparatorMargin * 2 + 1 + ItemPadding;
                continue;
            }

            var w = bounds.Width - ItemPadding * 2;
            var itemRect = new SKRect(ItemPadding, y, ItemPadding + w, y + ItemHeight);

            bool isHovered = item == _hoveredItem;
            var anim = EnsureItemHoverAnim(item);

            if (isHovered) anim.StartNewAnimation(AnimationDirection.In);
            else anim.StartNewAnimation(AnimationDirection.Out);

            float hoverProgress = (float)anim.GetProgress();

            if (hoverProgress > 0.001f || isHovered)
            {
                byte hoverAlpha = (byte)(Math.Max(180, 180 + 70 * hoverProgress));
                using var hoverPaint = new SKPaint { Color = HoverBackColor.ToSKColor().WithAlpha((byte)(fadeAlpha * hoverAlpha / 255f)), IsAntialias = true, FilterQuality = SKFilterQuality.High };
                canvas.DrawRoundRect(itemRect, 7, 7, hoverPaint);
            }

            float textX = itemRect.Left + 12;

            if (ShowIcons && item.Icon != null)
            {
                var iconY = itemRect.Top + (ItemHeight - ImageScalingSize.Height) / 2;
                using var iconImage = SKImage.FromBitmap(item.Icon.ToSKBitmap());
                using var iconPaint = new SKPaint { IsAntialias = true, FilterQuality = SKFilterQuality.High, Color = SKColors.White.WithAlpha(fadeAlpha) };
                canvas.DrawImage(iconImage, new SKRect(textX, iconY, textX + ImageScalingSize.Width, iconY + ImageScalingSize.Height), iconPaint);
                textX += ImageScalingSize.Width + 8;
            }

            var textColor = isHovered ? MenuForeColor.BlendWith(HoverForeColor, 0.6f) : MenuForeColor;

            using (var textPaint = new SKPaint { Color = textColor.ToSKColor().WithAlpha(fadeAlpha), TextSize = Font.Size.PtToPx(this), Typeface = SKTypeface.FromFamilyName(Font.FontFamily.Name), IsAntialias = true, SubpixelText = true, LcdRenderText = true })
            {
                var metrics = textPaint.FontMetrics;
                float textHeight = metrics.Descent - metrics.Ascent;
                float textY = itemRect.Top + (itemRect.Height - textHeight) / 2f - metrics.Ascent;

                if (item.Text.Contains("&"))
                    canvas.DrawTextWithMnemonic(item.Text, textPaint, textX, textY);
                else
                    canvas.DrawText(item.Text, textX, textY, textPaint);
            }

            if (ShowSubmenuArrow && item.HasDropDown)
            {
                using var arrowPaint = new SKPaint { Color = textColor.ToSKColor().WithAlpha(fadeAlpha), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1.2f, StrokeCap = SKStrokeCap.Round, StrokeJoin = SKStrokeJoin.Round };
                float chevronSize = 5f;
                float chevronX = itemRect.Right - 14;
                float chevronY = itemRect.MidY;
                using var chevronPath = new SKPath();
                chevronPath.MoveTo(chevronX - chevronSize, chevronY - chevronSize);
                chevronPath.LineTo(chevronX - chevronSize / 2, chevronY);
                chevronPath.LineTo(chevronX - chevronSize, chevronY + chevronSize);
                canvas.DrawPath(chevronPath, arrowPaint);
            }

            y += ItemHeight + ItemPadding;
        }
    }
}