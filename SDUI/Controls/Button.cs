using SDUI.Animation;
using SDUI.Extensions;
using SkiaSharp;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SDUI.Controls;

public class Button : SKControl
{
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

    private readonly Animation.AnimationEngine animationManager;
    private readonly Animation.AnimationEngine hoverAnimationManager;

    public Button()
    {
        SetStyle(ControlStyles.Selectable, true);
        TabStop = true;

        animationManager = new Animation.AnimationEngine(false)
        {
            Increment = 0.03,
            AnimationType = AnimationType.EaseOut
        };

        hoverAnimationManager = new Animation.AnimationEngine
        {
            Increment = 0.07,
            AnimationType = AnimationType.Linear
        };

        hoverAnimationManager.OnAnimationProgress += sender => Invalidate();
        animationManager.OnAnimationProgress += sender => Invalidate();
    }

    protected override void OnTextChanged(EventArgs e)
    {
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

    protected override void OnClick(EventArgs e)
    {
        if (DialogResult != DialogResult.None && FindForm() is Form form)
        {
            form.DialogResult = DialogResult;
        }
        base.OnClick(e);
    }

    public void PerformClick()
    {
        if (CanSelect)
            OnClick(EventArgs.Empty);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space)
        {
            PerformClick();
            e.Handled = true;
        }
        base.OnKeyDown(e);
    }

    protected override void OnGotFocus(EventArgs e)
    {
        base.OnGotFocus(e);
        Invalidate();
    }

    protected override void OnLostFocus(EventArgs e)
    {
        base.OnLostFocus(e);
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        _mouseState = 2;
        animationManager.StartNewAnimation(AnimationDirection.In, e.Location);
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        _mouseState = 1;
        Invalidate();
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        _mouseState = 1;
        hoverAnimationManager.StartNewAnimation(AnimationDirection.In);
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _mouseState = 0;
        hoverAnimationManager.StartNewAnimation(AnimationDirection.Out);
        Invalidate();
    }

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear();

        var rect = new SKRect(0, 0, Width, Height);
        var inflate = _shadowDepth / 4f;
        rect.Inflate(-inflate, -inflate);

        // Ana renk ayarları
        var color = Color.Empty;
        if (Color != Color.Transparent)
            color = Enabled ? Color : Color.FromArgb(200, Color);
        else
            color = Color.FromArgb(20, ColorScheme.ForeColor);

        var paint = new SKPaint
        {
            Color = color.ToSKColor(),
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };

        // Gölge çizimi
        using (var shadowPaint = new SKPaint
        {
            Color = SKColors.Black.WithAlpha(60),
            ImageFilter = SKImageFilter.CreateDropShadow(
                _shadowDepth,
                _shadowDepth,
                3,
                3,
                SKColors.Black.WithAlpha(60)
            ),
            IsAntialias = true
        })
        {
            canvas.DrawRoundRect(rect, _radius, _radius, shadowPaint);
        }

        // Ana buton çizimi
        canvas.DrawRoundRect(rect, _radius, _radius, paint);

        // Hover efekti
        var hoverProgress = hoverAnimationManager.GetProgress();
        if (hoverProgress > 0)
        {
            using var hoverPaint = new SKPaint
            {
                Color = color.ToSKColor().WithAlpha((byte)(hoverProgress * 65)),
                IsAntialias = true
            };
            canvas.DrawRoundRect(rect, _radius, _radius, hoverPaint);
        }

        // Ripple efekti
        if (animationManager.IsAnimating())
        {
            for (int i = 0; i < animationManager.GetAnimationCount(); i++)
            {
                var animationValue = animationManager.GetProgress(i);
                var animationSource = animationManager.GetSource(i);

                using var ripplePaint = new SKPaint
                {
                    Color = ColorScheme.BackColor.ToSKColor().WithAlpha((byte)(101 - (animationValue * 100))),
                    IsAntialias = true
                };

                var rippleSize = (float)(animationValue * Width * 2.0);
                var rippleRect = new SKRect(
                    animationSource.X - rippleSize / 2,
                    animationSource.Y - rippleSize / 2,
                    animationSource.X + rippleSize / 2,
                    animationSource.Y + rippleSize / 2
                );

                canvas.DrawOval(rippleRect, ripplePaint);
            }
        }

        // İkon çizimi
        if (Image != null)
        {
            var imageRect = new Rectangle(8, (Height - 24) / 2, 24, 24);
            if (string.IsNullOrEmpty(Text))
                imageRect.X += 2;

            using (var stream = new System.IO.MemoryStream())
            {
                Image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                stream.Position = 0;
                using var skImage = SKImage.FromEncodedData(SKData.Create(stream));
                canvas.DrawImage(skImage, SKRect.Create(imageRect.X, imageRect.Y, imageRect.Width, imageRect.Height));
            }
        }

        // Text çizimi
        if (!string.IsNullOrEmpty(Text))
        {
            var foreColor = Color == Color.Transparent ? ColorScheme.ForeColor : ForeColor;
            if (!Enabled)
                foreColor = Color.Gray;

            using var textPaint = canvas.CreateTextPaint(Font, foreColor, this, TextAlign);
            var x = textPaint.GetTextX(Width, textPaint.MeasureText(Text), TextAlign, Image != null);
            var y = textPaint.GetTextY(Height, TextAlign);

            if (AutoEllipsis)
            {
                var maxWidth = Width - (Image != null ? 40 : 16);
                canvas.DrawTextWithEllipsis(Text, textPaint, x, y, maxWidth);
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
    }

    public override Size GetPreferredSize(Size proposedSize)
    {
        int extra = 16;

        if (Image != null)
            extra += 24 + 4;

        return new Size((int)Math.Ceiling(_textSize.Width) + extra, 32);
    }
}