using SDUI.Extensions;
using SkiaSharp;
using System.Drawing;
using System.Windows.Forms;

namespace SDUI.Controls;

public class GroupBox : UIElementBase
{
    private int _shadowDepth = 4;
    private int _radius = 10;
    private ContentAlignment _textAlign = ContentAlignment.MiddleCenter;

    public ContentAlignment TextAlign
    {
        get => _textAlign;
        set
        {
            if (_textAlign == value) return;
            _textAlign = value;
            Invalidate();
        }
    }

    public int ShadowDepth
    {
        get => _shadowDepth;
        set
        {
            if (_shadowDepth == value) return;
            _shadowDepth = value;
            Invalidate();
        }
    }

    public int Radius
    {
        get => _radius;
        set
        {
            _radius = value;
            Invalidate();
        }
    }

    public GroupBox()
    {
        this.BackColor = Color.Transparent;
        this.Padding = new Padding(3, 8, 3, 3);
    }

    public override void OnPaint(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear();

        // Debug çerçevesi
        if (ColorScheme.DrawDebugBorders)
        {
            using var paint = new SKPaint
            {
                Color = SKColors.Red,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1
            };
            canvas.DrawRect(0, 0, Width - 1, Height - 1, paint);
        }

        var rect = new SKRect(0, 0, Width, Height);
        var inflate = _shadowDepth / 4f;
        rect.Inflate(-inflate, -inflate);
        var shadowRect = rect;

        // Başlık alanı için rect
        var titleRect = new SKRect(0, 0, rect.Width, Font.Height + 7);

        // Gölge çizimi
        using (var paint = new SKPaint
        {
            Color = SKColors.Black.WithAlpha(20),
            IsAntialias = true,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, _shadowDepth / 2f)
        })
        {
            canvas.DrawRoundRect(shadowRect, _radius, _radius, paint);
        }

        // Arka plan çizimi
        using (var paint = new SKPaint
        {
            Color = ColorScheme.BackColor2.ToSKColor(),
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        })
        {
            canvas.DrawRoundRect(rect, _radius, _radius, paint);
        }

        // Başlık alanı çizimi
        canvas.Save();
        canvas.ClipRect(titleRect);

        // Başlık çizgisi
        using (var paint = new SKPaint
        {
            Color = ColorScheme.BorderColor.ToSKColor(),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1
        })
        {
            canvas.DrawLine(0, titleRect.Height - 1, titleRect.Width, titleRect.Height - 1, paint);
        }

        // Başlık arka plan
        using (var paint = new SKPaint
        {
            Color = ColorScheme.BackColor2.ToSKColor().WithAlpha(15),
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        })
        {
            canvas.DrawRoundRect(rect, _radius, _radius, paint);
        }

        canvas.Restore();

        // Başlık metni çizimi
        if (!string.IsNullOrEmpty(Text))
        {
            using var textPaint = new SKPaint
            {
                Color = ColorScheme.ForeColor.ToSKColor(),
                TextSize = Font.Size.PtToPx(this),
                Typeface = SKTypeface.FromFamilyName(Font.FontFamily.Name),
                IsAntialias = true
            };

            var textWidth = textPaint.MeasureText(Text);
            var textHeight = textPaint.FontMetrics.XHeight;
            float textX;
            float textY = titleRect.Height / 2f + textHeight / 2f;

            switch (TextAlign)
            {
                case ContentAlignment.MiddleLeft:
                    textX = rect.Left + Padding.Left;
                    break;
                case ContentAlignment.MiddleRight:
                    textX = rect.Right - textWidth - Padding.Right;
                    break;
                case ContentAlignment.MiddleCenter:
                default:
                    textX = rect.Left + (rect.Width - textWidth) / 2f;
                    break;
            }

            canvas.DrawText(Text, textX, textY, textPaint);
        }

        // Çerçeve çizimi
        using (var paint = new SKPaint
        {
            Color = ColorScheme.BorderColor.ToSKColor(),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1
        })
        {
            canvas.DrawRoundRect(rect, _radius, _radius, paint);
        }
    }

    public override Size GetPreferredSize(Size proposedSize)
    {
        var preferredSize = base.GetPreferredSize(proposedSize);
        preferredSize.Width += _shadowDepth;
        preferredSize.Height += _shadowDepth;

        return preferredSize;
    }
}