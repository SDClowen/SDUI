using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using SDUI.Animation;
using SDUI.Extensions;
using SDUI.Helpers;
using SkiaSharp;

namespace SDUI.Controls;

public class Button : UIElementBase, IButtonControl
{
    private readonly AnimationManager animationManager;
    private readonly AnimationManager hoverAnimationManager;
    private readonly AnimationManager pressAnimationManager;

    private SKImage _cachedImage;

    private int _elevation = 1;

    private int _mouseState;
    private bool _needsRedraw = true;

    private int _radius = 12;
    private Point? _rippleCenter;

    private float _shadowDepth = 4f;
    private SizeF _textSize;

    public Button()
    {
        TabStop = true;

        // Ripple animasyonu - her tıklamada yeni başlar
        animationManager = new AnimationManager(true)
        {
            Increment = 0.05,
            AnimationType = AnimationType.EaseOut,
            InterruptAnimation = true
        };

        // Hover animasyonu - smooth geçişler
        hoverAnimationManager = new AnimationManager(true)
        {
            Increment = 0.10,
            AnimationType = AnimationType.EaseInOut,
            InterruptAnimation = true
        };

        hoverAnimationManager.OnAnimationProgress += sender => Invalidate();
        animationManager.OnAnimationProgress += sender => Invalidate();

        pressAnimationManager = new AnimationManager(true)
        {
            Increment = 0.18,
            AnimationType = AnimationType.EaseInOut,
            InterruptAnimation = true
        };
        pressAnimationManager.OnAnimationProgress += sender => Invalidate();
    }

    [Category("Appearance")] public Image Image { get; set; }

    [Category("Appearance")] public Color Color { get; set; } = Color.Transparent;

    [Category("Behavior")] public bool IsDefault { get; set; }

    [Category("Behavior")] public bool IsCancel { get; set; }

    [Category("Appearance")]
    public float ShadowDepth
    {
        get => _shadowDepth;
        set
        {
            if (_shadowDepth == value)
                return;

            _shadowDepth = value;
            Invalidate();
        }
    }

    [Category("Appearance")]
    public int Radius
    {
        get => _radius;
        set
        {
            if (_radius == value)
                return;

            _radius = value;
            Invalidate();
        }
    }

    [Category("Appearance")]
    public int Elevation
    {
        get => _elevation;
        set
        {
            if (_elevation == value)
                return;

            _elevation = Math.Clamp(value, 0, 5);
            Invalidate();
        }
    }

    [Category("Behavior")] public DialogResult DialogResult { get; set; }

    public void NotifyDefault(bool value)
    {
        //throw new NotImplementedException();
    }

    void IButtonControl.PerformClick()
    {
        PerformClick();
    }

    private void InvalidateCache()
    {
        _needsRedraw = true;
        _cachedImage?.Dispose();
        _cachedImage = null;
    }

    internal override void OnTextChanged(EventArgs e)
    {
        InvalidateCache();
        base.OnTextChanged(e);
        using (var paint = new SKPaint())
        {
            paint.TextSize = Font.Size.PtToPx(this);
            paint.Typeface = FontManager.GetSKTypeface(Font);
            var metrics = paint.FontMetrics;
            _textSize = new SizeF(paint.MeasureText(Text), metrics.Descent - metrics.Ascent);
        }

        if (AutoSize)
            Size = GetPreferredSize(Size.Empty);
    }

    public override void OnClick(EventArgs e)
    {
        if (DialogResult != DialogResult.None && FindForm() is Form form) form.DialogResult = DialogResult;
        base.OnClick(e);
    }

    internal override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space)
        {
            PerformClick();
            e.Handled = true;
        }

        base.OnKeyDown(e);
    }

    internal override void OnGotFocus(EventArgs e)
    {
        base.OnGotFocus(e);
        Invalidate();
    }

    internal override void OnLostFocus(EventArgs e)
    {
        base.OnLostFocus(e);
        RestartAnimation(pressAnimationManager, AnimationDirection.Out);
        Invalidate();
    }

    internal override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        _mouseState = 2;
        _rippleCenter = e.Location;
        RestartAnimation(animationManager, AnimationDirection.In, e.Location);
        RestartAnimation(pressAnimationManager, AnimationDirection.In);
        Invalidate();
    }

    internal override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        _mouseState = 1;
        RestartAnimation(pressAnimationManager, AnimationDirection.Out);
        Invalidate();
    }

    internal override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        _mouseState = 1;
        RestartAnimation(hoverAnimationManager, AnimationDirection.In);
        Invalidate();
    }

    internal override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _mouseState = 0;
        RestartAnimation(hoverAnimationManager, AnimationDirection.Out);
        RestartAnimation(pressAnimationManager, AnimationDirection.Out);
        Invalidate();
    }

    public override void OnPaint(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        var hoverProgress = (float)hoverAnimationManager.GetProgress();
        var pressProgress = (float)pressAnimationManager.GetProgress();
        var rippleProgress = (float)animationManager.GetProgress();

        var baseRect = new SKRect(0, 0, Width, Height);

        // Add padding for elevation shadow
        // We use the base elevation for padding to prevent the button from being too small.
        var elevationOffset = ColorScheme.GetElevationOffset(_elevation);
        var elevationBlur = ColorScheme.GetElevationBlur(_elevation);

        // Calculate padding needed for the shadow
        // We add a small buffer (1px) to avoid hard clipping
        var hPadding = Math.Max(elevationOffset, elevationBlur / 2) + 1;
        var vPaddingTop = Math.Max(elevationOffset / 2, elevationBlur / 2) + 1;
        var vPaddingBottom = elevationOffset + elevationBlur / 2 + 1;

        var bodyRect = new SKRect(
            baseRect.Left + hPadding,
            baseRect.Top + vPaddingTop,
            baseRect.Right - hPadding,
            baseRect.Bottom - vPaddingBottom);

        canvas.Save();

        // Calculate dynamic elevation based on state
        var currentElevation = _elevation;
        if (Enabled)
        {
            if (pressProgress > 0) currentElevation += 1;
            else if (hoverProgress > 0) currentElevation += 1;
        }

        currentElevation = Math.Min(currentElevation, 5);

        // Draw elevation shadow
        if (Enabled && currentElevation > 0) ElevationHelper.DrawElevation(canvas, bodyRect, _radius, currentElevation);

        DrawButton(canvas, bodyRect, hoverProgress, pressProgress);

        // Draw ripple effect
        if (rippleProgress > 0 && rippleProgress < 1 && _rippleCenter.HasValue)
        {
            canvas.Save();
            using (var path = new SKPath())
            {
                path.AddRoundRect(bodyRect, _radius, _radius);
                canvas.ClipPath(path, SKClipOperation.Intersect, true);

                ElevationHelper.DrawRipple(
                    canvas,
                    new SKPoint(_rippleCenter.Value.X, _rippleCenter.Value.Y),
                    Math.Max(bodyRect.Width, bodyRect.Height) * rippleProgress,
                    rippleProgress,
                    ColorScheme.Primary.Alpha(100));
            }

            canvas.Restore();
        }

        canvas.Restore();
    }

    private void DrawButton(SKCanvas canvas, SKRect bodyRect, float hoverProgress, float pressProgress)
    {
        // Determine base color - use Primary for filled buttons
        var fillColor = Color != Color.Transparent
            ? Color.ToSKColor()
            : ColorScheme.Primary.ToSKColor();

        if (!Enabled)
            // Material Design 3 Disabled State: OnSurface 12%
            fillColor = ColorScheme.OnSurface.Alpha(30).ToSKColor();

        // Draw base fill
        using (var fillPaint = new SKPaint
               {
                   Color = fillColor,
                   IsAntialias = true,
                   Style = SKPaintStyle.Fill
               })
        {
            canvas.DrawRoundRect(bodyRect, _radius, _radius, fillPaint);
        }

        // Draw state layer (hover/press)
        if (Enabled)
        {
            var stateLayerColor = Color.Transparent;
            if (pressProgress > 0)
                stateLayerColor = ColorScheme.StateLayerPressed;
            else if (hoverProgress > 0)
                stateLayerColor = ColorScheme.StateLayerHover;

            if (stateLayerColor.A > 0)
            {
                using var statePaint = new SKPaint
                {
                    Color = stateLayerColor.ToSKColor(),
                    IsAntialias = true,
                    Style = SKPaintStyle.Fill
                };
                canvas.DrawRoundRect(bodyRect, _radius, _radius, statePaint);
            }
        }

        // Draw focus indicator
        if (Focused)
        {
            using var focusPaint = new SKPaint
            {
                Color = ColorScheme.OnPrimary.Alpha(90).ToSKColor(),
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1.5f
            };
            var focusRect = bodyRect;
            focusRect.Inflate(2, 2);
            canvas.DrawRoundRect(focusRect, _radius + 1, _radius + 1, focusPaint);
        }

        // Icon drawing
        var contentStartX = bodyRect.Left + 16f;
        if (Image != null)
        {
            var imageSize = 20;
            var imageRect = new Rectangle(
                (int)contentStartX,
                (int)(bodyRect.Top + (bodyRect.Height - imageSize) / 2f),
                imageSize,
                imageSize);

            if (string.IsNullOrEmpty(Text))
                imageRect.X = (int)(bodyRect.Left + (bodyRect.Width - imageSize) / 2f);

            using (var stream = new MemoryStream())
            {
                Image.Save(stream, ImageFormat.Png);
                stream.Position = 0;
                using var skImage = SKImage.FromEncodedData(SKData.Create(stream));
                canvas.DrawImage(skImage, SKRect.Create(imageRect.X, imageRect.Y, imageRect.Width, imageRect.Height));
            }

            contentStartX += imageSize + 8f;
        }

        // Text drawing with OnPrimary color
        if (!string.IsNullOrEmpty(Text))
        {
            var textColor = Color != Color.Transparent
                ? ColorScheme.OnPrimary.ToSKColor()
                : ColorScheme.OnPrimary.ToSKColor();

            if (!Enabled)
                textColor = ColorScheme.OnSurface.Alpha(80).ToSKColor();

            using var textPaint = new SKPaint
            {
                Color = textColor,
                IsAntialias = true
            };

            using var font = new SKFont
            {
                Size = Font.Size.PtToPx(this),
                Typeface = FontManager.GetSKTypeface(Font),
                Subpixel = true,
                Edging = SKFontEdging.SubpixelAntialias
            };

            var textWidth = font.MeasureText(Text);
            var textX = Image == null
                ? bodyRect.Left + (bodyRect.Width - textWidth) / 2f
                : contentStartX;
            var textY = bodyRect.Top + (bodyRect.Height + font.Metrics.CapHeight) / 2f;

            canvas.DrawText(Text, textX, textY, font, textPaint);
        }
    }

    private void DrawAnimations(SKCanvas canvas, SKRect bodyRect, float hoverProgress, float pressProgress)
    {
        var accentSk = ColorScheme.AccentColor.ToSKColor();

        // Hover parıltısı
        if (hoverProgress > 0f)
        {
            using var hoverPaint = new SKPaint
            {
                Color = accentSk.WithAlpha((byte)Math.Clamp(hoverProgress * 90f, 0f, 120f)),
                IsAntialias = true
            };
            canvas.DrawRoundRect(bodyRect, _radius, _radius, hoverPaint);
        }

        // Ripple efekti
        if (animationManager.IsAnimating())
            for (var i = 0; i < animationManager.GetAnimationCount(); i++)
            {
                var animationValue = animationManager.GetProgress(i);
                var animationSource = animationManager.GetSource(i);

                var alpha = (byte)Math.Clamp((1.0 - animationValue) * (140 + pressProgress * 90f), 0, 255);
                using var ripplePaint = new SKPaint
                {
                    Color = accentSk.WithAlpha(alpha),
                    IsAntialias = true,
                    FilterQuality = SKFilterQuality.High,
                    Style = SKPaintStyle.Fill
                };

                var rippleSize = (float)(animationValue * Math.Max(Width, Height) * 2.2f);
                var rippleRect = new SKRect(
                    animationSource.X - rippleSize / 2f,
                    animationSource.Y - rippleSize / 2f,
                    animationSource.X + rippleSize / 2f,
                    animationSource.Y + rippleSize / 2f
                );

                canvas.Save();
                canvas.ClipRoundRect(new SKRoundRect(bodyRect, _radius, _radius), SKClipOperation.Intersect, true);
                canvas.DrawOval(rippleRect, ripplePaint);
                canvas.Restore();
            }
    }

    private static void RestartAnimation(AnimationManager engine, AnimationDirection direction, Point? source = null)
    {
        if (engine == null)
            return;

        engine.SetDirection(direction);

        var startProgress = direction switch
        {
            AnimationDirection.Out or AnimationDirection.InOutOut or AnimationDirection.InOutRepeatingOut => 1.0,
            _ => 0.0
        };

        engine.SetProgress(startProgress);

        if (source.HasValue)
            engine.StartNewAnimation(direction, source.Value);
        else
            engine.StartNewAnimation(direction);
    }

    public override Size GetPreferredSize(Size proposedSize)
    {
        var extra = 16;

        if (Image != null)
            extra += 24 + 4;

        using (var font = new SKFont())
        {
            font.Size = Font.Size * 1.5f;
            font.Typeface = FontManager.GetSKTypeface(Font);
            _textSize = new SizeF(font.MeasureText(Text), font.Metrics.Descent - font.Metrics.Ascent);
        }

        return new Size((int)Math.Ceiling(_textSize.Width) + extra, 32);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _cachedImage?.Dispose();
            _cachedImage = null;
        }

        base.Dispose(disposing);
    }

    internal override void OnEnabledChanged(EventArgs e)
    {
        InvalidateCache();
        base.OnEnabledChanged(e);
    }

    internal override void OnBackColorChanged(EventArgs e)
    {
        InvalidateCache();
        base.OnBackColorChanged(e);
    }

    internal override void OnForeColorChanged(EventArgs e)
    {
        InvalidateCache();
        base.OnForeColorChanged(e);
    }

    internal override void OnSizeChanged(EventArgs e)
    {
        InvalidateCache();
        base.OnSizeChanged(e);
    }
}