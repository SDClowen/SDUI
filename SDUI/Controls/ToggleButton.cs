using SDUI.Animation;
using SDUI.Extensions;
using SkiaSharp;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SDUI.Controls;

public class ToggleButton : SKControl
{
    private readonly AnimationEngine animationManager;
    private Point _mouseLocation;
    private int _mouseState;
    private bool _checked;

    [Browsable(true)]
    public override string Text
    {
        get => base.Text;
        set { }
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
            Invalidate();
        }
    }

    public event EventHandler CheckedChanged;
    protected virtual void OnCheckedChanged(EventArgs e) => CheckedChanged?.Invoke(this, e);

    public ToggleButton()
    {
        SetStyle(
            ControlStyles.UserPaint |
            ControlStyles.SupportsTransparentBackColor |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw, true);

        SetStyle(ControlStyles.FixedHeight | ControlStyles.Selectable, false);

        this.MinimumSize = new Size(56, 22);

        animationManager = new()
        {
            AnimationType = AnimationType.EaseInOut,
            Increment = 0.06
        };

        animationManager.OnAnimationProgress += _ => Invalidate();
    }

    protected override void OnCreateControl()
    {
        base.OnCreateControl();
        if (DesignMode) return;

        _mouseState = 0;
        MouseEnter += (_, _) => _mouseState = 1;
        MouseLeave += (_, _) =>
        {
            _mouseLocation = new Point(-1, -1);
            _mouseState = 0;
        };
        MouseDown += (_, e) =>
        {
            _mouseState = 2;
            if (e.Button == MouseButtons.Left && ClientRectangle.Contains(_mouseLocation))
            {
                Checked = !Checked;
            }
        };
        MouseUp += (_, _) => _mouseState = 1;
        MouseMove += (_, e) =>
        {
            _mouseLocation = e.Location;
            Cursor = ClientRectangle.Contains(_mouseLocation) ? Cursors.Hand : Cursors.Default;
        };

        CheckedChanged += (_, _) => 
        {
            animationManager.StartNewAnimation(Checked ? AnimationDirection.In : AnimationDirection.Out);
        };
    }

    protected override void OnClick(EventArgs e)
    {
        // OnClick'i kaldırıyoruz çünkü MouseDown'da işlemi yapıyoruz
    }

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear();

        // Antialiasing için yüksek kalite ayarı
        canvas.SetMatrix(SKMatrix.CreateScale(1.0f, 1.0f));

        int toggleSize = Height - 3;
        float radius = Height / 2f - 1;
        var textWidth = 0f;

        // Metin çizimi
        if (!string.IsNullOrEmpty(Text))
        {
            using var textPaint = new SKPaint
            {
                Color = ColorScheme.ForeColor.ToSKColor(),
                TextSize = Font.Size.PtToPx(this),
                Typeface = SKTypeface.FromFamilyName(Font.FontFamily.Name),
                IsAntialias = true,
                SubpixelText = true,
                LcdRenderText = true
            };

            var textBounds = new SKRect();
            textPaint.MeasureText(Text, ref textBounds);
            textWidth = textBounds.Width;
            canvas.DrawText(Text, 0, Height / 2f + textBounds.Height / 2f, textPaint);

            canvas.Translate(textWidth + 10, 0);
        }

        var progress = (float)animationManager.GetProgress();
        var toggleWidth = Width - textWidth - 10;

        // Toggle arka planı gölgesi
        using (var shadowPaint = new SKPaint
        {
            Color = SKColors.Black.WithAlpha(20),
            ImageFilter = SKImageFilter.CreateDropShadow(0, 1, 2, 2, SKColors.Black.WithAlpha(20)),
            IsAntialias = true,
            FilterQuality = SKFilterQuality.High,
            Style = SKPaintStyle.Fill
        })
        {
            var shadowRect = new SKRect(0, 0, toggleWidth, Height - 1);
            canvas.DrawRoundRect(shadowRect, radius, radius, shadowPaint);
        }

        // Toggle arka planı
        using (var paint = new SKPaint
        {
            IsAntialias = true,
            FilterQuality = SKFilterQuality.High,
            Style = SKPaintStyle.Fill
        })
        {
            var rect = new SKRect(0, 0, toggleWidth, Height - 1);

            // Arka plan rengi
            if (Checked)
            {
                // Checked durumunda accent color'a animasyonlu geçiş
                paint.Color = ColorScheme.BackColor2.ToSKColor().InterpolateColor(ColorScheme.AccentColor.ToSKColor(), progress);
            }
            else
            {
                // Unchecked durumunda accent color'dan animasyonlu çıkış
                paint.Color = ColorScheme.AccentColor.ToSKColor().InterpolateColor(ColorScheme.BackColor2.ToSKColor(), 1 - progress);
            }
            canvas.DrawRoundRect(rect, radius, radius, paint);

            // Çerçeve
            paint.Style = SKPaintStyle.Stroke;
            paint.StrokeWidth = 1;
            
            if (Checked)
            {
                // Checked durumunda accent color'a animasyonlu geçiş
                paint.Color = ColorScheme.BorderColor.ToSKColor().InterpolateColor(ColorScheme.AccentColor.ToSKColor(), progress);
            }
            else
            {
                // Unchecked durumunda accent color'dan animasyonlu çıkış
                paint.Color = ColorScheme.AccentColor.ToSKColor().InterpolateColor(ColorScheme.BorderColor.ToSKColor(), 1 - progress);
            }
            canvas.DrawRoundRect(rect, radius, radius, paint);
        }

        // Toggle düğmesi - iOS tarzı
        using (var paint = new SKPaint
        {
            Color = SKColors.White,
            IsAntialias = true,
            FilterQuality = SKFilterQuality.High,
            Style = SKPaintStyle.Fill,
            ImageFilter = SKImageFilter.CreateDropShadow(0, 1, 2, 1, SKColors.Black.WithAlpha(40))
        })
        {
            float padding = 2f;
            float circleRadius = (toggleSize - padding * 2) / 2f;
            
            // Başlangıç ve bitiş konumlarını düzelt
            float startX = padding + circleRadius;
            float endX = toggleWidth - padding - circleRadius;
            float x = startX + (endX - startX) * progress;
            
            canvas.DrawCircle(x, Height / 2f, circleRadius, paint);
        }

        if (!string.IsNullOrEmpty(Text))
        {
            canvas.Translate(-(textWidth + 10), 0);
        }
    }

    
}