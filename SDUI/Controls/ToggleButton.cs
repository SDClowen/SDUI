using System;
using System.ComponentModel;

using System.Windows.Forms;
using SDUI.Animation;
using SDUI.Extensions;
using SDUI.Helpers;
using SkiaSharp;

namespace SDUI.Controls;

public class ToggleButton : UIElementBase
{
    private readonly AnimationManager animationManager;
    private bool _checked;
    private SKPoint _mouseLocation;
    private int _mouseState;

    public ToggleButton()
    {
        MinimumSize = new SKSize(56, 22);
        animationManager = new AnimationManager
        {
            AnimationType = AnimationType.EaseInOut,
            Increment = 0.06,
            Singular = true,
            InterruptAnimation = true
        };
        animationManager.OnAnimationProgress += _ => Invalidate();
    }

    private SkiaSharp.SKRect LocalRect => new(0, 0, Width, Height); // local koordinatlar

    [Browsable(true)]
    public override string Text
    {
        get => base.Text;
        set => base.Text = value; // setter artık metni güncelliyor
    }

    [Category("Behavior")]
    public bool Checked
    {
        get => _checked;
        set
        {
            if (_checked == value) return;
            _checked = value;
            OnCheckedChanged(EventArgs.Empty);
            animationManager.StartNewAnimation(_checked ? AnimationDirection.In : AnimationDirection.Out);
            Invalidate();
        }
    }

    public event EventHandler CheckedChanged;

    protected virtual void OnCheckedChanged(EventArgs e)
    {
        CheckedChanged?.Invoke(this, e);
    }

    // UIElementBase olayları Windows Forms standart eventleri yayınlamıyor olabilir.
    // Bu yüzden etkileşimleri override internal metotlarla ele alıyoruz.
    internal override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        _mouseState = 1;
        Invalidate();
    }

    internal override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _mouseLocation = new SKPoint(-1, -1);
        _mouseState = 0;
        Invalidate();
    }

    internal override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        _mouseLocation = e.Location;
        Cursor = LocalRect.Contains(_mouseLocation) ? Cursors.Hand : Cursors.Default;
    }

    internal override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button == MouseButtons.Left && LocalRect.Contains(e.Location))
        {
            _mouseState = 2;
            Checked = !Checked;
        }
    }

    internal override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (_mouseState == 2) _mouseState = 1;
    }

    public override void OnPaint(SKCanvas canvas)
    {
        base.OnPaint(canvas);

        var spacing = 10f * ScaleFactor;
        var toggleSize = Height - 3f * ScaleFactor;
        var radius = Height / 2f - 1;
        var textWidth = 0f;

        if (!string.IsNullOrEmpty(Text))
        {
            using var font = new SKFont
            {
                Size = Font.Size.Topx(this),
                Typeface = FontManager.GetSKTypeface(Font),
                Subpixel = true,
                Edging = SKFontEdging.SubpixelAntialias
            };
            using var textPaint = new SKPaint
            {
                Color = ColorScheme.ForeColor,
                IsAntialias = true
            };

            var textBounds = new SkiaSharp.SKRect();
            font.MeasureText(Text, out textBounds);
            textWidth = textBounds.Width;
            TextRenderingHelper.DrawText(canvas, Text, 0, Height / 2f + textBounds.Height / 2f, font, textPaint);

            canvas.Translate(textWidth + spacing, 0);
        }

        var progress = (float)animationManager.GetProgress();
        var toggleWidth = Width - textWidth - spacing;

        using (var shadowFilter = SKImageFilter.CreateDropShadow(0, 1, 2, 2, SKColors.Black.WithAlpha(20)))
        using (var shadowPaint = new SKPaint
               {
                   Color = SKColors.Black.WithAlpha(20),
                   ImageFilter = shadowFilter,
                   IsAntialias = true,
                   FilterQuality = SKFilterQuality.High,
                   Style = SKPaintStyle.Fill
               })
        {
            var shadowRect = new SkiaSharp.SKRect(0, 0, toggleWidth, Height - 1);
            canvas.DrawRoundRect(shadowRect, radius, radius, shadowPaint);
        }

        using (var paint = new SKPaint
               {
                   IsAntialias = true,
                   FilterQuality = SKFilterQuality.High,
                   Style = SKPaintStyle.Fill
               })
        {
            var rect = new SkiaSharp.SKRect(0, 0, toggleWidth, Height - 1);
            paint.Color = Checked
                ? ColorScheme.BackColor2.InterpolateColor(ColorScheme.AccentColor, progress)
                : ColorScheme.AccentColor
                    .InterpolateColor(ColorScheme.BackColor2, 1 - progress);
            canvas.DrawRoundRect(rect, radius, radius, paint);

            paint.Style = SKPaintStyle.Stroke;
            paint.StrokeWidth = 1;
            paint.Color = Checked
                ? ColorScheme.BorderColor.InterpolateColor(ColorScheme.AccentColor, progress)
                : ColorScheme.AccentColor
                    .InterpolateColor(ColorScheme.BorderColor, 1 - progress);
            canvas.DrawRoundRect(rect, radius, radius, paint);
        }

        using (var thumbShadowFilter = SKImageFilter.CreateDropShadow(0, 1, 2, 1, SKColors.Black.WithAlpha(40)))
        using (var paint = new SKPaint
               {
                   Color = SKColors.White,
                   IsAntialias = true,
                   FilterQuality = SKFilterQuality.High,
                   Style = SKPaintStyle.Fill,
                   ImageFilter = thumbShadowFilter
               })
        {
            var padding = 2f * ScaleFactor;
            var circleRadius = (toggleSize - padding * 2) / 2f;
            var startX = padding + circleRadius;
            var endX = toggleWidth - padding - circleRadius;
            var x = startX + (endX - startX) * progress;
            canvas.DrawCircle(x, Height / 2f, circleRadius, paint);
        }

        if (!string.IsNullOrEmpty(Text))
            canvas.Translate(-(textWidth + spacing), 0);
    }
}