using SDUI.Animation;
using SDUI.Extensions;
using SDUI.Helpers;
using SkiaSharp;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SDUI.Controls;

public class ToggleButton : UIElementBase
{
    private readonly AnimationManager animationManager;
    private Point _mouseLocation;
    private int _mouseState;
    private bool _checked;

    private Rectangle LocalRect => new Rectangle(0, 0, Width, Height); // local koordinatlar

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
    protected virtual void OnCheckedChanged(EventArgs e) => CheckedChanged?.Invoke(this, e);

    public ToggleButton()
    {
        MinimumSize = new Size(56, 22);
        animationManager = new()
        {
            AnimationType = AnimationType.EaseInOut,
            Increment = 0.06,
            Singular = true,
            InterruptAnimation = true
        };
        animationManager.OnAnimationProgress += _ => Invalidate();
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
        _mouseLocation = new Point(-1, -1);
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
        if (_mouseState == 2)
        {
            _mouseState = 1;
        }
    }

    public override void OnPaint(SKPaintSurfaceEventArgs e)
    {
        base.OnPaint(e);
        var canvas = e.Surface.Canvas;
        canvas.Clear();

        int toggleSize = Height - 3;
        float radius = Height / 2f - 1;
        var textWidth = 0f;

        if (!string.IsNullOrEmpty(Text))
        {
            using var font = new SKFont
            {
                Size = Font.Size.PtToPx(this),
                Typeface = SDUI.Helpers.FontManager.GetSKTypeface(Font),
                Subpixel = true,
                Edging = SKFontEdging.SubpixelAntialias
            };
            using var textPaint = new SKPaint
            {
                Color = ColorScheme.ForeColor.ToSKColor(),
                IsAntialias = true
            };

            var textBounds = new SKRect();
            font.MeasureText(Text, out textBounds);
            textWidth = textBounds.Width;
            TextRenderingHelper.DrawText(canvas, Text, 0, Height / 2f + textBounds.Height / 2f, font, textPaint);

            canvas.Translate(textWidth + 10, 0);
        }

        var progress = (float)animationManager.GetProgress();
        var toggleWidth = Width - textWidth - 10;

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
            var shadowRect = new SKRect(0, 0, toggleWidth, Height - 1);
            canvas.DrawRoundRect(shadowRect, radius, radius, shadowPaint);
        }

        using (var paint = new SKPaint
        {
            IsAntialias = true,
            FilterQuality = SKFilterQuality.High,
            Style = SKPaintStyle.Fill
        })
        {
            var rect = new SKRect(0, 0, toggleWidth, Height - 1);
            paint.Color = Checked
                ? ColorScheme.BackColor2.ToSKColor().InterpolateColor(ColorScheme.AccentColor.ToSKColor(), progress)
                : ColorScheme.AccentColor.ToSKColor().InterpolateColor(ColorScheme.BackColor2.ToSKColor(), 1 - progress);
            canvas.DrawRoundRect(rect, radius, radius, paint);

            paint.Style = SKPaintStyle.Stroke;
            paint.StrokeWidth = 1;
            paint.Color = Checked
                ? ColorScheme.BorderColor.ToSKColor().InterpolateColor(ColorScheme.AccentColor.ToSKColor(), progress)
                : ColorScheme.AccentColor.ToSKColor().InterpolateColor(ColorScheme.BorderColor.ToSKColor(), 1 - progress);
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
            float padding = 2f;
            float circleRadius = (toggleSize - padding * 2) / 2f;
            float startX = padding + circleRadius;
            float endX = toggleWidth - padding - circleRadius;
            float x = startX + (endX - startX) * progress;
            canvas.DrawCircle(x, Height / 2f, circleRadius, paint);
        }

        if (!string.IsNullOrEmpty(Text))
            canvas.Translate(-(textWidth + 10), 0);
    }
}