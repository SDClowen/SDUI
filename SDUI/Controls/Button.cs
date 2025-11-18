using SDUI.Animation;
using SDUI.Extensions;
using SkiaSharp;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SDUI.Controls;

public class Button : UIElementBase, IButtonControl
{
    [Category("Appearance")]
    public Image Image { get; set; }

    [Category("Appearance")]
    public Color Color { get; set; } = Color.Transparent;

    private int _mouseState = 0;
    private SizeF _textSize;

    [Category("Behavior")]
    public DialogResult DialogResult { get; set; }

    [Category("Behavior")]
    public bool IsDefault { get; set; }

    [Category("Behavior")]
    public bool IsCancel { get; set; }

    private float _shadowDepth = 4f;
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

    private int _radius = 6;
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

    private readonly AnimationManager animationManager;
    private readonly AnimationManager hoverAnimationManager;
    private readonly AnimationManager pressAnimationManager;

    private SKImage _cachedImage;
    private bool _needsRedraw = true;

    private void InvalidateCache()
    {
        _needsRedraw = true;
        _cachedImage?.Dispose();
        _cachedImage = null;
    }

    public Button()
    {
        TabStop = true;

        // Ripple animasyonu - her tıklamada yeni başlar
        animationManager = new AnimationManager(singular: true)
        {
            Increment = 0.05,
            AnimationType = AnimationType.EaseOut,
            InterruptAnimation = true
        };

        // Hover animasyonu - smooth geçişler
        hoverAnimationManager = new AnimationManager(singular: true)
        {
            Increment = 0.10,
            AnimationType = AnimationType.EaseInOut,
            InterruptAnimation = true
        };

        hoverAnimationManager.OnAnimationProgress += sender => Invalidate();
        animationManager.OnAnimationProgress += sender => Invalidate();

        pressAnimationManager = new AnimationManager(singular: true)
        {
            Increment = 0.18,
            AnimationType = AnimationType.EaseInOut,
            InterruptAnimation = true
        };
        pressAnimationManager.OnAnimationProgress += sender => Invalidate();
    }

    internal override void OnTextChanged(EventArgs e)
    {
        InvalidateCache();
        base.OnTextChanged(e);
        using (var paint = new SKPaint())
        {
            paint.TextSize = Font.Size.PtToPx(this);
            paint.Typeface = SKTypeface.FromFamilyName(Font.FontFamily.Name, SKFontStyle.Normal);
            var metrics = paint.FontMetrics;
            _textSize = new SizeF(paint.MeasureText(Text), metrics.Descent - metrics.Ascent);
        }
        if (AutoSize)
            Size = GetPreferredSize(Size.Empty);
    }

    public override void OnClick(EventArgs e)
    {
        if (DialogResult != DialogResult.None && FindForm() is Form form)
        {
            form.DialogResult = DialogResult;
        }
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
        // Tema arkaplanını her frame temizle (siyah görünümleri engeller)
        canvas.Clear(ColorScheme.BackColor.ToSKColor());

        var hoverProgress = (float)hoverAnimationManager.GetProgress();
        var pressProgress = (float)pressAnimationManager.GetProgress();

        var baseRect = new SKRect(0, 0, Width, Height);
        var bodyRect = new SKRect(baseRect.Left, baseRect.Top, baseRect.Right, baseRect.Bottom);
        bodyRect.Inflate(-_shadowDepth / 4f, -_shadowDepth / 4f);

        canvas.Save();

        DrawButton(canvas, bodyRect, hoverProgress, pressProgress);
        DrawAnimations(canvas, bodyRect, hoverProgress, pressProgress);

        canvas.Restore();
    }

    private void DrawButton(SKCanvas canvas, SKRect bodyRect, float hoverProgress, float pressProgress)
    {
        var accentSk = ColorScheme.AccentColor.ToSKColor();
        // Sade tasarım: temel dolgu her zaman tema arkaplan rengi
        var baseSk = Color != Color.Transparent
            ? (Enabled ? Color.ToSKColor() : Color.FromArgb(160, Color).ToSKColor())
            : ColorScheme.BackColor.ToSKColor();

        // Sadelik: dolgu rengi doğrudan temel renk (tema arkaplanı veya kullanıcı rengi)
        var highlightBlend = Math.Clamp(hoverProgress * 0.15f + pressProgress * 0.30f, 0f, 0.4f);
        var fillColor = baseSk;

        if (!Enabled)
        {
            fillColor = fillColor.WithAlpha((byte)(fillColor.Alpha * 0.6f));
        }

        // Kenarlık (gölge yerine sade kenarlık)
        using (var borderPaint = new SKPaint
        {
            Color = ColorScheme.BorderColor.ToSKColor(),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1f
        })
        {
            canvas.DrawRoundRect(bodyRect, _radius, _radius, borderPaint);
        }

        using (var fillPaint = new SKPaint
        {
            Color = fillColor,
            IsAntialias = true,
            FilterQuality = SKFilterQuality.High,
            Style = SKPaintStyle.Fill
        })
        {
            canvas.DrawRoundRect(bodyRect, _radius, _radius, fillPaint);
        }

        // Hover/press parıltısı
        var glowIntensity = Math.Clamp(hoverProgress * 0.4f + pressProgress * 0.3f, 0f, 1f);
        if (glowIntensity > 0)
        {
            using var glowPaint = new SKPaint
            {
                Color = accentSk.WithAlpha((byte)(80 * glowIntensity)),
                IsAntialias = true
            };
            canvas.DrawRoundRect(bodyRect, _radius, _radius, glowPaint);
        }

        // Focus halkası
        if (Focused)
        {
            using var focusPaint = new SKPaint
            {
                Color = accentSk.WithAlpha(180),
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2f
            };
            canvas.DrawRoundRect(bodyRect, _radius, _radius, focusPaint);
        }

        // Geçersiz durum kenarlığı
        if (!IsValid)
        {
            using var invalidPaint = new SKPaint
            {
                Color = Color.Red.ToSKColor(),
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2f
            };
            canvas.DrawRoundRect(bodyRect, _radius, _radius, invalidPaint);
        }

        // İkon çizimi
        float contentStartX = bodyRect.Left + 8f;
        if (Image != null)
        {
            var imageRect = new Rectangle(
                (int)contentStartX,
                (int)(bodyRect.Top + (bodyRect.Height - 24) / 2f),
                24,
                24);

            if (string.IsNullOrEmpty(Text))
                imageRect.X += 2;

            using (var stream = new System.IO.MemoryStream())
            {
                Image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                stream.Position = 0;
                using var skImage = SKImage.FromEncodedData(SKData.Create(stream));
                canvas.DrawImage(skImage, SKRect.Create(imageRect.X, imageRect.Y, imageRect.Width, imageRect.Height));
            }
            contentStartX += 24 + 6f;
        }

        // Metin çizimi
        if (!string.IsNullOrEmpty(Text))
        {
            var textColor = ColorScheme.ForeColor.ToSKColor().InterpolateColor(accentSk, Math.Clamp(highlightBlend * 0.4f + hoverProgress * 0.1f, 0f, 0.6f));
            if (!Enabled)
                textColor = textColor.WithAlpha((byte)(textColor.Alpha * 0.6f));

            using var textPaint = new SKPaint
            {
                TextSize = Font.Size.PtToPx(this),
                Typeface = SKTypeface.FromFamilyName(Font.FontFamily.Name, SKFontStyle.Normal),
                Color = textColor,
                IsAntialias = true,
                SubpixelText = true,
                LcdRenderText = true,
                FilterQuality = SKFilterQuality.High,
                TextAlign = TextAlign == ContentAlignment.MiddleCenter ? SKTextAlign.Center :
                           TextAlign == ContentAlignment.MiddleRight ? SKTextAlign.Right :
                           SKTextAlign.Left
            };

            float availableWidth = bodyRect.Width - (contentStartX - bodyRect.Left) - 8f;
            float x = TextAlign switch
            {
                ContentAlignment.MiddleCenter => bodyRect.MidX,
                ContentAlignment.MiddleRight => bodyRect.Right - 8f,
                _ => contentStartX
            };
            float y = bodyRect.MidY + textPaint.TextSize / 3f;

            if (AutoEllipsis)
            {
                canvas.DrawTextWithEllipsis(Text, textPaint, x, y, availableWidth);
            }
            else if (UseMnemonic)
            {
                canvas.DrawTextWithMnemonic(Text, textPaint, x, y);
            }
            else
            {
                canvas.DrawText(Text, x, y, textPaint);
            }
        }

        // Validasyon mesajı çizimi
        if (!IsValid && !string.IsNullOrEmpty(ValidationText))
        {
            using var validationPaint = new SKPaint
            {
                Color = Color.Red.ToSKColor(),
                IsAntialias = true,
                TextSize = Font.Size,
                Typeface = SKTypeface.FromFamilyName(Font.FontFamily.Name)
            };

            canvas.DrawText(
                ValidationText,
                bodyRect.Left + 5f,
                bodyRect.Bottom + 15f,
                validationPaint);
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
                Color = accentSk.WithAlpha((byte)(Math.Clamp(hoverProgress * 90f, 0f, 120f))),
                IsAntialias = true
            };
            canvas.DrawRoundRect(bodyRect, _radius, _radius, hoverPaint);
        }

        // Ripple efekti
        if (animationManager.IsAnimating())
        {
            for (int i = 0; i < animationManager.GetAnimationCount(); i++)
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
        {
            engine.StartNewAnimation(direction, source.Value);
        }
        else
        {
            engine.StartNewAnimation(direction);
        }
    }

    public override Size GetPreferredSize(Size proposedSize)
    {
        int extra = 16;

        if (Image != null)
            extra += 24 + 4;

        using (var paint = new SKPaint())
        {
            paint.TextSize = Font.Size * 1.5f;
            paint.Typeface = SKTypeface.FromFamilyName(Font.FontFamily.Name);
            _textSize = new SizeF(paint.MeasureText(Text), paint.FontMetrics.Descent - paint.FontMetrics.Ascent);
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

    public void NotifyDefault(bool value)
    {
        //throw new NotImplementedException();
    }

    void IButtonControl.PerformClick()
    {
        PerformClick();
    }
}